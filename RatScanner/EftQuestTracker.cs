using Newtonsoft.Json.Linq;
using RatScanner.TarkovDev.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <summary>
/// Reads quest state out of the notifications log and records it in
/// <see cref="LocalProgressStore"/>, so a task the player finished is already
/// marked without being ticked by hand.
/// </summary>
internal static class EftQuestTracker {
	// Numeric message types from the game's chat message shape. Only the task
	// range is interesting here; the rest are ignored.
	private const int PlayerMessage = 1;
	private const int TaskStarted = 10;
	private const int TaskFailed = 11;
	private const int TaskFinished = 12;

	/// <summary>
	/// Raised on a background thread for every task the game reports, so the UI
	/// can notify and open it. The <see cref="TarkovTask"/> is null when the
	/// catalogue does not know the id, in which case only progress was recorded.
	/// </summary>
	internal static event Action<TarkovTask?, string, Logger.NotifyLevel>? TaskReported;

	/// <summary>
	/// Set while reading a past session's logs. Progress is still recorded, but
	/// nothing is announced or opened: history is not news.
	/// </summary>
	internal static bool Replaying { get; set; }

	internal static void Consume(string chunk) {
		if (string.IsNullOrEmpty(chunk)) return;

		MatchCollection lines;
		try {
			lines = EftLogLine.Matches(chunk);
		} catch (Exception e) {
			Logger.LogWarning($"Could not parse the EFT notifications log: {e.Message}");
			return;
		}

		foreach (Match line in lines) {
			try {
				var message = line.Groups["message"].Value;
				if (!message.Contains("ChatMessageReceived", StringComparison.OrdinalIgnoreCase)) continue;
				if (!line.Groups["json"].Success) continue;

				Handle(line.Groups["json"].Value);
			} catch (Exception e) {
				Logger.LogWarning($"Skipped an unreadable quest log line: {e.Message}");
			}
		}
	}

	private static void Handle(string json) {
		var node = JObject.Parse(json);
		var payload = node["message"];
		if (payload == null) return;

		var type = ReadInt(payload["type"]);
		if (type == PlayerMessage) return;
		if (type < TaskStarted || type > TaskFinished) return;

		var templateId = ReadString(payload["templateId"]);
		if (string.IsNullOrEmpty(templateId)) return;

		// The template is "<task id> <something>"; the id is what we key on.
		var taskId = templateId.Split(' ')[0].Trim();
		if (string.IsNullOrEmpty(taskId)) return;

		switch (type) {
			case TaskStarted:
				StartTask(taskId);
				break;
			case TaskFinished:
				CompleteTask(taskId);
				break;
			case TaskFailed:
				FailTask(taskId);
				break;
		}
	}

	/// <summary>
	/// Announces an accepted task. Nothing is written to progress: accepting a
	/// task says nothing about its objectives.
	/// </summary>
	private static void StartTask(string taskId) {
		var task = TaskFor(taskId);

		Announce(task,
			LocalizationService.Format("LogTrackingQuestAccepted", NameOf(task, taskId)),
			Logger.NotifyLevel.Info);
	}

	private static void CompleteTask(string taskId) {
		var task = TaskFor(taskId);

		// The objectives are completed as well, because a task being done implies
		// them, and the UI counts objectives separately from the task.
		foreach (var objective in task?.Objectives ?? []) {
			if (string.IsNullOrEmpty(objective.Id)) continue;
			Item.SetLocalObjectiveComplete(objective.Id, true, objective.Count);
		}

		Item.SetLocalTaskComplete(taskId, true);

		Announce(task,
			LocalizationService.Format("LogTrackingQuestFinished", NameOf(task, taskId)),
			Logger.NotifyLevel.Success);
	}

	private static void FailTask(string taskId) {
		var task = TaskFor(taskId);

		// A failed task's objectives are not necessarily all reset, so only the
		// task flag is set here.
		Item.SetLocalTaskFailed(taskId, true);

		Announce(task,
			LocalizationService.Format("LogTrackingQuestFailed", NameOf(task, taskId)),
			Logger.NotifyLevel.Warning);
	}

	/// <summary>
	/// Reports a task to whoever is listening, and to the log either way so a
	/// tracked event is never silent.
	/// </summary>
	private static void Announce(TarkovTask? task, string message, Logger.NotifyLevel level) {
		// A replayed session still writes progress, but stays quiet and leaves the
		// selection alone.
		if (Replaying) {
			Logger.LogInfo(message);
			return;
		}

		var handlers = TaskReported;
		if (handlers != null) {
			foreach (var handler in handlers.GetInvocationList()) {
				try {
					((Action<TarkovTask?, string, Logger.NotifyLevel>)handler)(task, message, level);
				} catch (Exception e) {
					Logger.LogWarning($"A quest listener threw: {e.Message}");
				}
			}
			return;
		}

		Logger.Notify(level, message);
	}

	/// <summary>
	/// The task for an id, or null when the catalogue does not carry it. The task
	/// is still recorded in that case, since the completion is what matters and an
	/// unknown id may just be a task the catalogue lacks.
	/// </summary>
	private static TarkovTask? TaskFor(string taskId) {
		try {
			return TarkovDevAPI.GetTasks().FirstOrDefault(t => t.Id == taskId);
		} catch (Exception e) {
			Logger.LogWarning($"Could not look up task {taskId}: {e.Message}");
			return null;
		}
	}

	private static string NameOf(TarkovTask? task, string taskId) => task?.Name ?? taskId;

	private static int ReadInt(JToken? token) {
		if (token == null) return 0;
		if (token.Type == JTokenType.Integer) return token.Value<int>();
		return int.TryParse(token.ToString(), out var value) ? value : 0;
	}

	private static string ReadString(JToken? token) => token?.ToString() ?? "";
}
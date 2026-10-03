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

		if (type == TaskFinished) {
			CompleteTask(taskId);
		} else if (type == TaskFailed) {
			FailTask(taskId);
		}
	}

	private static void CompleteTask(string taskId) {
		// The objectives are completed as well, because a task being done implies
		// them, and the UI counts objectives separately from the task.
		foreach (var objective in ObjectivesFor(taskId)) {
			if (string.IsNullOrEmpty(objective.Id)) continue;
			Item.SetLocalObjectiveComplete(objective.Id, true, objective.Count);
		}

		Item.SetLocalTaskComplete(taskId, true);
		Logger.LogInfo($"EFT reported task {NameFor(taskId)} finished.");
	}

	private static void FailTask(string taskId) {
		// A failed task's objectives are not necessarily all reset, so only the
		// task flag is set here.
		Item.SetLocalTaskFailed(taskId, true);
		Logger.LogInfo($"EFT reported task {NameFor(taskId)} failed.");
	}

	/// <summary>
	/// Objectives of a task, or nothing when the catalogue does not know the task.
	/// The task itself is still recorded in that case, since the completion is
	/// what matters and an unknown id may just be a task the catalogue lacks.
	/// </summary>
	private static TaskObjective[] ObjectivesFor(string taskId) {
		try {
			return TarkovDevAPI.GetTasks().FirstOrDefault(t => t.Id == taskId)?.Objectives?.ToArray() ?? [];
		} catch (Exception e) {
			Logger.LogWarning($"Could not look up the objectives of task {taskId}: {e.Message}");
			return [];
		}
	}

	private static string NameFor(string taskId) {
		try {
			return TarkovDevAPI.GetTasks().FirstOrDefault(t => t.Id == taskId)?.Name ?? taskId;
		} catch {
			return taskId;
		}
	}

	private static int ReadInt(JToken? token) {
		if (token == null) return 0;
		if (token.Type == JTokenType.Integer) return token.Value<int>();
		return int.TryParse(token.ToString(), out var value) ? value : 0;
	}

	private static string ReadString(JToken? token) => token?.ToString() ?? "";
}
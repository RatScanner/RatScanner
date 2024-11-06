namespace RatScanner;
public static class Constants {
	public static class Icon {
		public const string Patreon = "<svg style=\"width:24px;height:24px\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M14.82 2.41C18.78 2.41 22 5.65 22 9.62C22 13.58 18.78 16.8 14.82 16.8C10.85 16.8 7.61 13.58 7.61 9.62C7.61 5.65 10.85 2.41 14.82 2.41M2 21.6H5.5V2.41H2V21.6Z\" /></svg>";
		public const string Discord = "<svg style=\"width:24px;height:24px\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M22,24L16.75,19L17.38,21H4.5A2.5,2.5 0 0,1 2,18.5V3.5A2.5,2.5 0 0,1 4.5,1H19.5A2.5,2.5 0 0,1 22,3.5V24M12,6.8C9.32,6.8 7.44,7.95 7.44,7.95C8.47,7.03 10.27,6.5 10.27,6.5L10.1,6.33C8.41,6.36 6.88,7.53 6.88,7.53C5.16,11.12 5.27,14.22 5.27,14.22C6.67,16.03 8.75,15.9 8.75,15.9L9.46,15C8.21,14.73 7.42,13.62 7.42,13.62C7.42,13.62 9.3,14.9 12,14.9C14.7,14.9 16.58,13.62 16.58,13.62C16.58,13.62 15.79,14.73 14.54,15L15.25,15.9C15.25,15.9 17.33,16.03 18.73,14.22C18.73,14.22 18.84,11.12 17.12,7.53C17.12,7.53 15.59,6.36 13.9,6.33L13.73,6.5C13.73,6.5 15.53,7.03 16.56,7.95C16.56,7.95 14.68,6.8 12,6.8M9.93,10.59C10.58,10.59 11.11,11.16 11.1,11.86C11.1,12.55 10.58,13.13 9.93,13.13C9.29,13.13 8.77,12.55 8.77,11.86C8.77,11.16 9.28,10.59 9.93,10.59M14.1,10.59C14.75,10.59 15.27,11.16 15.27,11.86C15.27,12.55 14.75,13.13 14.1,13.13C13.46,13.13 12.94,12.55 12.94,11.86C12.94,11.16 13.45,10.59 14.1,10.59Z\" /></svg>";
		public const string TarkovDev = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 62.3669 54.0232""><defs><style>.a{fill:#988a66;stroke:#1d1d1b;stroke-miterlimit:10;stroke-width:0.01px;}</style></defs><path class=""a"" d=""M24.2535,54.0116h-8.66c-1.0875-1.8775-8.1553-14.1034-10.39-18,2.24-3.8957,9.4148-16.32,10.39-18h8.66c-3.4163,5.9187-6.9759,12.0768-10.39,18Z""/><path class=""a"" d=""M51.1012,46.5131l-4.33,7.5c-2.17.0031-16.2915.011-20.7834-.0019-2.2539-3.8876-9.4261-16.3135-10.3935-17.9981l4.33-7.5c3.4177,5.918,6.9709,12.08,10.3935,17.998Z""/><path class=""a"" d=""M58.0312,19.5131l4.33,7.5C61.279,28.8935,54.2249,41.1273,51.9677,45.011c-4.4936.0081-18.8408.0065-20.7834.0019l-4.33-7.5c6.834-.0008,13.9468.0029,20.7835-.002Z""/><path class=""a"" d=""M38.1135.0116h8.66c1.0875,1.8775,8.1553,14.1034,10.39,18-2.24,3.8957-9.4148,16.32-10.39,18h-8.66c3.4162-5.9188,6.9758-12.0768,10.39-18Z""/><path class=""a"" d=""M11.2658,7.51l4.33-7.5c2.17-.0031,16.2915-.011,20.7835.002C38.6332,3.9,45.8053,16.3256,46.7727,18.01l-4.33,7.5C39.025,19.5921,35.4718,13.43,32.0492,7.512Z""/><path class=""a"" d=""M4.3358,34.51l-4.33-7.5C1.088,25.13,8.142,12.8958,10.3992,9.0122,14.8929,9.004,29.24,9.0057,31.1827,9.01l4.33,7.5c-6.8339.0008-13.9467-.0028-20.7835.002Z""/></svg>";

		public static class TaskObjective {
			public static string FromType(string? type) => type switch {
				"key" => Key,
				"shoot" => Shoot,
				"giveItem" => GiveItem,
				"findItem" => FindItem,
				"findQuestItem" => FindQuestItem,
				"giveQuestItem" => GiveQuestItem,
				"plantQuestItem" => PlantQuestItem,
				"plantItem" => PlantItem,
				"taskStatus" => TaskStatus,
				"extract" => Extract,
				"mark" => Mark,
				"place" => Place,
				"traderLevel" => TraderLevel,
				"traderStanding" => TraderStanding,
				"skill" => Skill,
				"visit" => Visit,
				"buildWeapon" => BuildWeapon,
				"playerLevel" => PlayerLevel,
				"experience" => Experience,
				"warning" => Warning,
				_ => Unknown,
			};

			// mdi-key
			public const string Key = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M7 14c-1.1 0-2-.9-2-2s.9-2 2-2s2 .9 2 2s-.9 2-2 2m5.6-4c-.8-2.3-3-4-5.6-4c-3.3 0-6 2.7-6 6s2.7 6 6 6c2.6 0 4.8-1.7 5.6-4H16v4h4v-4h3v-4z\"/></svg>";
			// mdi-target-account
			public const string Shoot = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M20.95 11h1.55v2h-1.55c-.45 4.17-3.78 7.5-7.95 7.95v1.55h-2v-1.55C6.83 20.5 3.5 17.17 3.05 13H1.5v-2h1.55C3.5 6.83 6.83 3.5 11 3.05V1.5h2v1.55c4.17.45 7.5 3.78 7.95 7.95M5.07 11H6.5v2H5.07A6.98 6.98 0 0 0 11 18.93V17.5h2v1.43A6.98 6.98 0 0 0 18.93 13H17.5v-2h1.43A6.98 6.98 0 0 0 13 5.07V6.5h-2V5.07A6.98 6.98 0 0 0 5.07 11M16 16H8v-1c0-1.33 2.67-2 4-2s4 .67 4 2zm-4-8a2 2 0 0 1 2 2a2 2 0 0 1-2 2a2 2 0 0 1-2-2a2 2 0 0 1 2-2\"/></svg>";
			// mdi-close-circle-outline
			public const string GiveItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 20c-4.41 0-8-3.59-8-8s3.59-8 8-8s8 3.59 8 8s-3.59 8-8 8m0-18C6.47 2 2 6.47 2 12s4.47 10 10 10s10-4.47 10-10S17.53 2 12 2m2.59 6L12 10.59L9.41 8L8 9.41L10.59 12L8 14.59L9.41 16L12 13.41L14.59 16L16 14.59L13.41 12L16 9.41z\"/></svg>";
			// mdi-checkbox-marked-circle-outline
			public const string FindItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M20 12a8 8 0 0 1-8 8a8 8 0 0 1-8-8a8 8 0 0 1 8-8c.76 0 1.5.11 2.2.31l1.57-1.57A9.8 9.8 0 0 0 12 2A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10M7.91 10.08L6.5 11.5L11 16L21 6l-1.41-1.42L11 13.17z\"/></svg>";
			// mdi-alert-circle-outline
			public const string FindQuestItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><title>alert-circle-outline</title><path d=\"M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z\" /></svg>";
			// mdi-alert-circle-check-outline
			public const string GiveQuestItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"m18.75 22.16l-2.75-3L17.16 18l1.59 1.59L22.34 16l1.16 1.41zM11 15h2v2h-2zm0-8h2v6h-2zm1-5c5.5 0 10 4.5 10 10l-.08 1.31c-.61-.2-1.25-.31-1.98-.31l.06-1c0-4.42-3.58-8-8-8s-8 3.58-8 8s3.58 8 8 8c.71 0 1.39-.09 2.05-.26c.08.68.28 1.32.57 1.91c-.84.23-1.72.35-2.62.35c-5.53 0-10-4.5-10-10S6.47 2 12 2\"/></svg>";
			// mdi-arrow-down-thin-circle-outline
			public const string PlantQuestItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 20.03c4.41 0 8.03-3.62 8.03-8.03S16.41 3.97 12 3.97S3.97 7.59 3.97 12s3.62 8.03 8.03 8.03M12 22C6.46 22 2 17.54 2 12S6.46 2 12 2s10 4.46 10 10s-4.46 10-10 10m-1-8.46H8l4 3.96l4-3.96h-3V6.5h-2\"/></svg>";
			// mdi-arrow-down-thin-circle-outline
			public const string PlantItem = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 20.03c4.41 0 8.03-3.62 8.03-8.03S16.41 3.97 12 3.97S3.97 7.59 3.97 12s3.62 8.03 8.03 8.03M12 22C6.46 22 2 17.54 2 12S6.46 2 12 2s10 4.46 10 10s-4.46 10-10 10m-1-8.46H8l4 3.96l4-3.96h-3V6.5h-2\"/></svg>";
			// mdi-account-child-circle
			public const string TaskStatus = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 12a1.5 1.5 0 0 1 1.5 1.5A1.5 1.5 0 0 1 12 15a1.5 1.5 0 0 1-1.5-1.5A1.5 1.5 0 0 1 12 12m0-10a10 10 0 0 1 10 10a10 10 0 0 1-10 10A10 10 0 0 1 2 12A10 10 0 0 1 12 2m0 14c.72 0 1.4.15 2.04.5c.64.3.96.7.96 1.17v1.74c1.34-.6 2-1.33 2-2.21v-4.4c0-.8-.5-1.45-1.55-2c-1.05-.54-2.2-.8-3.45-.8s-2.4.26-3.45.8C7.5 11.35 7 12 7 12.8v4.4c0 .8.53 1.49 1.63 2.02c1.09.53 2.21.78 3.37.78l1-.08v-2.01L12 18c-1 0-2-.2-2.95-.61c.12-.39.48-.7 1.08-.98c.59-.28 1.21-.41 1.87-.41m0-12a2.5 2.5 0 0 0-2.5 2.5A2.5 2.5 0 0 0 12 9a2.5 2.5 0 0 0 2.5-2.5A2.5 2.5 0 0 0 12 4\"/></svg>";
			// mdi-heart-circle-outline
			public const string Extract = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 2A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10A10 10 0 0 0 12 2m0 2a8 8 0 0 1 8 8a8 8 0 0 1-8 8a8 8 0 0 1-8-8a8 8 0 0 1 8-8M9.75 7.82C8.21 7.82 7 9.03 7 10.57c0 1.89 1.7 3.43 4.28 5.77L12 17l.72-.66C15.3 14 17 12.46 17 10.57c0-1.54-1.21-2.75-2.75-2.75c-.87 0-1.7.41-2.25 1.05a3 3 0 0 0-2.25-1.05\"/></svg>";
			// mdi-remote
			public const string Mark = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 0C8.96 0 6.21 1.23 4.22 3.22l1.41 1.41A8.95 8.95 0 0 1 12 2c2.5 0 4.74 1 6.36 2.64l1.41-1.41C17.79 1.23 15.04 0 12 0M7.05 6.05l1.41 1.41a5.02 5.02 0 0 1 7.08 0l1.41-1.41A6.98 6.98 0 0 0 12 4c-1.93 0-3.68.78-4.95 2.05M12 15a2 2 0 0 1-2-2a2 2 0 0 1 2-2a2 2 0 0 1 2 2a2 2 0 0 1-2 2m3-6H9a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V10a1 1 0 0 0-1-1\"/></svg>";
			// mdi-arrow-down-drop-circle-outline
			public const string Place = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 2A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10A10 10 0 0 0 12 2m0 2a8 8 0 0 1 8 8a8 8 0 0 1-8 8a8 8 0 0 1-8-8a8 8 0 0 1 8-8m-5 6l5 5l5-5z\"/></svg>";
			// mdi-thumb-up
			public const string TraderLevel = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M23 10a2 2 0 0 0-2-2h-6.32l.96-4.57c.02-.1.03-.21.03-.32c0-.41-.17-.79-.44-1.06L14.17 1L7.59 7.58C7.22 7.95 7 8.45 7 9v10a2 2 0 0 0 2 2h9c.83 0 1.54-.5 1.84-1.22l3.02-7.05c.09-.23.14-.47.14-.73zM1 21h4V9H1z\"/></svg>";
			// mdi-thumb-up
			public const string TraderStanding = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M23 10a2 2 0 0 0-2-2h-6.32l.96-4.57c.02-.1.03-.21.03-.32c0-.41-.17-.79-.44-1.06L14.17 1L7.59 7.58C7.22 7.95 7 8.45 7 9v10a2 2 0 0 0 2 2h9c.83 0 1.54-.5 1.84-1.22l3.02-7.05c.09-.23.14-.47.14-.73zM1 21h4V9H1z\"/></svg>";
			// mdi-dumbbell
			public const string Skill = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M20.57 14.86L22 13.43L20.57 12L17 15.57L8.43 7L12 3.43L10.57 2L9.14 3.43L7.71 2L5.57 4.14L4.14 2.71L2.71 4.14l1.43 1.43L2 7.71l1.43 1.43L2 10.57L3.43 12L7 8.43L15.57 17L12 20.57L13.43 22l1.43-1.43L16.29 22l2.14-2.14l1.43 1.43l1.43-1.43l-1.43-1.43L22 16.29z\"/></svg>";
			// mdi-crosshairs-gps
			public const string Visit = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 8a4 4 0 0 1 4 4a4 4 0 0 1-4 4a4 4 0 0 1-4-4a4 4 0 0 1 4-4m-8.95 5H1v-2h2.05C3.5 6.83 6.83 3.5 11 3.05V1h2v2.05c4.17.45 7.5 3.78 7.95 7.95H23v2h-2.05c-.45 4.17-3.78 7.5-7.95 7.95V23h-2v-2.05C6.83 20.5 3.5 17.17 3.05 13M12 5a7 7 0 0 0-7 7a7 7 0 0 0 7 7a7 7 0 0 0 7-7a7 7 0 0 0-7-7\"/></svg>";
			// mdi-progress-wrench
			public const string BuildWeapon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M13 2.03v2.02c4.39.54 7.5 4.53 6.96 8.92c-.46 3.64-3.32 6.53-6.96 6.96v2c5.5-.55 9.5-5.43 8.95-10.93c-.45-4.75-4.22-8.5-8.95-8.97m-2 .03c-1.95.19-3.81.94-5.33 2.2L7.1 5.74c1.12-.9 2.47-1.48 3.9-1.68zM4.26 5.67A9.9 9.9 0 0 0 2.05 11h2c.19-1.42.75-2.77 1.64-3.9zM2.06 13c.2 1.96.97 3.81 2.21 5.33l1.42-1.43A8 8 0 0 1 4.06 13zm5.04 5.37l-1.43 1.37A10 10 0 0 0 11 22v-2a8 8 0 0 1-3.9-1.63m9.72-3.18l-4.11-4.11c.41-1.04.18-2.26-.68-3.11c-.9-.91-2.25-1.09-3.34-.59l1.94 1.94l-1.35 1.36l-1.99-1.95c-.54 1.09-.29 2.44.59 3.35c.86.86 2.08 1.08 3.12.68l4.11 4.1c.18.19.45.19.63 0l1.04-1.03c.22-.18.22-.5.04-.64\"/></svg>";
			// mdi-crown-circle-outline
			public const string PlayerLevel = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 2C6.47 2 2 6.5 2 12s4.5 10 10 10s10-4.5 10-10S17.5 2 12 2m0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8s8 3.58 8 8s-3.58 8-8 8m-4-6L7 8l3 2l2-3l2 3l3-2l-1 6zm.56 2c-.34 0-.56-.22-.56-.56V15h8v.44c0 .34-.22.56-.56.56z\"/></svg>";
			// mdi-eye-circle-outline
			public const string Experience = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M12 22A10 10 0 0 1 2 12A10 10 0 0 1 12 2a10 10 0 0 1 10 10a10 10 0 0 1-10 10m0-2a8 8 0 0 0 8-8a8 8 0 0 0-8-8a8 8 0 0 0-8 8a8 8 0 0 0 8 8m0-9a1 1 0 0 1 1 1a1 1 0 0 1-1 1a1 1 0 0 1-1-1a1 1 0 0 1 1-1m0-3c2.63 0 5 1.57 6 4a6.505 6.505 0 0 1-8.5 3.5A6.52 6.52 0 0 1 6 12c1-2.43 3.37-4 6-4m0 1.5A2.5 2.5 0 0 0 9.5 12a2.5 2.5 0 0 0 2.5 2.5a2.5 2.5 0 0 0 2.5-2.5A2.5 2.5 0 0 0 12 9.5\"/></svg>";
			// mdi-alert-circle
			public const string Warning = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1em\" height=\"1em\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M13 13h-2V7h2m0 10h-2v-2h2M12 2A10 10 0 0 0 2 12a10 10 0 0 0 10 10a10 10 0 0 0 10-10A10 10 0 0 0 12 2\"/></svg>";
			// mdi-help-circle-outlien
			public const string Unknown = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><title>help-circle-outline</title><path d=\"M11,18H13V16H11V18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,6A4,4 0 0,0 8,10H10A2,2 0 0,1 12,8A2,2 0 0,1 14,10C14,12 11,11.75 11,15H13C13,12.75 16,12.5 16,10A4,4 0 0,0 12,6Z\" /></svg>";
		}
	}
}

using System.Drawing;
using System.Drawing.Imaging;
using RatEye;
using RatStash;
using Xunit;

namespace RatScanner.Tests;

public class FullScreenNameScanTests {
	[Fact]
	public void SkipsUnmatchedInspectionsAndKeepsLaterMatchesInOrder() {
		var config = new Config();
		config.ProcessingConfig.UseCache = false;
		var firstItem = new Item { Id = "first", Name = "First match" };
		var secondItem = new Item { Id = "second", Name = "Second match" };
		var engine = new RatEyeEngine(config, Database.FromItems([firstItem, secondItem]));
		using var capture = new Bitmap(64, 64, PixelFormat.Format24bppRgb);
		var unmatched = engine.NewInspection(capture);
		var firstMatch = engine.NewInspection(capture);
		var secondMatch = engine.NewInspection(capture);

		var matches = RatScannerMain.GetFullScreenNameMatches(
			[unmatched, firstMatch, secondMatch], new Vector2(0, 0), 20,
			inspection => inspection == firstMatch ? firstItem : inspection == secondMatch ? secondItem : null
		);

		Assert.Collection(matches,
			match => {
				Assert.Same(firstMatch, match.Inspection);
				Assert.Same(firstItem, match.Item);
			},
			match => {
				Assert.Same(secondMatch, match.Inspection);
				Assert.Same(secondItem, match.Item);
			}
		);
	}
}

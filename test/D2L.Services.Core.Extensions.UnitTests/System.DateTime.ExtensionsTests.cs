using System;
using NUnit.Framework;

namespace D2L.Services {
	[TestFixture]
	public sealed class DateTimeExtensionsTests {
		[Test]
		public void TimeSinceUnixEpoch_UnixEpoch_0() {
			var dt = DotNetExtensions.UNIX_EPOCH.TimeSinceUnixEpoch();
			Assert.AreEqual( 0, dt.TotalMilliseconds );
		}

		[Test]
		public void TimeSinceUnixEpoch_UnixEpochPlusADay_ADay() {
			var theDayAfter = new DateTime( 1970, 1, 2, 0, 0, 0, DateTimeKind.Utc );
			var dt = theDayAfter.TimeSinceUnixEpoch();

			Assert.AreEqual( 1, dt.Days );
			Assert.AreEqual( 0, dt.Milliseconds );
		}
	}
}

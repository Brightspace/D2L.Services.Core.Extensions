using System;

namespace D2L.Services {
	public static partial class DotNetExtensions {
		public static readonly DateTime UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

		/// <summary>
		/// Get the time since Unix Epoch: 00:00:00 UTC January 1st 1970
		/// </summary>
		public static TimeSpan TimeSinceUnixEpoch( this DateTime @this ) {
			return @this.ToUniversalTime().Subtract( UNIX_EPOCH );			
		}
	}
}

using System;

namespace D2L.Services {
	public static partial class DotNetExtensions {
		/// <summary>
		/// Dispose if non-null
		/// </summary>
		public static void SafeDispose( this IDisposable @this ) {
			if( @this != null ) {
				@this.Dispose();
			}
		}
	}
}

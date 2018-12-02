using System;

namespace D2L.Services {
	public static partial class DotNetExtensions {

		/// <summary>
		/// A null-safe version of <see cref="Object.GetHashCode"/> that returns
		/// <c>0</c> if the object is <c>null</c>.
		/// </summary>
		/// <returns>
		/// The object's hash code, or <c>0</c> if the object is <c>null</c>
		/// </returns>
		public static int SafeGetHashCode( this object obj ) {
			return ( obj != null ) ? obj.GetHashCode() : 0;
		}

	}
}

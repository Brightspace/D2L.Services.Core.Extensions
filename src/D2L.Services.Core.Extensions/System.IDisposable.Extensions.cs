using System;
using System.Collections.Generic;

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
		
		/// <summary>
		/// Disposes the current object if it is not null. If an exception would
		/// be thrown, <paramref name="currentException"/> is changed to include
		/// the thrown exception according to the following rules:
		/// <list type="bullet">
		/// <item><description>
		/// If <paramref name="currentException"/> is <c>null</c>, it is set to
		/// the exception thrown.
		/// </description></item>
		/// <item><description>
		/// If <paramref name="currentException"/> is any exception other then
		/// <see cref="AggregateException"/>, it is set to a new
		/// <see cref="AggregateException"/> containing the original value of
		/// <paramref name="currentException"/> and the exception thrown.
		/// </description></item>
		/// <item><description>
		/// If <paramref name="currentException"/> is an
		/// <see cref="AggregateException"/>, it is set to a new
		/// <see cref="AggregateException"/> containing its original inner
		/// exceptions plus the newly thrown exception.
		/// </description></item>
		/// </list>
		/// </summary>
		public static void SafeDispose(
			this IDisposable disposable,
			ref Exception currentException
		) {
			try {
				disposable.SafeDispose();
			} catch( Exception newException ) {
				if( currentException == null ) {
					currentException = newException;
				} else if( currentException is AggregateException ) {
					List<Exception> exceptions = new List<Exception>();
					exceptions.AddRange( ((AggregateException)currentException).InnerExceptions );
					exceptions.Add( newException );
					currentException = new AggregateException( exceptions );
				} else {
					currentException = new AggregateException(
						currentException,
						newException
					);
				}
			}
		}
		
	}
}

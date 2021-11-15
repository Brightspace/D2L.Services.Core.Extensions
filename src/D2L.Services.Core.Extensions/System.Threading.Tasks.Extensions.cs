using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace D2L.Services {
	public static partial class DotNetExtensions {
		/// <summary>
		/// Configure task to avoid deadlocks.
		///
		/// NOTE: thread-local global variables will be unavailable inside the
		/// execution of the task. This means any code that "comes after" the
		/// await. An example of thread-local storage is HttpContext.Current.
		/// </summary>
		/// <returns>
		/// A configured task that should be await'ed.
		/// </returns>
		public static ConfiguredTaskAwaitable SafeAsync( this Task @this ) {
			return @this.ConfigureAwait( continueOnCapturedContext: false );
		}

		/// <summary>
		/// Configure task to avoid deadlocks.
		///
		/// NOTE: thread-local global variables will be unavailable inside the
		/// execution of the task. This means any code that "comes after" the
		/// await. An example of thread-local storage is HttpContext.Current.
		/// </summary>
		/// <returns>
		/// A configured task that should be await'ed.
		/// </returns>
		public static ConfiguredTaskAwaitable<T> SafeAsync<T>( this Task<T> @this ) {
			return @this.ConfigureAwait( continueOnCapturedContext: false );
		}

		/// <summary>
		/// Configure task to avoid deadlocks.
		///
		/// NOTE: thread-local global variables will be unavailable inside the
		/// execution of the task. This means any code that "comes after" the
		/// await. An example of thread-local storage is HttpContext.Current.
		/// </summary>
		/// <returns>
		/// A configured task that should be await'ed.
		/// </returns>
		public static ConfiguredValueTaskAwaitable SafeAsync( this ValueTask @this ) {
			return @this.ConfigureAwait( continueOnCapturedContext: false );
		}

		/// <summary>
		/// Configure task to avoid deadlocks.
		///
		/// NOTE: thread-local global variables will be unavailable inside the
		/// execution of the task. This means any code that "comes after" the
		/// await. An example of thread-local storage is HttpContext.Current.
		/// </summary>
		/// <returns>
		/// A configured task that should be await'ed.
		/// </returns>
		public static ConfiguredValueTaskAwaitable<T> SafeAsync<T>( this ValueTask<T> @this ) {
			return @this.ConfigureAwait( continueOnCapturedContext: false );
		}

		/// <summary>
		/// Block "safely". This is better than Task.Wait because Task.Wait throws
		/// AggregateExceptions instead of the exception we expect.
		///
		/// NOTE: See the NOTE in SafeAsync(). It applies to this function too.
		/// </summary>
		public static void SafeWait( this Task @this ) {
			@this.SafeAsync().GetAwaiter().GetResult();
		}


		/// <summary>
		/// Block "safely". This is better than Task.Result because Task.Result throws
		/// AggregateExceptions instead of the exception we expect.
		///
		/// NOTE: See the NOTE in SafeAsync(). It applies to this function too.
		/// </summary>
		public static T SafeWait<T>( this Task<T> @this ) {
			return @this.SafeAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Block "safely". This is better than Task.Wait because Task.Wait throws
		/// AggregateExceptions instead of the exception we expect.
		///
		/// NOTE: See the NOTE in SafeAsync(). It applies to this function too.
		/// </summary>
		public static void SafeWait( this ValueTask @this ) {
			@this.SafeAsync().GetAwaiter().GetResult();
		}


		/// <summary>
		/// Block "safely". This is better than Task.Result because Task.Result throws
		/// AggregateExceptions instead of the exception we expect.
		///
		/// NOTE: See the NOTE in SafeAsync(). It applies to this function too.
		/// </summary>
		public static T SafeWait<T>( this ValueTask<T> @this ) {
			return @this.SafeAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Execute an action after an async task completes
		/// </summary>
		public static async Task<TOut> Then<TIn, TOut>(
			this Task<TIn> task,
			Func<TIn,TOut> syncFunc
		) {
			return syncFunc( await task.SafeAsync() );
		}

		/// <summary>
		/// Execute an async action after an async task completes
		/// </summary>
		public static async Task<TOut> ThenAsync<TIn, TOut>(
			this Task<TIn> task,
			Func<TIn, Task<TOut>> asyncFunc
		) {
			return await asyncFunc( await task.SafeAsync() ).SafeAsync();
		}

		/// <summary>
		/// Upcast the result of a task
		/// </summary>
		public static async Task<TOut> As<TIn,TOut>( this Task<TIn> task ) where TIn: TOut {
			return await task.SafeAsync();
		}

	}
}

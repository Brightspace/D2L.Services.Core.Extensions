using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace D2L.Services {
	public static partial class DotNetExtensions {
		
		/// <summary>
		/// Invokes an asynchronous function on a collection of values. The
		/// function is invoked on every element in parallel.
		/// </summary>
		/// <param name="collection">
		/// The set of items on which to operate.
		/// </param>
		/// <param name="function">
		/// The async function to execute on each element of the enumerable.
		/// </param>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="function"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		public static Task ForEachInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T,Task> function
		) {
			return WaitOnAllTasksAsync( collection.Select( function ) );
		}
		
		/// <summary>
		/// Invokes an asynchronous function on a collection of values. The
		/// function is invoked on every element in parallel with each other up
		/// to <paramref name="maxConcurrency"/> at once.
		/// </summary>
		/// <param name="collection">
		/// The set of iems on which to operate.
		/// </param>
		/// <param name="function">
		/// The async function to execute on each element of the enumerable.
		/// </param>
		/// <param name="maxConcurrency">
		/// The maximum number of tasks that are allowed to be run concurrenty.
		/// </param>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="function"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="maxConcurrency"/> is nonpositive
		/// </exception>
		public static async Task ForEachInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T,Task> function,
			int maxConcurrency
		) {
			ValidateMaxConcurrency( maxConcurrency );
			IList<Task> tasks = new List<Task>();
			using( SemaphoreSlim limiter = new SemaphoreSlim( maxConcurrency ) ) {
				foreach( T input in collection ) {
					await limiter.WaitAsync().SafeAsync();
					tasks.Add( function( input ).ReleaseOnCompletion( limiter ) );
				}
				await WaitOnAllTasksAsync( tasks ).SafeAsync();
			}
		}

		/// <summary>
		/// Filters a sequence of values using an asynchronous predicate
		/// function. All elements are asynchronously checked against the
		/// predicate in parallel. The ordering of the elements is preserved.
		/// </summary>
		/// <param name="collection">
		/// The set of iems on which to operate.
		/// </param>
		/// <param name="predicate">
		/// An async function that returns <c>true</c> iff the element should be
		/// included in the results.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains elements from the
		/// input sequence that satisfy the <paramref name="predicate"/>. The
		/// ordering of the elements is preserved.
		/// </returns>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="predicate"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		public static async Task<IEnumerable<T>> FilterInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T, Task<bool>> predicate
		) {
			IList<T> storedCollection = collection as IList<T> ?? collection.ToList();
			return Filter(
				storedCollection,
				await storedCollection.MapInParallelAsync( predicate ).SafeAsync()
			);
		}

		/// <summary>
		/// Filters a sequence of values using an asynchronous predicate
		/// function. All elements are asynchronously checked against the
		/// predicate in parallel with each other up to
		/// <paramref name="maxConcurrency"/> at once. The ordering of the
		/// elements is preserved.
		/// </summary>
		/// <param name="collection">
		/// The set of iems on which to operate.
		/// </param>
		/// <param name="predicate">
		/// An async function that returns <c>true</c> iff the element should be
		/// included in the results.
		/// </param>
		/// <param name="maxConcurrency">
		/// The maximum number of tasks that are allowed to be run concurrenty.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> that contains elements from the
		/// input sequence that satisfy the <paramref name="predicate"/>. The
		/// ordering of the elements is preserved.
		/// </returns>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="predicate"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="maxConcurrency"/> is nonpositive
		/// </exception>
		public static async Task<IEnumerable<T>> FilterInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T, Task<bool>> predicate,
			int maxConcurrency
		) {
			ValidateMaxConcurrency( maxConcurrency );
			IList<T> storedCollection = collection as IList<T> ?? collection.ToList();
			return Filter(
				storedCollection,
				await storedCollection.MapInParallelAsync( predicate, maxConcurrency ).SafeAsync()
			);
		}

		/// <summary>
		/// Projects each element of a sequence into a new form using an
		/// asynchronous transformation function. All transformations are
		/// performed asynchronously and in parallel with each other. The
		/// ordering of the elements is preserved.
		/// </summary>
		/// <param name="collection">
		/// The set of iems on which to operate.
		/// </param>
		/// <param name="transform">
		/// The async transformation function to apply to each element.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{TOut}"/> whose elements are the result of
		/// invoking the <paramref name="transform"/> function on each element
		/// of the input sequence. The ordering of the elements is preserved.
		/// </returns>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="transform"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		public static async Task<IEnumerable<TOut>> MapInParallelAsync<TIn,TOut>(
			this IEnumerable<TIn> collection,
			Func<TIn, Task<TOut>> transform
		) {
			IList<Task<TOut>> tasks = collection.Select( transform ).ToList();
			await WaitOnAllTasksAsync( tasks ).SafeAsync();
			return tasks.Select( completedTask => completedTask.Result );
		}

		/// <summary>
		/// Projects each element of a sequence into a new form using an
		/// asynchronous transformation function. All transformations are
		/// performed asynchronously and in parallel with each other up to
		/// <paramref name="maxConcurrency"/> at once.
		/// The ordering of the elements is preserved.
		/// </summary>
		/// <param name="collection">
		/// The set of iems on which to operate.
		/// </param>
		/// <param name="transform">
		/// The async transformation function to apply to each element.
		/// </param>
		/// <param name="maxConcurrency">
		/// The maximum number of tasks that are allowed to be run concurrenty.
		/// </param>
		/// <returns>
		/// An <see cref="IEnumerable{TOut}"/> whose elements are the result of
		/// invoking the <paramref name="transform"/> function on each element
		/// of the input sequence. The ordering of the elements is preserved.
		/// </returns>
		/// <exception cref="AggregateException">
		/// An exception was thrown by one or more invocations of
		/// <paramref name="transform"/>. The exceptions thrown are stored in the
		/// <see cref="AggregateException.InnerExceptions" /> of the
		/// <see cref="AggregateException"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="maxConcurrency"/> is nonpositive
		/// </exception>
		public static async Task<IEnumerable<TOut>> MapInParallelAsync<TIn,TOut>(
			this IEnumerable<TIn> collection,
			Func<TIn, Task<TOut>> transform,
			int maxConcurrency
		) {
			ValidateMaxConcurrency( maxConcurrency );
			IList<Task<TOut>> tasks = new List<Task<TOut>>();
			using( SemaphoreSlim limiter = new SemaphoreSlim( maxConcurrency ) ) {
				foreach( TIn input in collection ) {
					await limiter.WaitAsync().SafeAsync();
					tasks.Add( transform( input ).ReleaseOnCompletion<TOut>( limiter ) );
				}
				await WaitOnAllTasksAsync( tasks ).SafeAsync();
				return tasks.Select( completedTask => completedTask.Result );
			}
		}
		
		private static async Task WaitOnAllTasksAsync( IEnumerable<Task> tasks ) {
			Task allTasks = Task.WhenAll( tasks );
			try {
				await allTasks.SafeAsync();
			} catch( Exception exception ) {
				// If one of the tasks fails, Task.WhenAll just returns the
				// first exception thrown. To get an AggregateException that
				// contains the exceptions thrown by all failed tasks, we need
				// to inspect the Exception property of the task.
				throw allTasks.Exception ?? exception;
			}
		}
		
		private static async Task ReleaseOnCompletion(
			this Task task,
			SemaphoreSlim semaphore
		) {
			try {
				await task.SafeAsync();
			} finally {
				semaphore.Release();
			}
		}
		
		private static async Task<T> ReleaseOnCompletion<T>(
			this Task<T> task,
			SemaphoreSlim semaphore
		) {
			try {
				return await task.SafeAsync();
			} finally {
				semaphore.Release();
			}
		}
		
		private static IEnumerable<T> Filter<T>(
			IList<T> values,
			IEnumerable<bool> includeBits
		) {
			int i = 0;
			foreach( bool shouldInclude in includeBits ) {
				if( shouldInclude ) {
					yield return values[i];
				}
				i++;
			}
		}
		
		private static void ValidateMaxConcurrency( int maxConcurrency ) {
			if( maxConcurrency <= 0 ) {
				throw new ArgumentOutOfRangeException(
					paramName: "maxConcurrency",
					message: "maxConcurrency must be a positive integer"
				);
			}
		}
		
	}
}

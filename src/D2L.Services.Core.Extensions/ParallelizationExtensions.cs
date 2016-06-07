using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D2L.Services {
	public static partial class DotNetExtensions {
		
		/// <summary>
		/// Invokes an asynchronous function on a collection of values. The
		/// function is invoked on every element in parallel.
		/// </summary>
		/// <param name="function">
		/// The async function to execute on each element of the enumerable.
		/// </param>
		public static Task ForEachInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T,Task> function
		) {
			return WaitOnAllTasksAsync( collection.Select( function ) );
		}
		
		/// <summary>
		/// Filters a sequence of values using an asynchronous predicate
		/// function. All elements are asynchronously checked against the
		/// predicate in parallel. The ordering of the elements is preserved.
		/// </summary>
		/// <param name="predicate">
		/// An async function that returns <c>true</c> iff the element should be
		/// included in the results.
		/// </param>
		/// <returns>
		/// A list filtered on the given predicate. The ordering of the elements
		/// is preserved.
		/// </returns>
		public static async Task<IEnumerable<T>> FilterInParallelAsync<T>(
			this IEnumerable<T> collection,
			Func<T, Task<bool>> predicate
		) {
			IList<T> storedCollection = collection as IList<T> ?? collection.ToList();
			IList<Task<bool>> tasks = storedCollection.Select( predicate ).ToList();
			await WaitOnAllTasksAsync( tasks ).SafeAsync();
			return storedCollection.Where( ( element, i ) => tasks[i].Result );
		}
		
		/// <summary>
		/// Projects each element of a sequence into a new form using an
		/// asynchronous transformation function. All transformations are
		/// performed asynchronously and in parallel with each other.
		/// The ordering of the elements is preserved.
		/// </summary>
		/// <param name="transform">
		/// The async transformation function to apply to each element.
		/// </param>
		/// <returns>
		/// An IEnumerable{T} whose elements are the result of invoking the
		/// <paramref name="transform"/> function on each element of source.
		/// The ordering of the elements is preserved.
		/// </returns>
		public static async Task<IEnumerable<TOut>> MapInParallelAsync<TIn,TOut>(
			this IEnumerable<TIn> collection,
			Func<TIn, Task<TOut>> transform
		) {
			IList<Task<TOut>> tasks = collection.Select( transform ).ToList();
			await WaitOnAllTasksAsync( tasks ).SafeAsync();
			return tasks.Select( completedTask => completedTask.Result );
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
		
	}
}

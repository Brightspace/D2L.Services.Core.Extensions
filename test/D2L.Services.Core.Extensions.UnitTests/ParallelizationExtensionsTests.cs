using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace D2L.Services {

	[TestFixture]
	[Category( "Unit" )]
	internal sealed class ParallelizationExtensionsTests {

		private Random m_rng;

		[OneTimeSetUp]
		public void TestFixtureSetUp() {
			m_rng = new Random( DateTime.Now.GetHashCode() );
		}

		#region ForEachInParallelAsync Tests
		[Test]
		public async Task ForEachInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[] { 0, 0, 3, 0, 1, 2 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 3 ),
				new TestException( 1 ),
				new TestException( 2 )
			};

			Task forEachTask = numbers.ForEachInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				if( n != 0 ) {
					throw new TestException( n );
				}
			} );

			try {
				await forEachTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}

			Assert.Fail( "Expected AggregateException to be thrown." );
		}

		[Test]
		public async Task ForEachInParallelAsync_RespectsMaxConcurrency() {
			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;

			int[] inputs = Enumerable.Repeat<int>( 0, 12 ).ToArray();
			await inputs.ForEachInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				Assert.LessOrEqual( concurrentTasks, MAX_CONCURRENT );
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );
			}, MAX_CONCURRENT ).SafeAsync();

			Interlocked.MemoryBarrier();
			Assert.IsTrue( reachedMaximum );
		}

		[Test]
		public async Task ForEachInParallelAsync_ExceptionHandling_RespectsMaxConcurrency() {
			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;
			bool exceededMaximum = false;

			int[] numbers = { 1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 3 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 1 ),
				new TestException( 2 ),
				new TestException( 3 )
			};

			Task forEachTask = numbers.ForEachInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				exceededMaximum |= concurrentTasks > MAX_CONCURRENT;
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );

				if( n != 0 ) {
					throw new TestException( n );
				}
			}, MAX_CONCURRENT );

			AggregateException caughtException = null;
			try {
				await forEachTask.SafeAsync();
			} catch( AggregateException exception ) {
				caughtException = exception;
			}

			Interlocked.MemoryBarrier();
			Assert.IsTrue( reachedMaximum );
			Assert.IsFalse( exceededMaximum );
			Assert.IsNotNull( caughtException );
			CollectionAssert.AreEqual( expectedInnerExceptions, caughtException.InnerExceptions );
		}
		#endregion

		#region MapInParallelAsync Tests
		[Test]
		public async Task MapInParallelAsyncTest() {
			int[] numbers = new[] { 3, 6, 9, 12, 15, 18, 21, 24, 27 };

			IEnumerable<int> expected = numbers.Select( n => n + 1 );
			IEnumerable<int> actual = await numbers.MapInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				return n + 1;
			} ).SafeAsync();

			CollectionAssert.AreEqual( expected, actual );
		}

		[Test]
		public async Task MapInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[] { 0, 0, 3, 0, 1, 2 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 3 ),
				new TestException( 1 ),
				new TestException( 2 )
			};

			Task<IEnumerable<int>> mapTask = numbers.MapInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				if( n != 0 ) {
					throw new TestException( n );
				}
				return n;
			} );

			try {
				await mapTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}

			Assert.Fail( "Expected AggregateException to be thrown." );
		}

		[Test]
		public async Task MapInParallelAsync_RespectsMaxConcurrency() {
			int[] numbers = new[] { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36 };

			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;

			IEnumerable<int> expected = numbers.Select( n => n + 1 );
			IEnumerable<int> actual = await numbers.MapInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				Assert.LessOrEqual( concurrentTasks, MAX_CONCURRENT );
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );

				return n + 1;
			}, MAX_CONCURRENT ).SafeAsync();

			Interlocked.MemoryBarrier();
			CollectionAssert.AreEqual( expected, actual );
			Assert.IsTrue( reachedMaximum );
		}

		[Test]
		public async Task MapInParallelAsync_ExceptionHandling_RespectsMaxConcurrency() {
			int[] numbers = { 1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 3 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 1 ),
				new TestException( 2 ),
				new TestException( 3 )
			};

			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;
			bool exceededMaximum = false;

			Task<IEnumerable<int>> mapTask = numbers.MapInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				exceededMaximum |= concurrentTasks > MAX_CONCURRENT;
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );

				if( n != 0 ) {
					throw new TestException( n );
				}

				return n;
			}, MAX_CONCURRENT );

			AggregateException caughtException = null;
			try {
				await mapTask.SafeAsync();
			} catch( AggregateException exception ) {
				caughtException = exception;
			}

			Interlocked.MemoryBarrier();
			Assert.IsTrue( reachedMaximum );
			Assert.IsFalse( exceededMaximum );
			Assert.IsNotNull( caughtException );
			CollectionAssert.AreEqual( expectedInnerExceptions, caughtException.InnerExceptions );
		}
		#endregion

		#region FilterInParallelAsync Tests
		[Test]
		public async Task FilterInParallelAsyncTest() {
			int[] numbers = new[] { 1, 6, 1, 8, 0, 3, 3, 9, 8, 8, 7, 5 };
			int[] oddOnly = new[] { 1, 1, 3, 3, 9, 7, 5 };

			IEnumerable<int> result = await numbers.FilterInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				return n % 2 != 0;
			} ).SafeAsync();

			CollectionAssert.AreEqual( oddOnly, result );
		}

		[Test]
		public async Task FilterInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[] { 0, 0, 3, 0, 1, 2 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 3 ),
				new TestException( 1 ),
				new TestException( 2 )
			};

			Task<IEnumerable<int>> filterTask = numbers.FilterInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				if( n != 0 ) {
					throw new TestException( n );
				}
				return true;
			} );

			try {
				await filterTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}

			Assert.Fail( "Expected AggregateException to be thrown." );
		}

		[Test]
		public async Task FilterInParallelAsync_RespectsMaxConcurrency() {
			int[] numbers = new[] { 1, 6, 1, 8, 0, 3, 3, 9, 8, 8, 7, 5 };
			int[] oddOnly = new[] { 1, 1, 3, 3, 9, 7, 5 };

			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;

			IEnumerable<int> result = await numbers.FilterInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				Assert.LessOrEqual( concurrentTasks, MAX_CONCURRENT );
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );

				return n % 2 != 0;
			}, MAX_CONCURRENT ).SafeAsync();

			Interlocked.MemoryBarrier();
			CollectionAssert.AreEqual( oddOnly, result );
			Assert.IsTrue( reachedMaximum );
		}

		[Test]
		public async Task FilterInParallelAsync_ExceptionHandling_RespectsMaxConcurrency() {
			int[] numbers = { 1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 3 };
			Exception[] expectedInnerExceptions = new TestException[]{
				new TestException( 1 ),
				new TestException( 2 ),
				new TestException( 3 )
			};

			const int MAX_CONCURRENT = 3;
			int numConcurrent = 0;

			bool reachedMaximum = false;
			bool exceededMaximum = false;

			Task<IEnumerable<int>> filterTask = numbers.FilterInParallelAsync( async n => {
				int concurrentTasks = Interlocked.Increment( ref numConcurrent );
				exceededMaximum |= concurrentTasks > MAX_CONCURRENT;
				reachedMaximum |= concurrentTasks == MAX_CONCURRENT;

				await RandomDelay( 200, 500 ).SafeAsync();
				Interlocked.Decrement( ref numConcurrent );

				if( n != 0 ) {
					throw new TestException( n );
				}

				return true;
			}, MAX_CONCURRENT );

			AggregateException caughtException = null;
			try {
				await filterTask.SafeAsync();
			} catch( AggregateException exception ) {
				caughtException = exception;
			}

			Interlocked.MemoryBarrier();
			Assert.IsTrue( reachedMaximum );
			Assert.IsFalse( exceededMaximum );
			Assert.IsNotNull( caughtException );
			CollectionAssert.AreEqual( expectedInnerExceptions, caughtException.InnerExceptions );
		}
		#endregion


		private Task RandomDelay( double maxMilliseconds ) {
			return RandomDelay( 0, maxMilliseconds );
		}

		private Task RandomDelay( double minMilliseconds, double maxMilliseconds ) {
			return Task.Delay(
				TimeSpan.FromMilliseconds(
					minMilliseconds + ( m_rng.NextDouble() * ( maxMilliseconds - minMilliseconds ) )
				)
			);
		}

		private sealed class TestException : Exception {

			private readonly int m_id;

			public TestException( int id ) {
				m_id = id;
			}

			public override bool Equals( object obj ) {
				var that = obj as TestException;
				return ( that != null && this.m_id == that.m_id );
			}

			public override int GetHashCode() {
				return m_id.GetHashCode();
			}

		}

	}

}

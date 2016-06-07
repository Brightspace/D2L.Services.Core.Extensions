using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using D2L.Services;
using NUnit.Framework;

namespace D2L.Services {
	
	[TestFixture]
	[Category( "Unit" )]
	internal sealed class ParallelizationExtensionsTests {
		
		private Random m_rng;
		
		[TestFixtureSetUp]
		public void TestFixtureSetUp() {
			m_rng = new Random( DateTime.Now.GetHashCode() );
		}
		
		[Test]
		public async Task FilterInParallelAsyncTest() {
			int[] numbers = new[]{ 1, 6, 1, 8, 0, 3, 3, 9, 8, 8, 7, 5 };
			int[] oddOnly = new[]{ 1,    1,       3, 3, 9,       7, 5 };
			
			IEnumerable<int> result = await numbers.FilterInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				return n % 2 != 0;
			}).SafeAsync();
			
			CollectionAssert.AreEqual( oddOnly, result );
		}
		
		[Test]
		public async Task MapInParallelAsyncTest() {
			int[] numbers = new[]{ 3, 6, 9, 12, 15, 18, 21, 24, 27 };
			
			IEnumerable<int> expected = numbers.Select( n => n + 1 );
			IEnumerable<int> actual = await numbers.MapInParallelAsync( async n => {
				await RandomDelay( 900 ).SafeAsync();
				return n + 1;
			}).SafeAsync();
			
			CollectionAssert.AreEqual( expected, actual );
		}
		
		[Test]
		public async Task ForEachInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[]{ 0, 0, 3, 0, 1, 2 };
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
			});
			
			try {
				await forEachTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}
			
			Assert.Fail( "Expected AggregateException to be thrown." );
		}
		
		[Test]
		public async Task FilterInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[]{ 0, 0, 3, 0, 1, 2 };
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
			});
			
			try {
				await filterTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}
			
			Assert.Fail( "Expected AggregateException to be thrown." );
		}
		
		[Test]
		public async Task MapInParallelAsync_ExceptionHandlingTest() {
			int[] numbers = new[]{ 0, 0, 3, 0, 1, 2 };
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
			});
			
			try {
				await mapTask.SafeAsync();
			} catch( AggregateException exception ) {
				CollectionAssert.AreEqual( expectedInnerExceptions, exception.InnerExceptions );
				Assert.Pass();
			}
			
			Assert.Fail( "Expected AggregateException to be thrown." );
		}
		
		
		private Task RandomDelay( double maxMilliseconds ) {
			return Task.Delay( TimeSpan.FromMilliseconds( m_rng.NextDouble() * maxMilliseconds ) );
		}
		
		private sealed class TestException : Exception {
			
			private readonly int m_id;
			
			public TestException( int id ) {
				m_id = id;
			}
			
			public override bool Equals( object obj ) {
				var that = obj as TestException;
				return( that != null && this.m_id == that.m_id );
			}
			
			public override int GetHashCode() {
				return m_id.GetHashCode();
			}
			
		}
		
	}
	
}

using System;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;

namespace D2L.Services {

	[TestFixture]
	[Category( "Unit" )]
	internal sealed class IDisposableExtensionTests {

		[Test]
		public void SafeDisposeTest_DisposeThrows_NullBecomesException() {
			Exception exception = null;
			Exception thrownException = new Exception();

			MockDisposable( throws: thrownException ).SafeDispose( ref exception );

			Assert.AreSame( exception, thrownException );
		}

		[Test]
		public void SafeDisposeTest_DisposeThrows_NormalExceptionBecomesAggregateException() {
			Exception initialException = new Exception();

			Exception exception = initialException;
			Exception thrownException = new Exception();

			MockDisposable( throws: thrownException ).SafeDispose( ref exception );

			Assert.IsTrue( exception is AggregateException );

			ReadOnlyCollection<Exception> innerExceptions =
				( ( AggregateException )exception ).InnerExceptions;

			Assert.AreEqual( innerExceptions.Count, 2 );
			Assert.AreSame( innerExceptions[ 0 ], initialException );
			Assert.AreSame( innerExceptions[ 1 ], thrownException );
		}

		[Test]
		public void SafeDisposeTest_DisposeThrows_AggregateExceptionAddsNewInnerException() {
			AggregateException initialException = new AggregateException(
				innerExceptions: new[] { new Exception(), new Exception() }
			);

			Exception exception = initialException;
			Exception thrownException = new Exception();

			MockDisposable( throws: thrownException ).SafeDispose( ref exception );

			Assert.IsTrue( exception is AggregateException );

			ReadOnlyCollection<Exception> innerExceptions =
				( ( AggregateException )exception ).InnerExceptions;

			Assert.AreEqual( innerExceptions.Count, 3 );
			Assert.AreSame( innerExceptions[ 0 ], initialException.InnerExceptions[ 0 ] );
			Assert.AreSame( innerExceptions[ 1 ], initialException.InnerExceptions[ 1 ] );
			Assert.AreSame( innerExceptions[ 2 ], thrownException );
		}

		private static readonly Exception[] INITIAL_EXCEPTION_TEST_CASES = new[]{
			null,
			new Exception(),
			new AggregateException(),
			new AggregateException( new Exception(), new Exception() )
		};

		[Test, TestCaseSource( sourceName: "INITIAL_EXCEPTION_TEST_CASES" )]
		public void SafeDisposeTest_DisposeDoesNotThrow_ExceptionUnmodified(
			Exception initialException
		) {
			Exception exception = initialException;
			( new Mock<IDisposable>().Object ).SafeDispose( ref exception );
			Assert.AreSame( initialException, exception );
		}

		private static IDisposable MockDisposable( Exception throws ) {
			Mock<IDisposable> mock = new Mock<IDisposable>();
			mock.Setup( d => d.Dispose() ).Throws( throws );
			return mock.Object;
		}

	}

}

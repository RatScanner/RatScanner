using System;

namespace RatScanner.FetchModels.TarkovTracker
{
	// Exception for when TarkovTracker rate limiting is hit
	public class RateLimitExceededException : Exception
	{
		public RateLimitExceededException() : base("Rate limit exceeded") { }

		public RateLimitExceededException(string message)
			: base(message) { }

		public RateLimitExceededException(string message, Exception inner)
			: base(message, inner) { }
	}

	// Exception for when TarkovTracker token is not accepted
	public class UnauthorizedTokenException : Exception
	{
		public UnauthorizedTokenException() : base("Unauthorized token") { }

		public UnauthorizedTokenException(string message)
			: base(message) { }

		public UnauthorizedTokenException(string message, Exception inner)
			: base(message, inner) { }
	}
}

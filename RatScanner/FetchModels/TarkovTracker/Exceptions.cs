using System;
using System.Net;

namespace RatScanner.FetchModels.TarkovTracker;

// Exception for when TarkovTracker rate limiting is hit
public class RateLimitExceededException : Exception {
	public RateLimitExceededException() : base("Rate limit exceeded") { }

	public RateLimitExceededException(string message)
		: base(message) { }

	public RateLimitExceededException(string message, Exception inner)
		: base(message, inner) { }
}

// Exception for when TarkovTracker rate limiting is hit
public class UnauthorizedTokenException : Exception {
	public UnauthorizedTokenException() : base("Unauthorized token") { }

	public UnauthorizedTokenException(string message)
		: base(message) { }

	public UnauthorizedTokenException(string message, Exception inner)
		: base(message, inner) { }
}

public class TrackerApiException : Exception {
	public HttpStatusCode StatusCode { get; }

	public TrackerApiException(HttpStatusCode statusCode, string message)
		: base(message) {
		StatusCode = statusCode;
	}
}

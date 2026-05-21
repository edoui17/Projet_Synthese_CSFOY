using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.API.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenDatabaseExceptionOccurs_ShouldLogAndReturnGeneric500Response()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var databaseException = new InvalidOperationException("Failed to connect to database...");

        RequestDelegate next = (HttpContext context) => throw databaseException;

        var middleware = new ExceptionHandlingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        // Verify logging occurred
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                databaseException,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        // Read and verify response body
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var jsonDocument = JsonDocument.Parse(responseBody);
        var root = jsonDocument.RootElement;

        Assert.Equal("Internal Server Error", root.GetProperty("error").GetString());
        Assert.Equal("An unexpected error occurred while processing your request.", root.GetProperty("message").GetString());
    }
}

using System.Net;
using Moq;
using Moq.Protected;

namespace APIHealthMonitoring.UnitTests.Common.Helpers;

/// <summary>
/// Helper to construct a mocked IHttpClientFactory that returns pre-configured HttpResponses.
/// </summary>
public static class MockHttpClientFactory
{
    public static IHttpClientFactory Create(
        HttpStatusCode statusCode,
        string responseContent = "",
        Exception? exceptionToThrow = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        var setup = handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        if (exceptionToThrow is not null)
        {
            setup.ThrowsAsync(exceptionToThrow);
        }
        else
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            };
            setup.ReturnsAsync(response);
        }

        var client = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        return factoryMock.Object;
    }
}

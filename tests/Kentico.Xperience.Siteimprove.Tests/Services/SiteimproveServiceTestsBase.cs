using System.Net;
using System.Text.Json;

using CMS.Base.Internal;
using CMS.Core;
using CMS.Tests;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Base class for <see cref="SiteimproveService"/> tests.
    /// </summary>
    public class SiteimproveServiceTestsBase : UnitTests
    {
        protected const string API_USER = "API_USER";
        protected const string API_KEY = "API_KEY";
        protected const string DOMAIN = "http://test.com/";

        private protected SiteimproveService service;
        protected IHttpClientFactory httpClientFactory;
        private protected SiteimproveOptions options;
        protected IEventLogService eventLogService;
        private IHttpContextRetriever httpContextRetriever;

        public static int numberOfRequests;


        protected override void RegisterTestServices()
        {
            base.RegisterTestServices();

            httpContextRetriever = Substitute.For<IHttpContextRetriever>();
            Service.Use<IHttpContextRetriever>(httpContextRetriever);
        }


        [SetUp]
        public void SetUp()
        {
            var iOptions = Substitute.For<IOptions<SiteimproveOptions>>();
            options = new SiteimproveOptions
            {
                APIUser = API_USER,
                APIKey = API_KEY,
                EnableContentCheck = true
            };

            iOptions.Value.Returns(options);

            eventLogService = Substitute.For<IEventLogService>();

            httpClientFactory = Substitute.For<IHttpClientFactory>();

            MockHttpClient(new List<HttpResponseMessage>());


            var httpContext = Substitute.For<IHttpContext>();
            var httpRequest = Substitute.For<IRequest>();
            httpRequest.Url.Returns(new Uri(DOMAIN));

            httpContext.Request.Returns(httpRequest);
            httpContextRetriever.GetContext().Returns(httpContext);

            numberOfRequests = 0;

            service = new SiteimproveService(httpClientFactory, iOptions, eventLogService);
        }


        protected static HttpResponseMessage GetMessage(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return GetMessage<object>(statusCode: statusCode);
        }


        protected static HttpResponseMessage GetMessage<T>(T content = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            where T : class
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonSerializer.Serialize(content))
            };
        }


        /// <summary>
        /// Used for mocking responses of httpClient. Needed to allow mocking multiple requests in the same method.
        /// </summary>
        /// <param name="responseMessages">Responses to use for requests, returned by the given order.</param>
        protected void MockHttpClient(IList<HttpResponseMessage> responseMessages)
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler(responseMessages));

            httpClient.BaseAddress = new Uri(SiteimproveConstants.BASE_URL);
            httpClientFactory.CreateClient(SiteimproveConstants.CLIENT_NAME).Returns(httpClient);
        }


        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private Queue<HttpResponseMessage> responseMessages;


            public MockHttpMessageHandler(IEnumerable<HttpResponseMessage> responseMessages)
            {
                this.responseMessages = new Queue<HttpResponseMessage>(responseMessages);
            }


            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Send(request, cancellationToken));
            }


            protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                numberOfRequests++;
                return responseMessages.Dequeue();
            }
        }
    }
}

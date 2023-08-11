using System.Net.Http.Headers;
using System.Text;

using CMS.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="StartupExtensions"/> class.
    /// </summary>
    public class StartupExtensionsTests
    {
        [TestFixture]
        public class AddSiteimproveTests : UnitTests
        {
            private const string APIKEY = "APIKey";
            private const string APIUSER = "APIUser";

            private IServiceCollection services;
            private IConfiguration configuration;


            [SetUp]
            public void Setup()
            {
                services = new ServiceCollection();
            }


            [Test]
            public void AddSiteimprove_ValidOptions_SetupOptionsAddHttpClient()
            {
                bool enableContentCheck = true;

                configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.APIUser)}", APIUSER },
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.APIKey)}", APIKEY },
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.EnableContentCheck)}", enableContentCheck ? "true" : "false" },
                    })
                    .Build();

                services.AddSiteimprove(configuration);

                var serviceProvider = services.BuildServiceProvider();
                var siteimproveOptions = serviceProvider.GetRequiredService<IOptions<SiteimproveOptions>>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(SiteimproveConstants.CLIENT_NAME);

                var byteArray = Encoding.UTF8.GetBytes($"{APIUSER}:{APIKEY}");

                var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                Assert.Multiple(() =>
                {
                    Assert.That(siteimproveOptions, Is.Not.Null);
                    Assert.That(siteimproveOptions.Value.APIKey, Is.EqualTo(APIKEY));
                    Assert.That(siteimproveOptions.Value.APIUser, Is.EqualTo(APIUSER));
                    Assert.That(siteimproveOptions.Value.EnableContentCheck, Is.EqualTo(enableContentCheck));
                    Assert.That(httpClient.DefaultRequestHeaders.Authorization, Is.EqualTo(authorization));
                    Assert.That(httpClient.BaseAddress, Is.EqualTo(new Uri(SiteimproveConstants.BASE_URL)));
                });
            }


            [TestCase(null, APIKEY, TestName = "AddSiteimprove_APIUserNull_ThrowsException")]
            [TestCase("", APIKEY, TestName = "AddSiteimprove_APIUserEmpty_ThrowsException")]
            [TestCase(APIUSER, null, TestName = "AddSiteimprove_APIKeyNull_ThrowsException")]
            [TestCase(APIUSER, "", TestName = "AddSiteimprove_APIKeyEmpty_ThrowsException")]
            [TestCase(null, null, TestName = "AddSiteimprove_APIUserAndAPIKeyNull_ThrowsException")]
            [TestCase("", "", TestName = "AddSiteimprove_APIUserAndAPIKeyEmpty_ThrowsException")]
            public void AddSiteimprove_InvalidOptions_SetupOptionsAddHttpClient(string apiKey, string apiUser)
            {
                bool enableContentCheck = true;

                configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.APIUser)}", apiUser },
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.APIKey)}", apiKey },
                        { $"{SiteimproveConstants.SECTION_KEY}:{nameof(SiteimproveOptions.EnableContentCheck)}", enableContentCheck ? "true" : "false" },
                    })
                    .Build();

                Assert.Multiple(() =>
                {
                    Assert.That(() => services.AddSiteimprove(configuration), Throws.InvalidOperationException);
                });
            }
        }
    }
}

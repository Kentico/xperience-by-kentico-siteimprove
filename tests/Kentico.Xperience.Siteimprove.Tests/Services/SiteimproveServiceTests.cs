using System.Net;

using CMS.Core;
using CMS.DataEngine;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveService"/> class.
    /// </summary>
    public class SiteimproveServiceTests
    {
        public class GetTokenTests : SiteimproveServiceTestsBase
        {
            private const string TOKEN = "token";

            private const int CATEGORY_ID = 2;

            private SettingsKeyInfoProviderFake settingsProvider;


            [SetUp]
            public void Setup()
            {
                settingsProvider = Substitute.ForPartsOf<SettingsKeyInfoProviderFake>();

                Fake<SettingsKeyInfo, SettingsKeyInfoProvider>(settingsProvider).WithData(new SettingsKeyInfo
                {
                    KeyName = SiteimproveConstants.TOKEN_SETTINGS_KEY_NAME,
                    KeyDisplayName = "{$siteimprove.settings.token$}",
                    KeyDescription = "{$siteimprove.settings.token.description$}",
                    KeyValue = TOKEN,
                    KeyType = "string",
                    KeyCategoryID = CATEGORY_ID,
                    KeyGUID = Guid.NewGuid(),
                    KeyLastModified = DateTime.UtcNow,
                    KeyIsHidden = true,
                    KeyExplanationText = string.Empty,
                    KeyIsCustom = true
                });

                Fake<SettingsCategoryInfo, SettingsCategoryInfoProvider>().WithData(new SettingsCategoryInfo
                {
                    CategoryID = 1,
                    CategoryDisplayName = "Test Root Category",
                    CategoryName = "Root",
                    CategoryOrder = 1,
                    CategoryIDPath = "/00000001",
                    CategoryLevel = 0,
                    CategoryChildCount = 1,
                    CategoryResourceID = 1,
                },
                new SettingsCategoryInfo
                {
                    CategoryID = CATEGORY_ID,
                    CategoryDisplayName = "Test Integrations Category",
                    CategoryName = SiteimproveConstants.CATEGORY_NAME,
                    CategoryOrder = 1,
                    CategoryParentID = 1,
                    CategoryIDPath = $"/00000001/0000000{CATEGORY_ID}",
                    CategoryLevel = 1,
                    CategoryChildCount = 0,
                    CategoryResourceID = 1,
                });
            }


            [Test]
            public async Task GetToken_TokenInDatabase_ReturnsCorrectToken()
            {
                string token = await service.GetToken();

                Assert.Multiple(() =>
                {
                    Assert.That(token, Is.EqualTo(TOKEN));
                    Assert.That(numberOfRequests, Is.EqualTo(0));

                    eventLogService.Received(0).LogEvent(Arg.Any<EventLogData>());
                    settingsProvider.ReceivedWithAnyArgs(0).SetFake(default);
                });
            }


            [Test]
            public async Task GetToken_TokenNotInDatabaseRequestSuccessful_ReturnsCorrectToken()
            {
                Fake<SettingsKeyInfo, SettingsKeyInfoProvider>(settingsProvider).WithData();

                MockHttpClient(new List<HttpResponseMessage>
                {
                    GetMessage(new SiteimproveToken
                    {
                        Value = TOKEN
                    })
                });

                string token = await service.GetToken();

                Assert.Multiple(() =>
                {
                    Assert.That(token, Is.EqualTo(TOKEN));
                    Assert.That(numberOfRequests, Is.EqualTo(1));

                    eventLogService.Received(0).LogEvent(Arg.Any<EventLogData>());
                    settingsProvider.Received(1).SetFake(Arg.Is<SettingsKeyInfo>(i =>
                        string.Equals(i.KeyName, SiteimproveConstants.TOKEN_SETTINGS_KEY_NAME, StringComparison.Ordinal)
                        && string.Equals(i.KeyValue, TOKEN, StringComparison.Ordinal)
                        && i.KeyCategoryID == CATEGORY_ID
                        && i.KeyIsCustom == true
                        && i.KeyIsHidden == true
                    ));
                });
            }


            [Test]
            public async Task GetToken_TokenNotInDatabaseRequestFailed_ReturnsEmptyToken()
            {
                Fake<SettingsKeyInfo, SettingsKeyInfoProvider>(settingsProvider).WithData();

                MockHttpClient(new List<HttpResponseMessage>
                {
                    GetMessage(HttpStatusCode.BadRequest)
                });

                string token = await service.GetToken();

                Assert.Multiple(() =>
                {
                    Assert.That(token, Is.Empty);
                    Assert.That(numberOfRequests, Is.EqualTo(1));

                    eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(i => i.EventType == EventTypeEnum.Error));
                    settingsProvider.ReceivedWithAnyArgs(0).SetFake(default);
                });
            }


            internal abstract class SettingsKeyInfoProviderFake : SettingsKeyInfoProvider
            {
                public abstract void SetFake(SettingsKeyInfo info);


                protected override void SetSettingsKeyInfoInternal(SettingsKeyInfo info)
                {
                    SetFake(info);
                }
            }
        }


        [TestFixture]
        public class CheckPagesTests : SiteimproveServiceTestsBase
        {
            [TestCaseSource(nameof(TestCases))]
            public async Task CheckPages_TestCases(IEnumerable<string> urls, IList<HttpResponseMessage> responseMessages)
            {
                int errors = responseMessages.Where(r => r?.StatusCode != HttpStatusCode.OK).Count();

                MockHttpClient(responseMessages);

                await service.CheckPages(urls);

                Assert.Multiple(() =>
                {
                    Assert.That(numberOfRequests, Is.EqualTo(urls.Count()));
                    Assert.That(numberOfRequests, Is.EqualTo(responseMessages.Count));

                    httpClientFactory.Received(1).CreateClient(SiteimproveConstants.CLIENT_NAME);
                    eventLogService.Received(errors).LogEvent(Arg.Is<EventLogData>(i => i.EventType == EventTypeEnum.Error));
                });
            }


            public static IEnumerable<TestCaseData> TestCases
            {
                get
                {
                    yield return new TestCaseData(
                        new List<string>
                        {
                            "http://test.com/",
                            "http://test.com/welcome",
                            "http://test.com/contact"
                        },
                        new List<HttpResponseMessage>
                        {
                            GetMessage(),
                            GetMessage(),
                            GetMessage()
                        })
                        .SetName("CheckPages_AllRequestsSuccessful_NoExceptionLogged");

                    yield return new TestCaseData(
                        new List<string>
                        {
                            "http://test.com/",
                            "http://test.com/error",
                            "http://test.com/welcome",
                            "http://test.com/contact"
                        },
                        new List<HttpResponseMessage>
                        {
                            GetMessage(),
                            GetMessage(HttpStatusCode.BadRequest),
                            GetMessage(),
                            GetMessage()
                        })
                        .SetName("CheckPages_OneRequestFailed_OneExceptionLogged");

                    yield return new TestCaseData(
                        new List<string>
                        {
                            "http://error.com/",
                            "http://error.com/welcome",
                            "http://error.com/welcome"
                        },
                        new List<HttpResponseMessage>
                        {
                            GetMessage(HttpStatusCode.BadRequest),
                            GetMessage(HttpStatusCode.BadRequest),
                            null,
                        })
                        .SetName("CheckPages_AllRequestsFailed_AllExceptionsLogged");
                }
            }
        }


        [TestFixture]
        public class IsContentCheckEnabledInternalTests : SiteimproveServiceTestsBase
        {
            [TestCaseSource(nameof(TestCases))]
            public async Task IsContentCheckEnabledInternal_TestCases(bool expectedResult, IList<HttpResponseMessage> responseMessages, bool enableContentCheck)
            {
                int errors = responseMessages.Where(r => r?.StatusCode != HttpStatusCode.OK).Count();

                MockHttpClient(responseMessages);

                options.EnableContentCheck = enableContentCheck;

                bool result = await service.IsContentCheckEnabledInternal();

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(expectedResult));
                    Assert.That(numberOfRequests, Is.EqualTo(responseMessages.Count));

                    httpClientFactory.Received(enableContentCheck ? 1 : 0).CreateClient(SiteimproveConstants.CLIENT_NAME);
                    eventLogService.Received(errors).LogEvent(Arg.Is<EventLogData>(i => i.EventType == EventTypeEnum.Error));
                });
            }


            [Test]
            public async Task IsContentCheckEnabledInternal_NotReadyNotSubscribed_ReturnsFalse()
            {
                MockHttpClient(new List<HttpResponseMessage>
                {
                    GetMessage(new SiteimproveContentCheck
                    {
                        IsReady = false,
                    }),
                    GetMessage(HttpStatusCode.PaymentRequired)
                });

                bool result = await service.IsContentCheckEnabledInternal();

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(false));
                    Assert.That(numberOfRequests, Is.EqualTo(2));

                    httpClientFactory.Received(1).CreateClient(SiteimproveConstants.CLIENT_NAME);
                    eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(i =>
                        i.EventType == EventTypeEnum.Error
                        && string.Equals(i.EventDescription, "Cannot enable content check. User not subscribed to Prepublish feature.", StringComparison.Ordinal)));
                });
            }


            public static IEnumerable<TestCaseData> TestCases
            {
                get
                {
                    yield return new TestCaseData(false, new List<HttpResponseMessage>(), false)
                        .SetName("IsContentCheckEnabledInternal_OptionsDisabled_ReturnsFalse");

                    yield return new TestCaseData(
                        true,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = true
                            })
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_ReadyRequestSuccessful_ReturnsTrue");

                    yield return new TestCaseData(
                        true,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false,
                            }),
                            GetMessage(),
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = true
                            })
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyAllRequestsSuccessful_ReturnsTrue");

                    yield return new TestCaseData(
                        false,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false,
                            }),
                            GetMessage(),
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false
                            })
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyEnableNotReadyAllRequestsSuccessful_ReturnsFalse");

                    yield return new TestCaseData(
                        false,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false,
                            }),
                            GetMessage(HttpStatusCode.BadRequest)
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyEnableRequestFailed_ReturnsFalse");


                    yield return new TestCaseData(
                        false,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false,
                            }),
                            null
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyEnableRequestNull_ReturnsFalse");

                    yield return new TestCaseData(
                        true,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(HttpStatusCode.BadRequest),
                            GetMessage(),
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = true
                            })
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyFirstRequestFailed_ReturnsTrue");

                    yield return new TestCaseData(
                        false,
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveContentCheck
                            {
                                IsReady = false
                            }),
                            GetMessage(),
                            GetMessage(HttpStatusCode.BadRequest)
                        },
                        true)
                        .SetName("IsContentCheckEnabledInternal_NotReadyLastRequestFailed_ReturnsFalse");
                }
            }
        }


        [TestFixture]
        public class GetSideIDTests : SiteimproveServiceTestsBase
        {
            private static long invalidID = -1;

            [TestCaseSource(nameof(TestCases))]
            public async Task GetSiteID_TestCases(IList<HttpResponseMessage> responseMessages, long expectedResult)
            {
                int errors = responseMessages.Where(r => r?.StatusCode != HttpStatusCode.OK).Count();

                MockHttpClient(responseMessages);

                long result = await service.GetSiteID();

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(expectedResult));
                    Assert.That(numberOfRequests, Is.EqualTo(responseMessages.Count));

                    httpClientFactory.Received(1).CreateClient(SiteimproveConstants.CLIENT_NAME);
                    eventLogService.Received(errors).LogEvent(Arg.Is<EventLogData>(i => i.EventType == EventTypeEnum.Error));

                });
            }


            [Test]
            public async Task GetSiteID_AllRequestsSuccessfulSiteNotFound_ReturnsInvalidID()
            {
                MockHttpClient(new List<HttpResponseMessage>
                {
                    GetMessage(new SiteimproveSites
                    {
                        TotalItems = 3,
                    }),
                    GetMessage(new SiteimproveSites
                    {
                        Sites = new List<SiteimproveSite>
                        {
                            new SiteimproveSite
                            {
                                SiteID = 123,
                                SiteUrl = "https://1568989.com"
                            },
                            new SiteimproveSite
                            {
                                SiteID = 1234,
                                SiteUrl = "https://notThisOneEither.com"
                            },
                            new SiteimproveSite
                            {
                                SiteID = 136,
                                SiteUrl = "https://alsoNotThisOne.com",
                            }
                        },
                        TotalItems = 3
                    })
                });

                long result = await service.GetSiteID();

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(invalidID));
                    Assert.That(numberOfRequests, Is.EqualTo(2));

                    httpClientFactory.Received(1).CreateClient(SiteimproveConstants.CLIENT_NAME);
                    eventLogService.Received(1).LogEvent(Arg.Is<EventLogData>(i =>
                        i.EventType == EventTypeEnum.Error
                        && string.Equals(i.EventDescription, $"Site with URL '{new Uri(DOMAIN).Host}' could not be found on Siteimprove.", StringComparison.Ordinal)));
                });
            }


            public static IEnumerable<TestCaseData> TestCases
            {
                get
                {
                    yield return new TestCaseData(
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveSites
                            {
                                TotalItems = 3,
                            }),
                            GetMessage(new SiteimproveSites
                            {
                                Sites = new List<SiteimproveSite>
                                {
                                    new SiteimproveSite
                                    {
                                        SiteID = 123,
                                        SiteUrl = "https://1568989.com"
                                    },
                                    new SiteimproveSite
                                    {
                                        SiteID = 1234,
                                        SiteUrl = "https://notThisOneEither.com"
                                    },
                                    new SiteimproveSite
                                    {
                                        SiteID = 1,
                                        SiteUrl = DOMAIN,
                                    }
                                },
                                TotalItems = 3
                            })
                        },
                        1)
                        .SetName("GetSiteID_AllRequestsSuccessfulSiteFound_ReturnsCorrectID");

                    yield return new TestCaseData(
                        new List<HttpResponseMessage>
                        {
                            GetMessage(HttpStatusCode.BadRequest)
                        },
                        invalidID)
                        .SetName("GetSiteID_FirstRequestFailed_ReturnsInvalidID");

                    yield return new TestCaseData(
                        new List<HttpResponseMessage>
                        {
                            GetMessage(new SiteimproveSites
                            {
                                TotalItems = 3,
                            }),
                            GetMessage(HttpStatusCode.BadRequest)
                        },
                        invalidID)
                        .SetName("GetSiteID_SecondRequestFailed_ReturnsInvalidID");
                }
            }
        }
    }
}

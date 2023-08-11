using CMS.Core;
using CMS.Tests;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Siteimprove.Tests
{
    /// <summary>
    /// Tests for <see cref="SiteimproveController"/> class.
    /// </summary>
    public class SiteimproveControllerTests
    {
        [TestFixture]
        public class ConfigurationScriptTests : UnitTests
        {
            private SiteimproveController controller;
            private ISiteimproveScriptsProvider scriptsProvider;


            protected override void RegisterTestServices()
            {
                base.RegisterTestServices();

                scriptsProvider = Substitute.For<ISiteimproveScriptsProvider>();
                Service.Use<ISiteimproveScriptsProvider>(scriptsProvider);
            }


            [SetUp]
            public void SetUp()
            {
                controller = new SiteimproveController();
            }


            [Test]
            public async Task ConfigurationScript_Valid_ReturnsCorrectContent()
            {
                int pageID = 20;
                var script = $"const test = 'test_{pageID}';";

                scriptsProvider.GetConfigurationScript(pageID, CancellationToken.None).Returns(script);

                var result = await controller.ConfigurationScript(pageID, CancellationToken.None) as ContentResult;

                Assert.Multiple(async () =>
                {
                    Assert.That(result.Content, Is.EqualTo(script));
                    Assert.That(result.ContentType, Is.EqualTo("application/javascript"));

                    await scriptsProvider.Received(1).GetConfigurationScript(pageID, CancellationToken.None);
                });
            }
        }
    }
}

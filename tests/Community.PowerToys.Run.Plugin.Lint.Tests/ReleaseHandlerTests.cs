using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    public class ReleaseHandlerTests
    {
        [Test]
        public async Task GetPackagesAsync_should_return_no_packages_when_release_is_null()
        {
            var subject = new ReleaseHandler(null, NullLogger.Instance);
            var result = await subject.GetPackagesAsync();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetPackagesAsync_should_return_packages()
        {
            var release = new Release
            {
                assets =
                [
                    new Asset
                    {
                        browser_download_url = "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update/releases/download/v0.2.0/Sample-0.2.0-x64.zip",
                        name = "Sample-0.2.0-x64.zip",
                        content_type = "application/x-zip-compressed",
                    }
                ],
            };

            var subject = new ReleaseHandler(release, NullLogger.Instance);
            var result = await subject.GetPackagesAsync();
            result.Should().NotBeEmpty();
        }

        [Test]
        public async Task GetPackagesAsync_should_return_no_packages_when_asset_has_wrong_content_type()
        {
            var release = new Release
            {
                assets =
                [
                    new Asset
                    {
                        browser_download_url = "https://github.com/microsoft/PowerToys/releases/download/v0.84.1/PowerToysSetup-0.84.1-x64.exe",
                        name = "PowerToysSetup-0.84.1-x64.exe",
                        content_type = "application/x-msdownload",
                    }
                ],
            };

            var subject = new ReleaseHandler(release, NullLogger.Instance);
            var result = await subject.GetPackagesAsync();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetChecksumsAsync_should_return_no_checksums_when_release_is_null()
        {
            var subject = new ReleaseHandler(null, NullLogger.Instance);
            var result = await subject.GetChecksumsAsync();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetChecksumsAsync_should_return_no_checksums_when_file_is_missing()
        {
            var subject = new ReleaseHandler(new() { assets = [] }, NullLogger.Instance);
            var result = await subject.GetChecksumsAsync();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetChecksumsAsync_should_return_checksums()
        {
            var release = new Release
            {
                assets =
                [
                    new Asset
                    {
                        browser_download_url = "https://github.com/neilenns/DiscordTimestamp/releases/download/v1.1.1/checksums.txt",
                        name = "checksums.txt",
                    }
                ],
            };

            var subject = new ReleaseHandler(release, NullLogger.Instance);
            var result = await subject.GetChecksumsAsync();
            result.Should().NotBeEmpty();
        }
    }
}

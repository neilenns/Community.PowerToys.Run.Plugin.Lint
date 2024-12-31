using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    public class ExtensionsTests
    {
        [Test]
        public void IsPersonalAccessToken_should_validate_arg()
        {
            "ghp_FOOBAR".IsPersonalAccessToken().Should().BeTrue();
            "github_pat_FOOBAR".IsPersonalAccessToken().Should().BeTrue();
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".IsPersonalAccessToken().Should().BeFalse();
            @"..\..\..\Packages\Valid-0.87.0-x64.zip".IsPersonalAccessToken().Should().BeFalse();
            "".IsPersonalAccessToken().Should().BeFalse();
            ((string)null!).IsPersonalAccessToken().Should().BeFalse();
        }

        [Test]
        public void IsUrl_should_validate_arg()
        {
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".IsUrl().Should().BeTrue();
            @"..\..\..\Packages\Valid-0.87.0-x64.zip".IsUrl().Should().BeFalse();
            "".IsUrl().Should().BeFalse();
            ((string)null!).IsUrl().Should().BeFalse();
        }

        [Test]
        public void IsFile_should_validate_arg()
        {
            @"..\..\..\Packages\Valid-0.87.0-x64.zip".IsFile().Should().BeTrue();
            @"..\..\..\Packages".IsFile().Should().BeFalse();
            @"..\..\..\Fail\Valid-0.87.0-x64.zip".IsFile().Should().BeFalse();
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".IsFile().Should().BeFalse();
            "".IsFile().Should().BeFalse();
            ((string)null!).IsFile().Should().BeFalse();
        }

        [Test]
        public void IsDirectory_should_validate_arg()
        {
            @"..\..\..\..\..\src\Community.PowerToys.Run.Plugin.Lint\".IsDirectory().Should().BeTrue();
            @"..\..\..\Packages\Valid-0.87.0-x64.zip".IsDirectory().Should().BeFalse();
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".IsDirectory().Should().BeFalse();
            "".IsDirectory().Should().BeFalse();
            ((string)null!).IsDirectory().Should().BeFalse();
        }

        [Test]
        public void GetGitHubOptions_should_parse_URL()
        {
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Install" });

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugins#bang".GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugins" });

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugins?tab=readme-ov-file#bang".GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugins" });

            "https://gitfail.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubOptions().Should().BeNull();

            "".GetGitHubOptions().Should().BeNull();

            ((string)null!).GetGitHubOptions().Should().BeNull();
        }

        [Test]
        public void GetGitHubOptions_should_include_config()
        {
            var settings = new Dictionary<string, string?>
            {
                { "GitHubOptions:PersonalAccessToken", "PersonalAccessToken" },
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install".GetGitHubOptions(config).Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Install", PersonalAccessToken = "PersonalAccessToken" });
        }

        [Test]
        public void GetEmbeddedResourceContent_should_return_file_content()
        {
            "Directory.Packages.props.xml".GetEmbeddedResourceContent().Should().NotBeEmpty();
            "Directory.Packages.props.json".GetEmbeddedResourceContent().Should().BeNull();
            "".GetEmbeddedResourceContent().Should().BeNull();
            ((string)null!).GetEmbeddedResourceContent().Should().BeNull();
        }

        [Test]
        public void IsZip_should_determine_if_Asset_is_zip_file()
        {
            new Asset { content_type = "application/x-zip-compressed" }.IsZip().Should().BeTrue();
            new Asset { name = "Sample-0.2.0-x64.zip" }.IsZip().Should().BeTrue();
            new Asset { name = "JetbrainsProjects-v1.0.1-x64.zip", content_type = "binary/octet-stream" }.IsZip().Should().BeTrue();
            new Asset { name = "", content_type = "" }.IsZip().Should().BeFalse();
            new Asset().IsZip().Should().BeFalse();
            ((Asset)null!).IsZip().Should().BeFalse();
        }

        [Test]
        public void IsChecksumsFile_should_determine_if_Asset_is_checksums_file()
        {
            new Asset { name = "checksums.txt" }.IsChecksumsFile().Should().BeTrue();
            new Asset { name = "checksums.zip" }.IsChecksumsFile().Should().BeFalse();
            new Asset { name = "" }.IsChecksumsFile().Should().BeFalse();
            new Asset().IsChecksumsFile().Should().BeFalse();
            ((Asset)null!).IsChecksumsFile().Should().BeFalse();
        }

        [Test]
        public void HasValidTargetFramework_should_validate_Assembly()
        {
            new Package(@"..\..\..\Packages\Valid-0.87.0-x64.zip").Load()
                .HasValidTargetFramework().Should().BeTrue();
            new Package(@"..\..\..\Packages\InvalidTarget-0.82.1-x64.zip").Load()
                .HasValidTargetFramework().Should().BeFalse();
            new Package("Community.PowerToys.Run.Plugin.Lint.Tests.dll")
                .HasValidTargetFramework().Should().BeFalse();
            ((Package)null!)
                .HasValidTargetFramework().Should().BeFalse();
        }

        [Test]
        public void HasValidTargetPlatform_should_validate_Assembly()
        {
            new Package(@"..\..\..\Packages\Valid-0.87.0-x64.zip").Load()
                .HasValidTargetPlatform().Should().BeTrue();
            new Package(@"..\..\..\Packages\InvalidTarget-0.82.1-x64.zip").Load()
                .HasValidTargetPlatform().Should().BeFalse();
            new Package("Community.PowerToys.Run.Plugin.Lint.Tests.dll")
                .HasValidTargetPlatform().Should().BeFalse();
            ((Package)null!)
                .HasValidTargetPlatform().Should().BeFalse();
        }

        [Test]
        public void HasValidAuthor_should_validate_Author()
        {
            new Metadata { Author = "hlaueriksson" }.HasValidAuthor(new() { login = "hlaueriksson" }).Should().BeTrue();
            new Metadata { Author = "Henrik Lau Eriksson" }.HasValidAuthor(new() { name = "Henrik Lau Eriksson" }).Should().BeTrue();
            new Metadata { Author = "Foo" }.HasValidAuthor(new() { login = "Bar" }).Should().BeFalse();
            new Metadata { Author = "Foo" }.HasValidAuthor(new()).Should().BeFalse();
            new Metadata { Author = "Foo" }.HasValidAuthor(null!).Should().BeFalse();
            new Metadata { Author = "" }.HasValidAuthor(new() { name = "" }).Should().BeFalse();
            new Metadata().HasValidAuthor(new()).Should().BeFalse();
            ((Metadata)null!).HasValidAuthor(null!).Should().BeFalse();
        }
    }
}

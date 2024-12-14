using FluentAssertions;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    public class ExtensionsTests
    {
        [Test]
        public void GetGitHubOptions_should_parse_URL()
        {
            "https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update".GetGitHubOptions().Should()
                .BeEquivalentTo(new GitHubOptions { Owner = "hlaueriksson", Repo = "Community.PowerToys.Run.Plugin.Update" });

            Action act = () => "https://gitfail.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update".GetGitHubOptions();
            act.Should().Throw<ArgumentException>();

            act = () => "".GetGitHubOptions();
            act.Should().Throw<ArgumentException>();

            act = () => ((string)null!).GetGitHubOptions();
            act.Should().Throw<ArgumentNullException>();
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
        public void HasValidTargetFramework_should_validate_Assembly()
        {
            new Package(new(), @"..\..\..\Packages\Valid-0.82.1-x64.zip").Load()
                .HasValidTargetFramework().Should().BeTrue();
            new Package(new(), @"..\..\..\Packages\InvalidTarget-0.82.1-x64.zip").Load()
                .HasValidTargetFramework().Should().BeFalse();
            new Package(new(), "Community.PowerToys.Run.Plugin.Lint.Tests.dll")
                .HasValidTargetFramework().Should().BeFalse();
            ((Package)null!)
                .HasValidTargetFramework().Should().BeFalse();
        }

        [Test]
        public void HasValidTargetPlatform_should_validate_Assembly()
        {
            new Package(new(), @"..\..\..\Packages\Valid-0.82.1-x64.zip").Load()
                .HasValidTargetPlatform().Should().BeTrue();
            new Package(new(), @"..\..\..\Packages\InvalidTarget-0.82.1-x64.zip").Load()
                .HasValidTargetPlatform().Should().BeFalse();
            new Package(new(), "Community.PowerToys.Run.Plugin.Lint.Tests.dll")
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

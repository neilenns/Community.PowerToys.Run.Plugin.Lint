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
    }
}

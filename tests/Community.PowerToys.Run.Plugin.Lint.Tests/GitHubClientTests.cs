using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    public class GitHubClientTests
    {
        private GitHubClient _subject;
        private GitHubClient _subjectWithInvalidOptions;

        [SetUp]
        public void SetUp()
        {
            _subject = new GitHubClient(new GitHubOptions { Owner = "hlaueriksson", Repo = "GEmojiSharp" }, NullLogger.Instance);
            _subjectWithInvalidOptions = new GitHubClient(new GitHubOptions { Owner = "fail-hlaueriksson", Repo = "Fail-GEmojiSharp" }, NullLogger.Instance);
        }

        [Test]
        public async Task GetUserAsync_should_return_user()
        {
            var result = await _subject.GetUserAsync();
            result.Should().NotBeNull();

            result = await _subjectWithInvalidOptions.GetUserAsync();
            result.Should().BeNull();
        }

        [Test]
        public async Task GetRepositoryAsync_should_return_repo()
        {
            var result = await _subject.GetRepositoryAsync();
            result.Should().NotBeNull();

            result = await _subjectWithInvalidOptions.GetRepositoryAsync();
            result.Should().BeNull();
        }

        [Test]
        public async Task GetReadmeAsync_should_return_readme()
        {
            var result = await _subject.GetReadmeAsync();
            result.Should().NotBeNull();

            result = await _subjectWithInvalidOptions.GetReadmeAsync();
            result.Should().BeNull();
        }

        [Test]
        public async Task GetLatestReleaseAsync_should_return_latest_release()
        {
            var result = await _subject.GetLatestReleaseAsync();
            result.Should().NotBeNull();

            result = await _subjectWithInvalidOptions.GetLatestReleaseAsync();
            result.Should().BeNull();
        }
    }
}

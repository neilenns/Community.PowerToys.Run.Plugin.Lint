namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    [Category("Integration")]
    public class VerifyTests
    {
        [Test]
        public async Task Verify_Directory_Packages_props()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://raw.githubusercontent.com/microsoft/PowerToys/main/Directory.Packages.props");
            var content = await response.Content.ReadAsStringAsync();

            await Verify(content);
        }
    }
}

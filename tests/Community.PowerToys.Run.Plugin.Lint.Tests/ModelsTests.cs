using FluentAssertions;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    public class ModelsTests
    {
        [Test]
        public void Package_ctor()
        {
            var subject = new Package(@"..\..\..\Packages\Valid-0.87.0-x64.zip");
            subject.FileInfo.Should().NotBeNull();
        }

        [Test]
        public void Package_Load()
        {
            var subject = new Package(@"..\..\..\Packages\Valid-0.87.0-x64.zip");
            subject.Load();
            subject.FileStream.Should().NotBeNull();
            subject.ZipArchive.Should().NotBeNull();
            subject.Metadata.Should().NotBeNull();
            subject.AssemblyDefinition.Should().NotBeNull();

            subject = new Package(@"..\..\..\Community.PowerToys.Run.Plugin.Lint.Tests.csproj");
            Action act = () => subject.Load();
            act.Should().Throw<InvalidDataException>();
        }

        [Test]
        public void Package_Dispose()
        {
            var subject = new Package(@"..\..\..\Packages\Valid-0.87.0-x64.zip");
            subject.Load();
            subject.Dispose();
        }
    }
}

using System.Diagnostics;
using System.Text;
using FluentAssertions;

namespace Community.PowerToys.Run.Plugin.Lint.Tests
{
    [Explicit, Category("Integration")]
    public class IntegrationTests
    {
        [Test]
        public void Community_PowerToys_Run_Plugin_Install()
        {
            var (ExitCode, StandardOutput, StandardError) = Run("https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install");
            ExitCode.Should().Be(0);
            StandardOutput.Should().Contain("Linting");
            StandardError.Should().BeEmpty();
        }

        [Test]
        public void GEmojiSharp()
        {
            var (ExitCode, StandardOutput, StandardError) = Run("https://github.com/hlaueriksson/GEmojiSharp");
            ExitCode.Should().Be(2);
            StandardOutput.Should().ContainAll(["PTRUN1401"]);
            StandardError.Should().BeEmpty();
        }

        [Test]
        public void Valid()
        {
            var (ExitCode, StandardOutput, StandardError) = Run(@"..\..\..\Packages\Valid-0.87.0-x64.zip");
            ExitCode.Should().Be(1);
            StandardOutput.Should().ContainAll(["PTRUN1401"]);
            StandardError.Should().BeEmpty();
        }

        static (int ExitCode, string StandardOutput, string StandardError) Run(string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "Community.PowerToys.Run.Plugin.Lint.exe",
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using var exeProcess = Process.Start(startInfo);
            exeProcess!.WaitForExit();
            return new(exeProcess.ExitCode, exeProcess.StandardOutput.ReadToEnd(), exeProcess.StandardError.ReadToEnd());
        }
    }
}

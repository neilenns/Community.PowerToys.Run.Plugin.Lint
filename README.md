# Community.PowerToys.Run.Plugin.Lint

[![build](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Lint/actions/workflows/build.yml/badge.svg)](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Lint/actions/workflows/build.yml)
[![Community.PowerToys.Run.Plugin.Lint](https://img.shields.io/nuget/v/Community.PowerToys.Run.Plugin.Lint.svg?label=Community.PowerToys.Run.Plugin.Lint)](https://www.nuget.org/packages/Community.PowerToys.Run.Plugin.Lint)

> Linter for PowerToys Run community plugins

## Installation

Install:

```cmd
dotnet tool install -g Community.PowerToys.Run.Plugin.Lint
```

The tool is installed as `ptrun-lint.exe` in `%UserProfile%\.dotnet\tools`.

Update:

```cmd
dotnet tool update -g Community.PowerToys.Run.Plugin.Lint
```

Uninstall:

```cmd
dotnet tool uninstall -g Community.PowerToys.Run.Plugin.Lint
```

## Usage

```cmd
ptrun-lint <url>
```

Arguments:

- `<url>` - An URL to a GitHub repo

Example:

```cmd
ptrun-lint https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Update
```

During linting, GitHub release assets are downloaded to `%LocalAppData%\Temp`.
After linting is completed, downloaded files are deleted.

Logs are written to a file, `ptrun-lint.log`, in the same directory as the tool was executed.

## Rules

| Code | Description |
| --- | --- |
| `PTRUN0001` | Args should be valid |
| | Args missing |
| | GitHub repo URL missing |
| `PTRUN1001` | Repo should be valid |
| | Repository missing |
| `PTRUN1002` | Repo details should be valid |
| | Repository missing |
| | Topic "`powertoys-run-plugin`" missing |
| | License missing |
| `PTRUN1101` | Readme should be valid |
| | Readme missing |
| | Installation instructions missing |
| | Usage instructions missing |
| `PTRUN1201` | Release should be valid |
| | Release missing |
| | Asset missing |
| | Asset "`.zip`" missing |
| | Asset "`arm64`" platform missing |
| | Asset "`x64`" platform missing |
| `PTRUN1202` | Release notes should be valid (`<package>`) |
| | Release notes missing |
| | Package missing |
| `PTRUN1301` | Package should be valid (`<package>`) |
| | Package missing |
| | Filename does not match "`<name>-<version>-<platform>.zip`" convention |
| `PTRUN1302` | Package content should be valid (`<package>`) |
| | Package missing |
| | Plugin folder missing |
| | Metadata "`plugin.json`" missing |
| | Assembly "`.dll`" missing |
| `PTRUN1303` | Package checksum should be valid (`<package>`) |
| | Release notes missing |
| | Package missing |
| | Hash "`<hash>`" missing |
| `PTRUN1401` | Plugin metadata should be valid (`<package>`) |
| | Package missing |
| | Repository missing |
| | User missing |
| | ID is invalid |
| | ActionKeyword is not unique |
| | Name does not match plugin folder |
| | Author does not match GitHub user |
| | Version is invalid |
| | Version does not match filename version |
| | Website does not match repo URL |
| | ExecuteFileName missing in package |
| | ExecuteFileName does not match "`Community.PowerToys.Run.Plugin.<Name>.dll`" convention |
| | IcoPathDark missing in package |
| | IcoPathLight missing in package |
| | DynamicLoading is unnecessary |
| `PTRUN1402` | Package dependencies should be valid (`<package>`) |
| | Package missing |
| | Unnecessary dependency: `PowerToys.Common.UI.dll` |
| | Unnecessary dependency: `PowerToys.ManagedCommon.dll` |
| | Unnecessary dependency: `PowerToys.Settings.UI.Lib.dll` |
| | Unnecessary dependency: `Wox.Infrastructure.dll` |
| | Unnecessary dependency: `Wox.Plugin.dll` |
| | Unnecessary dependency: `Newtonsoft.Json`, consider using `System.Text.Json` |
| | Unnecessary dependency: `<dependency>`, already defined in Central Package Management (`Directory.Packages.props`) |
| `PTRUN1501` | Plugin assembly should be valid (`<package>`) |
| | Assembly could not be validated |
| | Target framework should be "`net8.0`" |
| | Target platform should be "`windows`" |
| | Main.PluginID does not match metadata (`plugin.json`) ID |

## Disclaimer

This is not an official Microsoft PowerToys tool.

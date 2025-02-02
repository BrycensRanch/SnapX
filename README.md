<p align="center"><a href="https://getsharex.com"><img src="./Linux.png" alt="SnapX Banner"/></a></p>
<h1 align="center">SnapX</h1>
<h3 align="center">Capture, share, and boost productivity. All in one.</h3>
<br>
<div align="center">
  <a href="https://github.com/BrycensRanch/SnapX/actions/workflows/build.yml"><img src="https://img.shields.io/github/actions/workflow/status/BrycensRanch/SnapX/build.yml?branch=develop&label=Build&cacheSeconds=3600" alt="GitHub Workflow Status"/></a>
  <a href="./LICENSE.txt"><img src="https://img.shields.io/github/license/BrycensRanch/SnapX?label=License&color=brightgreen&cacheSeconds=3600" alt="License"/></a>
  <a href="https://github.com/BrycensRanch/SnapX/releases/latest"><img src="https://img.shields.io/github/v/release/BrycensRanch/SnapX?label=Release&color=brightgreen&cacheSeconds=3600" alt="Release"/></a>
  <a href="https://getsharex.com/downloads"><img src="https://img.shields.io/github/downloads/BrycensRanch/SnapX/total?label=Downloads&cacheSeconds=3600" alt="Downloads"/></a>
  <a href="https://discord.gg/ys3ZCzttVQ"><img src="https://img.shields.io/discord/1267996919922430063?label=Discord&cacheSeconds=3600" alt="Discord"/></a>
</div>
<br>
<p align="center"><a href="https://github.com/BrycensRanch/SnapX"><img src="https://getsharex.com/img/ShareX_Screenshot.png" alt="Repo"/></a></p>

# :construction: This project is under development and is not ready for use. :construction:

## :warning: Disclaimer

SnapX is a [hard fork](https://producingoss.com/en/forks.html) of the application [ShareX](https://github.com/ShareX/ShareX).

## Feature wise

- SnapX is a cross-platform application.
- Elegance in user interfaces by separating essential settings from advanced or intermediate functionality
- Supporting high DPI screens
- Automatically tone maps HDR screenshots to SDR*

[1]: When tested on KDE Plasma Wayland 6.2.90 with HDR on the resulting screenshot's colors were not blown out. Your mileage may vary.

## Technical Details

- It uses [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview), [ImageSharp](https://docs.sixlabors.com/articles/imagesharp/?tabs=tabid-1) (cross-platform image library)
- Dependency on Newtonsoft.JSON dropped, traded out for [more strict yet performant System.Text.Json](https://dev.to/samira_talebi_cca34ce28b8/newtonsoftjson-vs-systemtextjson-in-net-80-which-should-you-choose-26a3)
- And it *will* use [SQLite](https://www.sqlite.org/about.html) to [store settings & history](https://github.com/BrycensRanch/SnapX/issues/28).
- The UI is now defined in a more modern, declarative style using MVVM and XAML, providing a clear improvement over the older WinForms approach. For SnapX.GTK4, it uses [BindingSharp](https://github.com/BrycensRanch/BindingSharp)
- Uses [Serilog](https://github.com/serilog/serilog) for structured logging
- The ability to fully configure SnapX via the Command Line via command flags & environment variables. Additionally, you can configure SnapX using the Windows Registry.
- Additionally, all uploaders are now forced to use HTTPS <2.0 & *optionally* uses TLS 1.3 out of the box.
- Keeps compatability with the custom uploader configuration format (.sxcu)
- As a user, you do **NOT** need to have .NET installed. Whether you're on Linux, Windows, or macOS.

What does this all mean? It means you'll be able to have a more **performant**, **reliable**, and *modern* application.

You will *not* receive any support from the ShareX project for this software.
If you have any issues with this project, please **open an issue** in this repository.

However, it's important to note that this project is maintained by volunteers,
and we may not be able to provide support for all issues.
We will do our best to help you, but we cannot guarantee that we will be able to resolve your issue.

<p align="center"> For further information, please check the source code.</p>

## Supported Linux Distributions

This project is built on Ubuntu 24.04 and is tested on the following distributions:

- Ubuntu 24.04
- Fedora 41

If you're using a different distribution, there will be a Flatpak package available when possible. If you're using a distribution that doesn't support Flatpak, you can build the project from source.

## Other platforms

When I initially started this port, I only came with one main goal: ShareX on Modern Linux on native Wayland.
I realized my work could be used on other platforms such as macOS or Windows...

That's why SnapX.Avalonia was created.

Powered by [FluentAvalonia](https://github.com/amwx/FluentAvalonia), it *should* look something like this.
Screenshot from [FluentSearch](https://github.com/adirh3/Fluent-Search): ![screenshot of the FluentSearch application that looks like a modern native Windows application](.github/image.png)

For screenshots, it uses your operating system's respective APIs. On Linux Wayland, it uses portals. This is a less performant implementation as it has to delete the requested screenshot file after reading it into memory.

## Development Dependencies

Instructions for other projects within the SnapX solution are not provided yet.

> SnapX.GTK4 does not use developer header files and only requires the binary package.

- `git`
- `gtk4` on Fedora or `libgtk-4-1` on Ubuntu
- `dotnet-sdk-9.0`
- `ffmpeg` (7.0.0)
- `clang`
- `zlib-devel`

### Ubuntu 24.04

```bash
sudo apt update -q && sudo apt install -y software-properties-common
sudo add-apt-repository ppa:dotnet/backports # Ubuntu 24.04 doesn't have .NET 9 packaged.
sudo add-apt-repository ppa:ubuntuhandbook1/ffmpeg7 # Ubuntu 24.04 doesn't have FFMPEG 7 packaged.
sudo apt install -y git libgtk-4-1 dotnet-sdk-9.0 ffmpeg clang
```

### Fedora 41

```bash
sudo dnf install -y git gtk4 dotnet-sdk-9.0 /usr/bin/ffmpeg clang zlib-devel @c-development @development-libs
```

## Building from Source

Only do this if you're a developer, the solution *does* build,
but you should have a backup of all your ShareX/SnapX data.
I do, in fact, mean it when I say the project isn't ready for use.

```bash
git clone https://github.com/BrycensRanch/SnapX
cd SnapX
./build.sh # Calls NUKE (https://nuke.build)
Output/snapx-ui/snapx-ui # Run SnapX.Avalonia
Output/snapx-gtk/snapx-gtk # Run SnapX.GTK4
# There is nothing stopping you from using regular dotnet building tools
# dotnet publish -c Release
# SnapX.Avalonia/bin/Release/net9.0/linux-x64/publish/snapx-ui
```

## Contributions

Contributions are welcome. The documentation for contributing is a work in progress, but here is a [rough draft](./.github/CONTRIBUTING.md).

Lastly...

I use Fedora Rawhide btw :^)

[Fine, I'll do it myself.](https://www.youtube.com/watch?v=L_WoOkDAqbM)

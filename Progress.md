# Internal document outlining the progress and goals of the project

# Checklist

- [x] Port `NativeMessagingHost` to .NET 9 (It was literally a few lines of code...)
- [ ] Port `UploadersLib` to .NET 9 and remove UI code
- [ ] Port `ScreenCaptureLib` to .NET 9 and GTK and unknown dependencies (Requires investigation)
- [ ] Investigate `UploadersLib` (What is it? What does it do?)
- [ ] Investigate `IndexerLib` (What is it? What does it do?)
- [ ] Investigate `HistoryLib` (What is it? What does it do?)
- [ ] Convert history to SQLite instead of JSON. I know this is a big change, but it'd remove the typically unnecessary built-in backup feature. <https://pl-rants.net/posts/when-not-json> <https://docs.servicestack.net/ormlite/>
- [ ] Log files should be a daily log file, not a whole MONTH (wtf?)
- [ ] Symlink ~/Documents/ShareX-Linux to their appropriate XDG directories to keep the familiar structure users are used to without violating the [XDG spec](https://specifications.freedesktop.org/basedir-spec/latest/).
- [ ] Package for all major distributions besides NixOS. This means: Fedora, Ubuntu, Snap, AppImages, .run, Arch Linux, and Debian if they're lucky...
- [ ] Create bindings for `go-keyring` so we're not storing API Credentials in plain text on the filesystem. It's also cross-platform. That way, if need be, porting this to every major OS should be less cumbersome.
- [ ] Learn Rust and a hint of C++ to use XCap and other cross-platform screen capture libraries. (This will make the port take much longer)
- [ ] Rust interop bindings

## Why I choose GTK4

Although GTK4 is not a full replacement for Windows Forms, it is a step in the right direction. GTK4 is a modern toolkit that is actively developed and maintained. It is also the toolkit used by GNOME, which is the most popular desktop environment on Linux. By using GTK4, we can ensure that the application will be compatible with the latest versions of GNOME and other desktop environments that use GTK4.

Truth be told, I really wanted this to be QT6, but there are no C# bindings for QT6 yet. I'm not going to write a C++ application. The project would **never** be done.

To accomplish this goal, <https://github.com/gircore/gir.core> is called upon.

### SemVer & New Commit Message Standard

The version for this port has been set to 0.0.0 until the project is in a usable state. The version will be updated to 1.0.0 when the project feature is complete and ready for general use. The project begins with ShareX's version 16 code base.

For the commit messages, I will be following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) standard. This will allow for automatic versioning and changelog generation. This will also allow for easier tracking of changes and features.

### Implementation of auto update & controversial features

#### Auto Update

ShareX on Windows has auto update functionality. This is a feature that I would like to implement in this port. This will allow users to receive updates automatically without having to manually download and install them. This will also allow for easier distribution of updates and bug fixes. I know this doesn't bode well with Linux users, that's why I ask you which one of you ported ShareX to Linux? Oh, that's right, no one. I'm doing this for free. I'm doing it my way. :laughing:

The idea is for ShareX to check for updates on startup. Since the goal is to have the application with one singular binary with no DLLs/.so files to worry about. Like Electron apps. It'll replace the application binary to the latest version that is the same major version. This will allow for easy updates and bug fixes to be distributed to users. Downgrades will not be allowed.

#### Telemetry

I'm aiming to add telemetry to the application. This will allow for the collection of anonymous usage data. This data will be used to improve the application and fix bugs. Coming in the form of [Sentry](https://sentry.io/). This will allow for the collection of crash reports and other useful data. This will be opt-out and can be disabled in the settings. This will be ***enabled*** by default. Telemetry is best when it represents the majority of the user base. I kindly ask you to not disable it. It's for the greater good. I know companies continue to abuse telemetry for their own gain, but this is not the case here. This is for the betterment of the application and the user experience. I'm not selling your data to advertisers. I'm not selling your data to anyone.

#### Why are you doing this?

WINE is not a solution. Wine is a compatibility layer. It is not a replacement for native applications. I enjoyed using ShareX. Previous attempts to have always been to try and negate the fact that ultimately a Windows application. I hope to reuse ShareX's code with the introduction of .NET 9 and GTK4, but with this port, it should become a Linux application

#### How are screenshots going to work?

On Windows, System.Drawing depends on the GDI+ native library, shipped with Windows Home.

On .NET 9, System.Drawing is not available on Linux/macOS. However, there is hope...





<https://sixlabors.com/products/imagesharp/> This library is a cross-platform library that can be used to manipulate images. This library is used by the Avalonia project to handle images. This library will be used to handle images in this project.


#### Snap & Flatpak

A snap package should be created easily with examples like https://github.com/NickvisionApps/Parabolic

##### Finally...

````
Jaex — 03/04/2017 8:37 PM
i know countless people who want to make linux version too
but nobody willing to do it
or give up middle of it after see difficulty
so it is only on talk
````

"Talk is cheap, show me the code"—Linus Torvalds

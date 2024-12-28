#
# spec file for package snapx, snapx-gtk, and snapx-ui
#
# Copyright (c) 2024 Brycen Granville <brycengranville@outlook.com>
#
# All modifications and additions to the file contributed by third parties
# remain the property of their copyright owners, unless otherwise agreed
# upon. The license for this file, and modifications and additions to the
# file, is the same license as for the pristine package itself (unless the
# license for the pristine package is not an Open Source License, in which
# case the license is the MIT License). An "Open Source License" is a
# license that conforms to the Open Source Definition (Version 1.9)
# published by the Open Source Initiative.

# Please submit bugfixes or comments via https://github.com/BrycensRanch/SnapX/issues


# This spec requires internet access! This is only meant to be built on Fedora COPR at the moment!


%global version         0.0.0
%bcond check 0

# Set the dotnet runtime

%ifarch x86_64
%global runtime_arch x64
%endif
%ifarch aarch64
%global runtime_arch arm64
%endif
%ifarch arm
%global runtime_arch arm
%endif
%ifarch armhf
%global runtime_arch arm
%endif

# The dotnet version folder path
%global         net             net9.0
%global         dotnet_runtime  linux-%{runtime_arch}


Name:           snapx
Version:        %{version}
Release:        5%{?dist}
Summary:        Screenshot tool that handles images, text, and video.

License:        GPL-3.0-or-later
URL:            https://github.com/BrycensRanch/SnapX
Source:         %{url}/archive/refs/heads/develop.tar.gz

BuildRequires:  dotnet-sdk-9.0
Requires:       /usr/bin/ffmpeg
Requires:       libcurl, fontconfig, freetype, openssl, glibc, libicu, at, sudo


# .NET architecture support is rather lacking.
ExclusiveArch:  x86_64 aarch64

# .NET is not supported by either of these.
%define _debugsource_template %{nil}
%global         debug_package %{nil}



%description
This is a port of the original ShareX application to Linux.
It is not an official release and is not affiliated with the original ShareX project.
Specifically, it is the CLI tool.

%package gtk
Summary:        SnapX GTK4 UI
Requires:       gtk4
BuildRequires:  gtk4-devel
%description gtk
SnapX but gtk4

%package ui
Summary:        SnapX Avalonia-based UI


%description ui
SnapX but with Avalonia. Works best on X11.

%prep
%autosetup -n SnapX-develop


%build
# Setup the correct compilation flags for the environment
# Not all distributions do this automatically
%if 0%{?fedora}
    # Do nothing, since Fedora 33 the build flags are already set
%else
    %set_build_flags
%endif
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export AVALONIA_TELEMETRY_OPTOUT=1
export NUKE_TELEMETRY_OPTOUT=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH=$PATH:/usr/local/bin

dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none SnapX.CLI
dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none SnapX.NativeMessagingHost
dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none SnapX.GTK4
dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none SnapX.Avalonia

%check
SnapX.CLI/bin/Release/%{net}/%{dotnet_runtime}/publish/snapx --version


%install
%{__mkdir} -p %{buildroot}%{_libdir}/snapx %{buildroot}%{_bindir} %{buildroot}%{_datadir}/SnapX
%{__cp} SnapX.CLI/bin/Release/%{net}/%{dotnet_runtime}/publish/snapx %{buildroot}%{_bindir}
%{__cp} SnapX.GTK4/bin/Release/%{net}/%{dotnet_runtime}/publish/snapx-gtk %{buildroot}%{_bindir}
%{__cp} SnapX.NativeMessagingHost/bin/Release/%{net}/%{dotnet_runtime}/native/SnapX_NativeMessagingHost %{buildroot}%{_libdir}/snapx
%{__cp} SnapX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/snapx-ui %{buildroot}%{_bindir}
%{__cp} SnapX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/*.so %{buildroot}%{_libdir}
%{__cp} -r SnapX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/Resources %{buildroot}%{_datadir}/SnapX

%files
%{_bindir}/%{name}
%{_libdir}/%{name}
%{_libdir}/*.so
%{_datadir}/SnapX
%license LICENSE.md

%files gtk
%{_bindir}/%{name}-gtk
%license LICENSE.md

%files ui
%{_bindir}/%{name}-ui
%license LICENSE.md


%if 0%{?fedora}
%changelog
%autochangelog
%else


%changelog
* Thu Nov 18 2024 Brycen G <brycengranville@outlook.com> 0.0.0-1
- Initial package
%endif

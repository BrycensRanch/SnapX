#
# spec file for package sharex & sharex-gtk
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

# Please submit bugfixes or comments via https://github.com/BrycensRanch/ShareX-Linux-Port/issues


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

# The dotnet version folder path
%global         net             net9.0
%global         dotnet_runtime  linux-%{runtime_arch}


Name:           sharex
Version:        %{version}
Release:        3%{?dist}
Summary:        Screenshot tool that handles images, text, and video.

License:        GPL-3.0-or-later
URL:            https://github.com/BrycensRanch/ShareX-Linux-Port
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
Summary:        ShareX GTK4 UI
Requires:       gtk4
BuildRequires:  gtk4-devel
%description gtk
ShareX but gtk4

%package ui
Summary:        ShareX Avalonia-based UI


%description ui
ShareX but with Avalonia. Works best on X11.

%prep
%autosetup -n ShareX-Linux-Port-develop


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
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH=$PATH:/usr/local/bin

dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none ShareX.CLI
dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none ShareX.GTK4
dotnet publish --configuration Release --runtime %{dotnet_runtime} \
    -p:DebugSymbols=false -p:DebugType=none ShareX.Avalonia

%check
ShareX.CLI/bin/Release/%{net}/%{dotnet_runtime}/publish/sharex --version


%install
%{__mkdir} -p %{buildroot}%{_libdir}/sharex %{buildroot}%{_bindir} %{buildroot}%{_datadir}/ShareX
%{__cp} ShareX.CLI/bin/Release/%{net}/%{dotnet_runtime}/publish/sharex %{buildroot}%{_bindir}
%{__cp} ShareX.GTK4/bin/Release/%{net}/%{dotnet_runtime}/publish/sharex-gtk %{buildroot}%{_bindir}
%{__cp} ShareX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/ShareX.Avalonia %{buildroot}%{_libdir}/sharex
%{__cp} ShareX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/*.so %{buildroot}%{_libdir}/sharex
%{__cp} -r ShareX.Avalonia/bin/Release/%{net}/%{dotnet_runtime}/publish/Resources %{buildroot}%{_datadir}/ShareX

# Create the wrapper shell script
cat > %{buildroot}%{_bindir}/sharex-ui <<EOF
#!/bin/sh
# Wrapper script to invoke the actual binary
exec %{_libdir}/%{name}/ShareX.Avalonia "\$@"
EOF
chmod +x %{buildroot}%{_bindir}/sharex-ui

%files
%{_bindir}/%{name}
%license LICENSE.md

%files gtk
%{_bindir}/%{name}-gtk
%license LICENSE.md

%files ui
%{_bindir}/%{name}-ui
%{_datadir}/ShareX
%{_libdir}/%{name}
%license LICENSE.md


%if 0%{?fedora}
%changelog
%autochangelog
%else


%changelog
* Thu Nov 18 2024 Brycen G <brycengranville@outlook.com> 0.0.0-1
- Initial package
%endif

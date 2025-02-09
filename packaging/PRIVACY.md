# SnapX's Privacy Policy

> Authored on 2/8/2025
> Last updated on 2/8/2025 7:28 PM EST

By default, SnapX collects telemetry data about how the application is performing in two ways:

- Application usage data (Tells us how many people are using my application on what systems)
- What Operating System you're running and which version you're using. ie `Windows 10 Home` or `macOS Sonana` or `Fedora Linux 42 (KDE Plasma) x86_64 Linux 6.10.9`
- Application version ie `1.0.0+44e1612` and the assembly you're using `snapx-ui`
- What CPU your computer has ie `i7-11800H`
- What GPU your computer has ie `RTX 3060 Laptop GPU`
- How much memory the application is using (Helps to identify memory leaks)
- How much overall memory your computer has to better understand the "headroom space" left.
- Application environment ie running it on Linux with AppImage, Flatpak, Snap. Or running the portable version of my application on Windows.
- Application crash data with stack traces that have had any PII removed & anonymized
- General Region ie `United States`

All this data helps to improve SnapX as it is [Free software](https://www.gnu.org/licenses/gpl-3.0.en.html).

### Definitions

telemetry - Modern, dynamic distributed systems require comprehensive monitoring to understand software behavior in various situations. Developers face challenges tracking the software’s performance in the field and responding to various modifications. To keep up with continuously changing requirements, it’s essential to have a simple way to collect data from systems the application is running.

SSDP - On devices and PCs that support SSDP, this feature can be enabled, disabled, or paused. When SSDP is enabled, devices communicate information about themselves and the services they provide to any other UPnP client. Using SSDP, computers connected to the network also provide information about available services.

anonymous - not identified by name; of unknown name.

## What I will not do

- Sell your data
- Validate the GDPR, CCPA, whatever.
- Spy on what you're doing
- Collect non-anonymous data such as your name, computer's name (Brycen's GamingLaptop), etc.
- Be evil

For application analytics, we use Aptabase.com and their privacy policy can be found [here](https://aptabase.io/legal/privacy).

## Services Used

All the services used for telemetry are open source.

- [Sentry](https://github.com/getsentry/sentry) - Application crash information & traces & performance analytics ie (specific code function taking a long time)
- [Aptabase](https://github.com/aptabase/aptabase) - Usage analytics ie (how many users are using a specific function such as uploading)

## In terms of fingerprinting

- Although your *public* IP address is naturally processed by these network services. It is not saved and thus discarded.

## How do I disable telemetry?

Go into the application's settings menu or config file (~/.config/SnapX/ApplicationConfig.json on Linux)

Set `DisableTelemetry` to `true` and then no data should be sent for telemetry or application analytics.

## How do I request for my data to be removed?

There is no personal data. All data collected is anonymous. So I can't exactly fulfill requests to remove your specific data because the data that is collected is what I'd call... gray. There are no distinct identifiers.

## Final notes

I made SnapX because I wanted to learn more about coding. I decided to make it free and open source for you, and this is really all I expect to learn. I doubt I'll even get any donations for my work. This is all I ask, **keep telemetry on**. Help me ***improve*** SnapX.

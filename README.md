<img src="media/RatLogo.png" height=100 align=right>

<div>
<a href="https://patreon.com/RatScanner">
 <img src="https://img.shields.io/badge/dynamic/json?color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F4117180&style=for-the-badge&logo=patreon" />
</a>

<a href="https://discord.gg/aHZf7aP" style="padding:10px">
 <img src="https://img.shields.io/discord/687549250435153930?label=Discord&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2&style=for-the-badge" />
</a>

<a href="https://github.com/RatScanner/RatScanner/releases/latest/download/RatScanner.zip">
<img src="https://img.shields.io/static/v1?&label=&message=Download&color=4FBD54&style=for-the-badge&logo=data:image/svg+xml;base64,PHN2ZyB2ZXJzaW9uPSIxLjEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHZpZXdCb3g9IjAsMCwxMDI0LDEwMjQiPgoJPGRlc2M+ZmlsZV9kb3dubG9hZCBpY29uIC0gTGljZW5zZWQgdW5kZXIgQXBhY2hlIExpY2Vuc2UgdjIuMCAoaHR0cDovL3d3dy5hcGFjaGUub3JnL2xpY2Vuc2VzL0xJQ0VOU0UtMi4wKSAtIENyZWF0ZWQgd2l0aCBJY29uZnUuY29tIC0gRGVyaXZhdGl2ZSB3b3JrIG9mIE1hdGVyaWFsIGljb25zIChDb3B5cmlnaHQgR29vZ2xlIEluYy4pPC9kZXNjPgoJPGcgZmlsbD0iI2ZmZmZmZiIgZmlsbC1ydWxlPSJub256ZXJvIiBzdHlsZT0ibWl4LWJsZW5kLW1vZGU6IG5vcm1hbCI+CgkJPHBhdGggZD0iTTUxMiw2ODIuNjdsLTI5OC42NywtMjk4LjY3aDE3MC42N3YtMjU2aDI1NnYyNTZoMTcwLjY3ek04MTAuNjcsNzY4djg1LjMzaC01OTcuMzR2LTg1LjMzeiIvPgoJPC9nPgo8L3N2Zz4=" />
</a>
</div>

# Rat Scanner

Rat Scanner is a open source tool for [Escape from Tarkov][escape-from-tarkov].

Please consider [supporting](#Support-the-project) the project to help finance the backend server as well as the [API][tarkov-dev].

[Tutorial / Demo Video][demo-video] - [Frequently asked Questions][faq-page]

<br/>

## Can I get banned for using Rat Scanner?

While Battlestate Games does not support nor is affiliated with this project, it exists since two years with over 1.000 players using it every day in their games. So far there has not been a single instance in which RatScanner was proven to have caused any ban.

<br/>

## What it does

Rat Scanner allows you to scan items in the game and provides you with data about items (average price, value per slot, ...).

The information is taken from a [third-party API][tarkov-dev] which takes the data directly from the game.

<br/>

## How it works

The tool is entirely external. This means it is not accessing any memory of the game, like cheats do.

Instead, when you want to scan a item, a screenshot is taken and image processing is applied to identify the clicked item. The item is then looked up in the database and information is displayed in the window and with a overlayed tooltip.

<br/>

## How to use

Your game may need to be in either `Borderless` or `Windowed` mode for the overlay to work.

There are currently two types of item scan methods

### Name scanning

_Name scanning refers to scanning the inspection name of a item._

- Simply left click onto the magnifier icon inside the inspect window

Limitations

- Uses / durability is always assumed at 100%
- Weapons and other modable items will only show info of the base item

<img src="media/NameScan.gif" width=400px>

### Icon scanning

_Icon scanning refers to scanning the icon of a item._

- Hold the modifier key down while left clicking on a item
- The modifier key can be changed in the settings (default is `Shift`)

Limitations

- It is unfortunately no longer possible to scan weapons
- Uses / durability is always assumed at 100%
- Items which share a icon with other items (especially keys) will result in a uncertain match
- There will be missmatches when scanning icons in the top left of the item stash since the bright light (top center of the screen) interferes with it

<img src="media/IconScan.gif" width=400px>

<br/>

## Minimal UI

Switch to the minimal ui by clicking the dedicated button inside the titlebar.
Get back to the standard view by **double clicking** anywhere inside the window.

Background opacity as well as the data which is shown can be configured in the settings.

<img src="media/MinimalUI-HowTo.gif" width=280px>

## Download

You can directly download the latest version [here][latest-release] or choose a specific version to download [here][releases].

After you downloaded the Zip-Archive (you only need `RatScanner.zip`) extract it anywhere on your PC and run `RatScanner.exe`.

Once it has launched, go into the settings menu (bottom right corner) and check that your resolution is set properly (default is FullHD).

If you have any problems with the process please checkout the [FAQ][faq-page] or join the [Discord][discord] if you need further help.

**Important:** If the tool does not seem to start, here's some [common issues][common-issues]

<br/>

## Setting up the repository for development

1. Clone the repository
2. Copy the `Data` folder from the latest release to `RatScanner\Data\`

### Compiling

- Open the solution inside Visual Studio and click Build -> Build Solution

### Publishing

- Run the `publish.bat` script which is inside the repository root.
- The output will be located in a folder called `publish` on the same level as the publish script.

<br/>

## Contributing

Please read `CONTRIBUTING.md` before contributing.

<br/>

## Support the project

This will help to finance the backend server as well as the [API][tarkov-dev] which provides the backend with data.

[![Patreon](https://img.shields.io/badge/dynamic/json?color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F4117180&style=for-the-badge&logo=patreon)](https://patreon.com/RatScanner)
[![PayPal](https://img.shields.io/static/v1?&label=PayPal&message=Donate&color=0079C1&style=for-the-badge&logo=paypal)](https://paypal.me/mscheve)

[escape-from-tarkov]: https://www.escapefromtarkov.com/
[tarkov-dev]: https://tarkov.dev/
[eft-icons-repo]: https://github.com/RatScanner/EfTIcons
[latest-release]: https://github.com/RatScanner/RatScanner/releases/latest/download/RatScanner.zip
[releases]: https://github.com/RatScanner/RatScanner/releases/
[demo-video]: https://www.youtube.com/watch?v=tXoIkgXFmdA
[faq-page]: FAQ.md
[faq-page-can-i-get-banned]: FAQ.md#can-i-get-banned-for-using-rat-scanner
[discord]: https://discord.com/invite/aHZf7aP
[common-issues]: https://github.com/RatScanner/RatScanner/blob/master/FAQ.md#program-issues-1

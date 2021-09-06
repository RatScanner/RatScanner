<img src="media/RatLogo.png" height=100 align=right>

<div>
  <a href="https://patreon.com/RatScanner">
    <img src="https://img.shields.io/badge/dynamic/json?color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F4117180&style=for-the-badge&logo=patreon" />
  </a>

  <a href="https://discord.gg/aHZf7aP" style="padding:10px">
    <img src="https://img.shields.io/discord/687549250435153930?label=Discord&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2&style=for-the-badge" />
  </a>
</div>

# Rat Scanner FAQ

## Table of Contents

### General
- [Can I get banned for using Rat Scanner?](#can-i-get-banned-for-using-rat-scanner)

### Program issues
- [There is no RatScanner.exe file](#there-is-no-ratscannerexe-file)
- [Rat Scanner is not starting](#rat-scanner-is-not-starting)
- [Nothing happens when scanning](#nothing-happens-when-scanning)
- [Could not find icon cache folder at: ...](#could-not-find-icon-cache-folder-at-)
- [Could not find dynamic correlation data at: ...](#could-not-find-dynamic-correlation-data-at-)
- [The type initializer for 'OpenCvSharp.NativeMethods' threw an exception.](#the-type-initializer-for-opencvsharpnativemethods-threw-an-exception)

### Scanning issues
- [Icon scanning gets a lot of wrong matches](#icon-scanning-gets-a-lot-of-wrong-matches)

<br/>

---

<br/>

# General

## Can I get banned for using Rat Scanner?
After over a year of trying to get in contact with BSG, it finally worked out.
So here it is, the long-awaited response, regarding the use of Rat Scanner:

> "[They (BSG)] checked it [Rat Scanner] and found nothing suspicious. But stay fair and don't give them [BSG/BE] a reason" - Battlestate Games head of PR

I know it's a rather brief response, when considering how long it took. Nevertheless, I hope lots of you who enjoy Rat Scanner can now continue to do so without the feeling of risking a ban.

<br/>

---

<br/>

# Program issues

## There is no RatScanner.exe file
Make sure you downloaded and extracted the files as described inside the [download section][download-section]

If you still cannot see `RatScanner.exe` it is most likely removed by your antivirus.
In that case, create a exception for it or disable your antivirus.

## Rat Scanner is not starting
Make sure you have the **x64 Version** of the [.Net Core Runtime][net-core-download] installed.

## Nothing happens when scanning
- Check that you set your resolution correctly inside the settings
- Try to run RatScanner as administrator

## Could not find icon cache folder at: ...
Please have a look at [the question below](#could-not-find-dynamic-correlation-data-at-).

## Could not find dynamic correlation data at: ...
1. Close RatScanner
2. Start Escape From Tarkov
3. Go to Mechanics trading screen and wait for all icons to load (no spinning circles)
4. Start RatScanner.exe

## The type initializer for 'OpenCvSharp.NativeMethods' threw an exception.
This probably means you are missing the Windows Media Features.

There are two ways to install the Media Feature Pack:
- Navigate to **Settings** > **Apps** > **Apps and features** > **Optional features** > **Add a feature**, and then locate **Media Feature Pack** in the list of available optional features.
- Download the pack installer for your Windows version from [here][windows-media-pack] and run it.

After the installation has finished, restart your computer to make sure the changes are applied.

<br/>

---

<br/>

# Scanning issues

## Icon scanning gets a lot of wrong matches
Icon scanning still has some known issues, some which are not possible to fix.
This currently leads to items like keys and small attachments matching wrong due to their simularity to other items.
Also, when in the stash, the light bright in the top center of the screen interferes with the top left section of the stash which results in extremly bad results.

[net-core-download]: https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-3.1.14-windows-x64-installer
[download-section]: https://github.com/Blightbuster/RatScanner#download
[windows-media-pack]: https://www.microsoft.com/en-us/software-download/mediafeaturepack

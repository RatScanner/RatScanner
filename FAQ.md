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

### Program issues
- [There is no RatScanner.exe file](#there-is-no-ratscannerexe-file)
- [Rat Scanner is not starting](#rat-scanner-is-not-starting)
- [Nothing happends when scanning](#nothing-happends-when-scanning)
- [Could not find icon cache folder at: ...](#could-not-find-icon-cache-folder-at-)
- [Could not find dynamic correlation data at: ...](#could-not-find-dynamic-correlation-data-at-)

### Scanning issues
- [Icon scanning gets a lot of wrong matches](#icon-scanning-gets-a-lot-of-wrong-matches)

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

## Nothing happends when scanning
- Check that you set your resolution correctly inside the settings
- Try to run RatScanner as administrator

## Could not find icon cache folder at: ...
Please have a look at [the below question](#could-not-find-dynamic-correlation-data-at-).

## Could not find dynamic correlation data at: ...
1. Close RatScanner
2. Start Escape From Tarkov
3. Go to Mechanics trading screen and wait for all icons to load (no spinning circles)
4. Start RatScanner.exe

<br/>

---

<br/>

# Scanning issues

## Icon scanning gets a lot of wrong matches
Icon scanning still has some known issues, some which are not possible to fix.
This currently leads to items like keys and small attachments matching wrong due to their simularity to other items.
Also, when in the stash, the light bright in the top center of the screen interferes with the top left section of the stash which results in extremly bad results.

[net-core-download]: https://dotnet.microsoft.com/download/dotnet-core/current/runtime
[download-section]: https://github.com/Blightbuster/RatScanner#download

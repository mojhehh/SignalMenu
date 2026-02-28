# Signal Menu

ii's Stupid Menu is one of the best mod menus out there but it gets detected pretty easily. Signal Menu fixes that.

## What is this

This is a fork of [ii's Stupid Menu](https://github.com/iiDk-the-actual/iis.Stupid.Menu) that takes the original menu and wraps it in a full safety system so you actually stay undetected. The base menu is great — tons of mods, good UI, solid foundation — but out of the box it leaks telemetry, doesn't spoof anything, and has no protection against bans or reports. We fixed all of that.

## What we changed

- Bans get caught before they kick in. If you get flagged mid-game, the menu locks the room down so you don't lose your session
- Players reaching for the report button get detected before they press it. Hand tracking with ping compensation so it works even on laggy lobbies  
- All the telemetry the game sends to PlayFab and the other backend stuff gets blocked. No more sending your hardware info, play data, or anything else
- Your device ID, hardware ID, and ad ID all get spoofed so even if something does get through it doesn't point back to you
- Moderators and content creators get flagged the second they join your room
- If you have another menu loaded it detects the conflict and overrides their patches so nothing breaks
- Voice alerts tell you whats happening — ban detected, report nearby, mod joined, etc. You don't have to stare at the screen
- Popup notifications for everything too

The original ii's menu had basic versions of some of this stuff but most of it was broken or incomplete. The autoban bypass patches literally did nothing (returned true instead of blocking), the advertiser ID wasn't actually being spoofed even when the setting was on, and one of the telemetry patches was sending the wrong value which could break game features. All fixed now.

## License

GPL-3.0. Keep it open source, credit the original authors, don't hide or lock the code. Full license in the [LICENSE](LICENSE) file.

All original ii's Stupid Menu code is by iiDk and Goldentrophy Software. Gorilla Tag belongs to Another Axiom. We're not affiliated with any of them.

## Building

You need the .NET SDK. Game references go in `References/`.

```
dotnet build --configuration Release
```

# Signal Menu

A comprehensive safety and mod menu fork of ii's Stupid Menu for Gorilla Tag, with integrated Signal Safety protection systems.

## About

Signal Menu is a fork of [ii's Stupid Menu](https://github.com/iiDk-the-actual/iis.Stupid.Menu) by iiDk, enhanced with the full [Signal Safety](https://github.com/mojhehh/SignalSafetyMenu) protection suite. Everything that made ii's great, plus enterprise-grade anti-cheat countermeasures.

### What's Added

- **Anti-Ban System** — Detects bans in real-time and automatically secures your room
- **Anti-Report** — Predictive hand tracking with ping compensation detects when players approach report buttons
- **Telemetry Blocking** — 15+ patches blocking PlayFab, Mothership, and game telemetry
- **Identity Spoofing** — Device ID, hardware ID, and advertiser ID rotation
- **Moderator Detection** — Real-time alerts when moderators or content creators join
- **Menu Detection** — Detects conflicting mods and auto-overrides their patches
- **RPC Protection** — Blocks malicious network events and rate limit tracking
- **Anti-Predictions** — Prevents server-side movement prediction exploits
- **Audio Notifications** — TTS voice alerts for all safety events
- **Visual Notifications** — In-game popup system for all events

### Using the Code

GPL-3.0 applies. If you use it:

- Keep your project open-source
- Credit the original authors
- Don't lock or hide code
- Follow the [GPL license](https://www.gnu.org/licenses/gpl-3.0.html)

Signal Menu is GPL-licensed. All original ii's Stupid Menu code remains credited to iiDk and Goldentrophy Software. Gorilla Tag is property of Another Axiom LLC. Not officially affiliated or endorsed.

## Building

Requires .NET SDK with `netstandard2.1` target. Game references must be in `References/`.

```
dotnet build --configuration Release
```

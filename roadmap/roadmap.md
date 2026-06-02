# Playlist Roadmap

This file tracks planned work for upcoming releases.

How to read this file:
- Now: actively planned or in-progress for the next release cycle
- Next: approved for upcoming releases after current work
- Later: good ideas that are not yet scheduled
- Icebox: parked ideas that may return later

Priority labels:
- P0: urgent, high user impact
- P1: important quality or UX improvements
- P2: valuable enhancement, lower urgency

Status labels:
- Planned
- In Progress
- Blocked
- Done

Item detail docs:
- Detailed documentation for roadmap items lives in `roadmap/items/`.

## Now
- [ ] P1 Per-playlist playback behavior modes
  - Status: Planned
  - Target: v1.3.0
  - Why it matters: Gives users control over how playback continues after each video and supports common queue behavior expectations.
  - Notes: Add per-playlist modes for auto-next, loop, and shuffle variants.
  - Details: roadmap/items/p1-per-playlist-playback-behavior-modes.md

- [ ] P1 Media player usability polish
  - Status: Planned
  - Target: v1.2.11
  - Why it matters: Makes common playback actions faster and clearer for daily use.
  - Notes: Improve control feedback and edge-case behavior around pause/seek/stop.
  - Details: roadmap/items/p1-media-player-usability-polish.md

- [ ] P1 Update experience refinement
  - Status: Planned
  - Target: v1.2.11
  - Why it matters: Helps users discover and apply updates with less friction.
  - Notes: Review inline notice states and install flow messaging.
  - Details: roadmap/items/p1-update-experience-refinement.md

## Next
- [ ] P1 Playlist management enhancements
  - Status: Planned
  - Target: v1.3.0
  - Why it matters: Improves organization for larger libraries.
  - Notes: Expand bulk actions and editing quality-of-life improvements.

- [ ] P1 Search and filtering improvements
  - Status: Planned
  - Target: v1.3.0
  - Why it matters: Helps users find items faster in large playlists.
  - Notes: Add richer filter behavior and sorting shortcuts.

- [ ] P2 History and insights improvements
  - Status: Planned
  - Target: v1.3.0
  - Why it matters: Gives users clearer visibility into playback activity.
  - Notes: Improve history browsing and retention options.

## Later
- [ ] P2 Accessibility pass
  - Status: Planned
  - Target: TBD
  - Why it matters: Improves usability for keyboard and assistive technology users.

- [ ] P2 Import and migration tools
  - Status: Planned
  - Target: TBD
  - Why it matters: Makes onboarding easier when moving from other tools.

- [ ] P2 UI theme customization
  - Status: Planned
  - Target: TBD
  - Why it matters: Lets users personalize the app experience.

## Icebox
- [ ] Cloud sync exploration
  - Status: Planned
  - Target: TBD
  - Why it matters: Could improve multi-device continuity.

- [ ] Plugin or extension model exploration
  - Status: Planned
  - Target: TBD
  - Why it matters: Could enable community-driven enhancements.

## Recently Completed
- [x] P0 Playback stability hardening (v1.2.11) - roadmap/items/p0-playback-stability-hardening.md
- [x] Context-aware playlist right-click menu (item actions vs empty-space new playlist)
- [x] Playback and settings DbContext concurrency reliability improvements
- [x] Better playback error logging for troubleshooting

## Planning Rules
- Keep items user-focused and benefit-driven.
- Keep each item tied to a target version or explicit TBD.
- Move completed items into Recently Completed at each release.
- If priorities conflict, sort by user impact first, then implementation risk.

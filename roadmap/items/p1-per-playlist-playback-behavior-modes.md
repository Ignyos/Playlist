# P1 Per-playlist Playback Behavior Modes

## Summary
Allow each playlist to define how playback should proceed when a video completes.

## Scope
- Add per-playlist playback mode configuration.
- Keep current behavior available as a selectable mode.
- Expose configuration in Playlist Details panel for existing playlists.
- Add left-panel playlist context-menu entry to open Playlist Details.
- Support automatic next-item playback.
- Support optional list looping when reaching the final item.
- Support shuffle with repeat-allowed behavior.
- Support shuffle with play-once behavior (stop after all items are played).

## Out of Scope
- Adding playback-mode controls to new playlist creation flow.
- Adding playback-mode controls directly in the media player view.
- Cross-playlist global playback profiles.
- Weighted shuffle or user-defined probability rules.

## UX Decisions
- New playlists default to current behavior (Stop after current item).
- Playback mode is configured from Playlist Details.
- Left-panel playlist context menu includes an entry to open Playlist Details.
- No in-player mode configuration is provided in this iteration.
- Changes apply immediately after mode change (auto-save behavior).
- Shuffle play-once tracking is session-based and resets when the user restarts playlist playback.

## Proposed Modes
- Stop after current item: current default behavior.
- Sequential auto-next: automatically play the next list item.
- Sequential auto-next with loop: continue to first item after last item.
- Shuffle continuous: select random next item continuously; repeats allowed.
- Shuffle play-once: random order without repeats until all items have played, then stop.

## Acceptance Criteria
- A playlist setting controls end-of-item behavior.
- Mode changes apply immediately when changed from Playlist Details, including during an active playback session.
- Sequential auto-next moves to adjacent next item automatically.
- Loop mode restarts at first item after last item completes.
- Shuffle continuous can repeat previously played items.
- Shuffle play-once does not repeat items in a cycle and stops after the cycle is complete.
- Shuffle play-once cycle resets when the user restarts playlist playback.
- Playlist Details changes are auto-saved without requiring an explicit Save action.
- Existing playlists retain current behavior by default after upgrade.

## Test Plan
- Validate each mode with playlists of size 1, 2, and many items.
- Validate next/previous manual controls still work in all modes.
- Validate behavior when manually seeking near end and allowing completion.
- Validate immediate mode application during an active playback session.
- Validate persistence by closing and reopening the app.
- Validate auto-save after each mode change in Playlist Details.
- Validate context-menu entry opens Playlist Details for the selected playlist.
- Validate shuffle play-once reset on explicit playlist restart.
- Validate upgrade migration defaults for existing playlists.

## Focused Manual QA Checklist
- Context menu to Playlist Details flow:
	- Right-click a playlist in the left panel and verify Playlist Details opens for the selected playlist.
	- Verify right-clicking empty space still shows only empty-space actions.
- Auto-save behavior in Playlist Details:
	- Change mode selection and verify save status feedback updates without clicking Save.
	- Close and reopen Playlist Details and verify the selected mode persists.
- Immediate mode switching during active playback:
	- Start playback, change mode in Playlist Details while media is playing, and verify the new mode is used on completion of the current item.
- Shuffle play-once reset behavior:
	- In shuffle play-once, verify each item plays at most once in a cycle.
	- Verify playback stops after all items in the cycle are played.
	- Explicitly restart playback and verify a new shuffle play-once cycle starts.
- Loop and sequential edge cases:
	- With one-item playlist: verify sequential auto-next does not advance and loop mode replays the same item.
	- With multi-item playlist: verify sequential mode stops at the end and loop mode returns to the first item.

## Risks and Dependencies
- Requires playlist-level data model and migration updates.
- Requires clear UI placement for mode selection and explanation.
- Requires deterministic handling for edge cases such as deleted items and unavailable media.

## Notes
- Roadmap target: v1.3.0.
- Suggested first implementation order: data model, playback engine logic, UI controls, migration and testing.

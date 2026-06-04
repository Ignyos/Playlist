# P1 Corrupted Media Handling and Persistent Skip

## Summary
Detect playback failures for individual media files, flag them with a visible warning state, and continue playlist playback according to the active mode without stopping the session.

## Scope
- Detect item-level playback failures that indicate unreadable or corrupted files.
- Persist a flag on problematic playlist items.
- Display warning state for flagged items in playlist UI.
- Automatically skip flagged items during playback continuation.
- Continue playback based on currently selected playlist mode (sequential, loop, shuffle continuous, shuffle play-once).
- Preserve skip behavior across playlist restarts and app restarts until the flag is cleared.

## Out of Scope
- Automatic file repair or transcoding.
- External file-health scanning outside playback attempts.
- Cross-playlist/global flags for shared paths in this iteration.

## UX Decisions
- Flagged items are always skipped by auto-advance logic in all modes.
- Flagged items remain skipped across repeated starts/restarts of the same playlist.
- Users can still see flagged items in the list with clear warning affordance.
- A future clear/unflag action should allow users to retry files after remediation.

## Proposed Behavior Rules
- On playback error for an item, mark that item as flagged.
- In sequential and loop modes, move to the next non-flagged item.
- In shuffle continuous mode, randomly select from non-flagged items.
- In shuffle play-once mode, treat flagged items as effectively exhausted for cycle completion to avoid retry loops.
- If all items are flagged or exhausted, stop playback and surface a user-facing summary message.

## Acceptance Criteria
- Playback errors on individual files do not crash the app or terminate the entire playlist unexpectedly.
- Failed items become flagged and visibly marked in UI.
- Auto-advance skips flagged items in all playback modes.
- Skip behavior persists across playlist and app restarts.
- Shuffle play-once does not repeatedly re-attempt flagged items within or across cycles.
- Users receive clear feedback when playback ends because no playable items remain.

## Test Plan
- Trigger playback failures on one or more items and verify flags are persisted.
- Validate sequential, loop, shuffle continuous, and shuffle play-once behavior with mixed healthy/flagged items.
- Restart playback sessions multiple times and verify flagged items continue to be skipped.
- Restart app and verify persisted skip behavior remains active.
- Validate all-flagged playlist behavior and user messaging.

## Risks and Dependencies
- Requires item-level metadata persistence changes and migration.
- Requires careful continuation logic to avoid infinite loops when most items are flagged.
- Requires clear UI signaling so skipped behavior is predictable to users.

## Notes
- Recommended policy: flagged items are treated as already played for shuffle play-once cycle completion.
- Target: v1.3.1.

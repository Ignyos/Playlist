# P1 Playlist Management Enhancements

## Summary
Let users organize playlists into two persistent sections: Queue and Completed.

## Scope
- Add a persistent playlist state with two values: Queue and Completed.
- Add a persistent queue order for playlists in the Queue section.
- Render Queue items at the top of the list.
- Render Completed items in a lower section separated by a visual divider.
- Sort Completed items alphabetically.
- Default new queue items to the bottom of the Queue.
- Add context menu actions to move queue items up or down.
- Add context menu actions to mark a playlist complete or return it to the queue.

## Out of Scope
- Cross-playlist auto-play or playlist chaining.
- More than two playlist states.
- Drag-and-drop reordering.
- Bulk playlist reclassification in this iteration.

## UX Decisions
- Queue items are user-ordered and persisted with an explicit order value.
- Completed items are persisted separately from queue order and always displayed alphabetically.
- Marking a playlist complete clears its queue order so it can be cleanly re-added later.
- Re-adding a completed playlist to the queue places it at the bottom and recalculates queue ordering as needed.
- Move Up and Move Down actions appear only for Queue items.

## Acceptance Criteria
- A playlist can be marked as Queue or Completed.
- Queue playlists appear above Completed playlists.
- A visible separator or section header distinguishes Queue from Completed.
- Queue order persists across app restarts.
- Completed sorting persists across app restarts.
- Adding a playlist to Queue places it at the bottom by default.
- Move Up and Move Down change only the order within Queue.
- Completed playlists are shown alphabetically in the Completed section.
- Marking a playlist Completed moves it out of Queue and clears its queue order.

## Test Plan
- Create several playlists and verify Queue ordering is persisted after restart.
- Add playlists to Queue and verify each new item lands at the bottom.
- Mark a Queue item Completed and verify it moves to the Completed section.
- Verify Completed items are alphabetically sorted after changes and restart.
- Verify Move Up and Move Down only affect Queue items.
- Verify re-adding a Completed item to Queue assigns a new bottom position.

## Risks and Dependencies
- Requires a data model change to store state separately from queue order.
- Requires list rendering updates for sectioning and sorting.
- Requires careful handling when items move between sections so ordering stays consistent.

## Notes
- Roadmap target: v1.3.0.
- Suggested implementation order: data model and migration, ordering logic, context menu actions, list rendering.
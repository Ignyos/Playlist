# Copilot Instructions for Playlist

Use this file for repository-specific response behavior.

## Source of truth for upcoming work
- The roadmap summary is defined in roadmap/roadmap.md.
- Detailed item docs live in roadmap/items/.
- When asked questions such as "What is next?" or "What is next on our list?", use roadmap/roadmap.md as the primary source.
- Summarize in this order: Now, Next, Later.
- If roadmap/roadmap.md and another document disagree, prioritize roadmap/roadmap.md and note the mismatch.

## How to answer roadmap questions
- Prefer concise, user-facing summaries.
- Include priority and target version when present.
- Highlight blockers or dependencies if marked.
- If asked for details about one item, consult roadmap/items/ for that item when available.
- If no items exist in Now, return the top 1-3 items from Next.

## Planning hygiene
- Keep roadmap items outcome-focused, not implementation-only.
- Preserve section structure in roadmap/roadmap.md (Now, Next, Later, Icebox, Recently Completed).
- Do not invent roadmap items unless explicitly asked to propose ideas.

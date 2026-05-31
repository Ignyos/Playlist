# Copilot Instructions for Playlist

Use this file for repository-specific response behavior.

## Source of truth for upcoming work
- The roadmap is defined in roadmap.md at the repository root.
- When asked questions such as "What is next?" or "What is next on our list?", use roadmap.md as the primary source.
- Summarize in this order: Now, Next, Later.
- If roadmap.md and another document disagree, prioritize roadmap.md and note the mismatch.

## How to answer roadmap questions
- Prefer concise, user-facing summaries.
- Include priority and target version when present.
- Highlight blockers or dependencies if marked.
- If no items exist in Now, return the top 1-3 items from Next.

## Planning hygiene
- Keep roadmap items outcome-focused, not implementation-only.
- Preserve section structure in roadmap.md (Now, Next, Later, Icebox, Recently Completed).
- Do not invent roadmap items unless explicitly asked to propose ideas.

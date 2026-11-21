# CLAUDE.md
@AGENTS.md
- Follow [AGENTS.md](AGENTS.md) as the single source of truth for architecture, coding, localization (`it`, `de`, `fr`, `en`), quality gates, and `dev`-only git workflow.
- Claude override: if executing in a linked worktree, commit and test there, then merge into `dev` from the primary root and remove the worktree + feature branch after pushing `origin dev`.

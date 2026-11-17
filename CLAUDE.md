# CLAUDE.md

@AGENTS.md

## Claude Code Execution Directives
- **Worktree & Branch Discipline:** Always execute changes in dedicated feature/fix branches or git worktrees. Keep git status clean.
- **Strict Quality Gates:** Before submitting or declaring work finished, always run:
  ```powershell
  dotnet build
  dotnet test --no-build
  ```
- **Adhere to ZERO DDD:** Strictly enforce POCO entities and CQRS handlers. Reject AggregateRoots, domain event dispatchers, or outbox patterns.
- **Analyzers & Warnings:** Treat warnings as errors. Fix all analyzer diagnostics (`Roslynator`, `SonarAnalyzer`, `StyleCop`, `Meziantou`) immediately.

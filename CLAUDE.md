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

## Git Workflow & Direct Merge Policy (STRICT)
- **Target Branch (`dev`):** Integrate all work directly into `dev` via local merge. **NEVER commit, merge, or push directly to `main`**, and **NEVER create Pull Requests (`gh pr create` / `gh pr`).**
- **Worktree Handling:** If running inside a linked git worktree, finish commits and tests on the feature branch, then run the merge on `dev` from the primary repository root (`git checkout dev && git pull --rebase origin dev && git merge --no-ff <branch>`).
- **Conflict Resolution:** Resolve any merge conflict markers autonomously, preserving Clean Architecture, CQRS handlers, `.resx` localization (`it`, `de`, `fr`, `en`), and `TreatWarningsAsErrors` compliance.
- **Pre-Push Quality Gate:** Before `git push origin dev`, execute and ensure zero warnings/errors on:
  - `dotnet build --configuration Release`
  - `dotnet test --configuration Release --no-build`
- **Cleanup:** Delete merged local/remote feature branches and remove temporary worktrees (`git worktree remove` + `git branch -D <branch>`).
---
paths:
  - "**"
---

# Working rules for every AI tool (FatturaFacile desktop app)

Single source of the cross-tool rules of this repository. The tool entry files (`CLAUDE.md`, `AGENTS.md`, `.cursorrules`, `.agy/instructions.md`, `.gemini/GEMINI.md`, `.github/copilot-instructions.md`) only point here: change the rules here, not there.

## Language

- Write every piece of code in English: identifiers, comments, docblocks, commit messages, test names, log and exception messages, and these AI rule files. No Italian (or any other language) in code or comments.
- The only exceptions are user-facing texts, which are translated through the translation mechanism of the project, and domain terms that are legal names (for example SDI, IBAN, AHV, QST).
- Chat replies to the owner follow the language the owner writes in.

## Stack

- .NET 11 / WPF / SQLite (Clean Architecture, MVVM).

## Principles

- The mandatory architectural invariants (no DDD patterns, Clean Architecture dependency rule, pure MVVM, StyleCop) are in `AGENTS.md` and `.github/copilot-instructions.md`: they win over anything generic.
- Multi-tenancy and country localization (CH vs IT) rules: `.github/copilot-instructions.md`.
- Payloads match the web-api FormRequests exactly.

## Behaviour

- Fix every error you meet (failing test, broken link, build or type error, wrong payload), also when it is outside the request. Never leave a known error behind: if it cannot be fixed now, say so explicitly.
- Read only what you need: signatures and the relevant parts, not whole files. Emit focused diffs, not whole files. No filler phrases and no unsolicited explanations.
- Follow the sibling files of the area you change (structure, naming, style). Reuse existing components before writing new ones.
- Never add or change dependencies without approval.
- Never write passwords, secrets or tokens as fixed strings. Never read or print `.env*`, `auth.json` or keys.

## API contract

- The web-api FormRequest (`C:\dev\fattura-facile\web-api`) is the source of truth of every payload: same field names, enums, `Y-m-d` dates, nullable and boolean types. When you change one side, change the other side and its tests in the same piece of work.

## Commands

- `dotnet build FatturaFacile.slnx`
- `dotnet test`
- Windows + Git Bash: keep paths short, write scripts with the file tools instead of nested quoting, files may be CRLF.

## Git

- Work on the current branch without review unless told otherwise (a colleague may open PRs: `git fetch` before pushing). Never commit or push unless the owner asks.
- Agent worktrees live in `C:\dev\fattura-facile\wt-*`: remove them when the work ends.

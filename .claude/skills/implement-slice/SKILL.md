---
name: implement-slice
description: Implement one feature or fix as a thin vertical slice with tests first. Use when asked to add or change product behavior.
---

# Implement a slice

1. Read `CONTEXT.md` and the relevant section of `docs/spec.md`. If the behavior is not specified, ask or propose a spec change first.
2. Create `feature/<topic>` or `fix/<topic>` from the latest `main`.
3. Write the failing test at the lowest layer that can express the behavior (Domain or Application first; UI only for UI behavior).
4. Implement through the layers: Domain → Application use case (+ port if new I/O) → Infrastructure → `CompositionRoot` → Desktop feature folder and/or CLI.
5. If output bytes change, add or update a golden case and stop for owner approval before changing any expected file.
6. Run `./scripts/verify.ps1` (add `-Scope full` for UI changes) and paste the command and result.
7. Open a PR with the template filled in. Report state as planned / changed / verified / merged.

Do not: compute a result in a ViewModel, reference Infrastructure from Desktop or CLI, suppress analyzers, or widen the task beyond the request.

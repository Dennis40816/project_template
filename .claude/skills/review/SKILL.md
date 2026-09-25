---
name: review
description: Independent review of a pull request or diff. Use when asked to review changes before merge.
---

# Review

Review as someone who did not write the change; do not reuse the author's conversation.

1. Read the PR description, risk level and verification output. Missing verification is a finding.
2. Check, in order:
   - Behavior matches `docs/spec.md`; golden expected files changed only with recorded owner approval.
   - Layer rules (`AGENTS.md`); one computation path per user-visible result.
   - User-data errors become `Issue`s; `catch` has `when` filters; ranges use `ByteRange` with checked arithmetic.
   - Tests fail without the change and cover the edge cases the change introduces.
   - Any new/old parallel path states its removal version.
3. Report findings ranked by severity with `path:line` and a concrete fix. Say explicitly when there are none.
4. Maximum two review rounds. Anything still open after round two becomes a GitHub issue, not another round.

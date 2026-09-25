---
name: release
description: Prepare and run a release. Use when asked to ship, publish or cut a version.
---

# Release

Follow `docs/release.md`; this skill is the checklist.

1. Confirm `main` CI is green and there is no open issue labelled as release-blocking.
2. Open a PR `chore(release): X.Y.Z` that changes only `VERSION`. Merge it.
3. Run the Release workflow on `main` with `source_sha` = main HEAD and `PREPARE_ONLY`. Report the run URL and result.
4. Only the owner chooses `PUBLISH` and approves the `release` environment. Never create `v*` tags by hand.
5. After publishing, confirm the published-smoke job passed and report the release URL.

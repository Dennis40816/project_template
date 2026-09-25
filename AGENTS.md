# Agent rules

適用於所有 AI coding agent（Codex、Claude Code 或其他）。只寫可以驗證的規則；程序細節放在 `.agents/skills/`。

## 不可違反

- 依賴只能往內：Domain ← Application ← Infrastructure；Bootstrap 組裝；Desktop 與 CLI 只呼叫 Application use case。
  `tests/ProjectTemplate.Architecture.Tests` 會擋，不要修改規則表來讓測試通過，要改先寫 ADR。
- 同一個使用者看得到的結果只有一條計算路徑。UI、CLI、匯出都從同一個 use case 取結果，不在 ViewModel 重算。
- 使用者資料的錯誤回傳 `Issue`，不丟例外；`catch` 一律帶 `when` 過濾。
- 位址與長度用 `ByteRange`（半開區間 `[Start, EndExclusive)`），算術用 `checked`。
- 不修改 golden 預期檔讓測試通過。預期輸出改變 = 行為改變，需要 owner 在 PR 核准，並更新 `testdata/golden/manifest.json` 的核准欄位。
- 不關閉 analyzer、不加 `#pragma warning disable`、不降低 `.editorconfig` 嚴重度，除非使用者明確要求。
- 新增新舊並存的路徑時，在 PR 與 ADR 寫明舊路徑的刪除版本。

## 工作方式

- 開始前讀 `CONTEXT.md`（用語）與 `docs/spec.md`（行為）。
- 在 `feature/<主題>` 或 `fix/<主題>` 分支工作，完成後開 PR；不直接推 `main`。
- 宣稱完成前執行 `./scripts/verify.ps1`，在回報裡貼上指令與結果；沒跑就說沒跑。
- 回報狀態分開寫：planned / changed / verified / merged。
- 審查最多兩輪；兩輪後仍有的意見開 issue，不再迴圈。
- 發現範圍外的 bug：開 GitHub Issue（label `needs-triage`），不順手修。
- Commit 與 PR 標題用 Conventional Commit；AI 產生的 commit 加 `Co-Authored-By:` trailer 註明是哪個 agent。
- 修改 `.agents/skills/` 後執行 `./scripts/sync-agent-skills.ps1`。

## 風險等級

| 等級 | 範圍 | 需要 |
| --- | --- | --- |
| R0 | 文件、註解 | `verify` 可略 |
| R1 | 局部行為，有測試 | 針對性測試 + `verify` |
| R2 | 跨模組或公開介面 | `verify` + 獨立審查（另一個 runtime 或新 session） |
| R3 | 輸出 bytes、golden、release、workflow | `verify -Scope full` + owner 核准 |

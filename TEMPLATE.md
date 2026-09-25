# 使用這個模板

這個模板整理自 FreeformHelper、NVT FW Combiner、NVT Event Buffer Replay 三個專案的經驗（2026-09）。
設計理由與「刻意不放的東西」見 [docs/adr/0003-lessons-from-previous-projects.md](docs/adr/0003-lessons-from-previous-projects.md)。

## 開新專案

1. GitHub 上按 **Use this template** 建立新 repo，clone 下來。
2. 改名並重設狀態：

   ```powershell
   ./scripts/init-project.ps1 -Name Acme.FwTool -Commit
   ```

   它會把所有 `ProjectTemplate` 改成新名字（檔案內容、檔名、資料夾）、`VERSION` 歸零為 0.1.0、
   重新產生 lock file、刪掉這份 TEMPLATE.md 與 UI snapshot。
3. 跑完整驗證，看過 UI snapshot 候選圖後核准：

   ```powershell
   ./scripts/verify.ps1 -Scope full
   ./scripts/approve-snapshots.ps1
   ```

4. 推上 GitHub 後套用 repo 設定（squash-only、自動刪分支、`release` environment、rulesets、labels）：

   ```powershell
   ./scripts/github/apply-repo-settings.ps1 -Repo <owner>/<repo> -Reviewer <你的帳號>
   ```

   私有 repo 的 rulesets 與 environment reviewer 需要付費方案；做不到的步驟會顯示警告並略過。
5. 改寫 `CONTEXT.md`（領域詞彙）與 `docs/spec.md`（產品規格），刪掉範例功能 `Inspect`（或留著當參考）。
6. 第一週內用 release workflow 發出 `v0.1.0`，讓發版路徑一開始就被實際跑過。

## 範例功能 Inspect

`Inspect` 計算檔案長度與 SHA-256，示範一個功能如何貫穿所有層：

Domain `FileFingerprint` → Application `InspectFileUseCase` + port `IFileSource` →
Infrastructure `FileSystemFileSource` → Bootstrap `CompositionRoot` →
Desktop `Features/Inspect` 與 CLI `inspect` 指令 → unit、golden、ui 三種測試。

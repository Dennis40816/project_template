# 發版

`main` 永遠可以發版。版本號只有一個來源：根目錄的 `VERSION`。Tag 只由 release workflow 建立。

## 步驟

1. 開 PR 把 `VERSION` 改成新版本（例如 `chore(release): 0.2.0`），merge。
2. GitHub → Actions → **Release** → Run workflow（分支選 `main`）：
   - `source_sha`：`main` 目前的 HEAD（40 字元）。不是 HEAD 時 candidate 會被略過。
   - `confirm_publish`：先用 `PREPARE_ONLY` 產生候選版，或直接 `PUBLISH`。
   - `summary`：這版的一段人話摘要，放在自動產生的 release notes 上方。
3. Candidate job：檢查版本比上一個 tag 新、tag 不存在 → `verify -Scope full` → 打包 → smoke。
4. `PUBLISH` 時，`release` environment 等你核准，然後建立 annotated tag `vX.Y.Z` 並發布 GitHub Release。
5. Published-smoke job 下載剛發布的檔案，再驗一次 SHA-256、版本與 golden 輸出。

## 發布物

`<Product>-vX.Y.Z-win-x64.zip`（`app/` 桌面程式、`cli/` 命令列、`RELEASE.json`）與 `SHA256SUMS`。
打包用封閉清單，多出任何檔案就失敗。

## Release notes

不維護手寫的 CHANGELOG 檔。Release notes 由 GitHub 從 merge 的 PR 標題與 label 產生（分類設定在 `.github/release.yml`），
再加上你在 workflow 輸入的摘要。所以 PR 標題要寫給使用者看得懂。

## Hotfix

只在需要修補舊版時，從該版 tag 開 `release/X.Y` 分支，修完開 PR 回 `main`，再從 `release/X.Y` 發版。
平常不維持長命的版本分支。

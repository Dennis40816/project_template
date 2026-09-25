# 0002 Trunk-based 開發與 workflow 發版

- 狀態：accepted
- 日期：2026-09-25

## 背景

先前專案同時維護多條版本分支、tag 由人手打、CHANGELOG 手寫數千行，版本號、tag 與文件常常對不起來，還發生過從錯的 base 開分支而需要重建版本。

## 決定

- `main` 是唯一長命分支；功能用短命分支，經 PR squash merge。
- 分支名稱、`main` 保護、`v*` tag 建立權限都用 GitHub ruleset 強制（`scripts/github/apply-repo-settings.ps1`）。
- `VERSION` 是唯一版本來源；release workflow 檢查版本、建立 tag、打包、發布、下載後再驗一次。
- Release notes 由 PR 標題自動產生，不維護 CHANGELOG 檔。
- 只有要修補舊版時才開 `release/X.Y`。

## 後果

- 規則寫在平台設定，不靠記憶；agent 也無法繞過。
- 私有 repo 的 rulesets 需要付費方案；沒有時只剩 CI 與 PR 慣例。

## 刪除期限

不適用。

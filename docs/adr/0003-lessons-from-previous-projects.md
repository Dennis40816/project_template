# 0003 從先前專案學到的事

- 狀態：accepted
- 日期：2026-09-25

## 背景

FreeformHelper、NVT FW Combiner、NVT Event Buffer Replay 三個專案裡，同樣的彎路重複出現。
共同根因：發現問題後的反應是多寫一條規則或多記一份證據，而不是加一個會擋住錯誤的機器檢查。

## 決定

| 先前的彎路 | 這個模板的做法 |
| --- | --- |
| 規則只寫在文件裡（分支 base、tag 命名） | GitHub ruleset、CI 與 release workflow 強制 |
| 證據與治理紀錄塞進 git（change record、attestation） | 證據放 PR 描述與 CI artifact；repo 只留 ADR |
| 行數 ratchet 反覆改規則 | `size-report.ps1` 只提示，不擋 |
| UI 單體，用 partial class 拆檔 | `Features/<功能>/` 的 UserControl + 小 ViewModel |
| 新舊雙軌沒有刪除期限 | ADR 與 PR 模板都有「刪除期限」欄位 |
| 趕版時直推 main | `main` 只接受 PR |
| 過早自建安裝與自動更新 | 發布單一 zip；需要時用現成方案 |
| AI 審查迴圈沒有停損 | 審查最多兩輪，其餘開 issue |
| 架構測試比對原始碼字串 | 只檢查 csproj 與組件 metadata |
| 每個 schema 版本一份全文複本 | 只維護現行 schema，舊版用 migration |
| 根目錄 TODO 檔與版本 ledger | GitHub Issues 與 Milestones |

## 刻意不放進模板

change record JSON、attestation、行數 gate、比對字串的架構測試、測試治理文件本身的測試、自建 updater、
每版一份 handoff 或 ledger 文件、根目錄 TODO 檔。

## 後果

模板本身幾乎沒有治理文件。需要更嚴格的控管時，先問「能不能變成一個會失敗的檢查」，再考慮寫文件。

## 刪除期限

不適用。

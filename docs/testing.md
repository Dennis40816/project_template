# 測試

測試分成四類，一類一個測試專案（也標了 xUnit `Category` trait，方便在 IDE 篩選）。`scripts/verify.ps1` 依範圍挑測試專案執行，跑的是 Microsoft Testing Platform：任何一個專案跑到 0 個測試就算失敗，避免測試框架升級後測試悄悄消失。

| 類別 | 專案 | 測什麼 | 何時跑 |
| --- | --- | --- | --- |
| `unit` | `Domain.Tests`、`Application.Tests` | 領域規則與 use case，port 用 fake | 每個 PR |
| `architecture` | `Architecture.Tests` | 專案參照與編譯後的組件參照是否符合規則表 | 每個 PR |
| `golden` | `Golden.Tests` | 經核准的輸入經過真實 host 的輸出，逐字比對 | 每個 PR |
| `ui` | `Ui.Tests` | Avalonia headless 互動與像素 snapshot | release 候選版（`-Scope full`） |

## 原則

- 架構測試只檢查結構（csproj 與組件 metadata），不比對原始碼或文件字串，改名與重構不會讓它壞掉。
- Golden 失敗代表行為改變。要嘛修程式，要嘛由 owner 核准新的預期檔並更新 `manifest.json` 的 `approvedBy`、`approvedOn`。
- Golden 只比 hash 不算數：每次都要在候選版上實際執行。
- UI snapshot 沒有核准圖時會略過並寫出 `*.received.png`；看過後用 `scripts/approve-snapshots.ps1` 核准。
- 一個測試類別超過約 400 行時拆成多個類別，不要用 partial 拆檔。

## 新增 golden case

1. 把輸入放進 `testdata/golden/cases/`（私有資料只放 hash 與來源說明，不 commit 原檔）。
2. 在 `manifest.json` 加一筆，填 `inputSha256`、`source`、`approvedBy`、`approvedOn`。
3. 產生預期輸出並人工確認後 commit。

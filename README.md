# ProjectTemplate

<!-- 一句話說明這個產品解決什麼問題、給誰用。 -->
ProjectTemplate 是 Windows 桌面工具（.NET 10 + Avalonia 12），附一個共用同一組 use case 的 CLI。

> 第一次使用這個模板？先看 [TEMPLATE.md](TEMPLATE.md)。`init-project.ps1` 執行後會刪除該檔。

## 快速開始

需要：.NET SDK 10.0.3xx、PowerShell 7（Windows PowerShell 5.1 也可以）。

```powershell
./scripts/verify.ps1              # 每個 PR 跑的檢查：build + unit + architecture + golden
./scripts/verify.ps1 -Scope full  # 加上 headless UI 與 snapshot，release 候選版跑這個
dotnet run --project src/ProjectTemplate.Desktop
dotnet run --project src/ProjectTemplate.Cli -- inspect testdata/golden/cases/sample-256.bin
```

## 專案結構

| 路徑 | 內容 |
| --- | --- |
| `src/ProjectTemplate.Domain` | 純領域模型，沒有任何相依 |
| `src/ProjectTemplate.Application` | Use case 與 port（介面） |
| `src/ProjectTemplate.Infrastructure` | Port 的實作（檔案、外部工具） |
| `src/ProjectTemplate.Bootstrap` | 組裝根，唯一知道所有實作的地方 |
| `src/ProjectTemplate.Desktop` | Avalonia UI，`Features/<功能>/` 一個資料夾一個功能 |
| `src/ProjectTemplate.Cli` | 命令列，與 Desktop 呼叫同一組 use case |
| `tests/` | unit、architecture、golden、ui 四類測試，見 [docs/testing.md](docs/testing.md) |
| `testdata/golden/` | Golden 輸入、預期輸出與核准紀錄 |
| `docs/` | [規格](docs/spec.md)、[ADR](docs/adr/)、[發版流程](docs/release.md) |

## 開發流程

1. 從 `main` 開短命分支：`feature/<主題>` 或 `fix/<主題>`。
2. 開 PR，標題用 Conventional Commit（`feat(inspect): show file size`），它會成為 squash 後的 commit。
3. CI 的 `ci-result` 通過後 squash merge，分支自動刪除。
4. 發版見 [docs/release.md](docs/release.md)。

待辦與 bug 一律開 GitHub Issue，不放在 repo 的 TODO 檔。AI agent 的規則見 [AGENTS.md](AGENTS.md)。

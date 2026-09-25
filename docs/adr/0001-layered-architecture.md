# 0001 分層架構與手動組裝

- 狀態：accepted
- 日期：2026-09-25

## 背景

Desktop 與 CLI 要得到一樣的結果，UI 又不能長成單體。先前的專案裡，UI 層佔了大部分程式碼，ViewModel 自己重算結果。

## 決定

- 六個專案：Domain、Application、Infrastructure、Bootstrap、Desktop、Cli。相依只能往內。
- Application 用 port（介面）存取外部，Infrastructure 實作 port。
- Bootstrap 手動組裝物件圖，不用 DI 容器。
- Desktop 以功能分資料夾：`Features/<功能>/` 放 View 與 ViewModel；MainWindow 只當 shell。
- 規則由 `Architecture.Tests` 檢查專案參照與編譯後的組件參照，不比對原始碼字串。

## 後果

- 加功能時要碰多個專案，但每層都小而清楚。
- 組裝程式碼隨功能變多；超過一個畫面讀不完時再考慮 DI 容器。

## 刪除期限

不適用。

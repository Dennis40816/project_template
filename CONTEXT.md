# 領域詞彙

人與 AI agent 共用的用語。程式碼的型別名稱與這裡一致；新概念先加在這裡再寫程式。

| 詞 | 意思 | 程式碼 |
| --- | --- | --- |
| Byte range | 半開區間 `[Start, EndExclusive)`，所有位址與範圍都用這個表示 | `ByteRange` |
| Fingerprint | 檔案內容的身分：長度加小寫 SHA-256 | `FileFingerprint` |
| Issue | 使用者資料的問題，有穩定的代碼，例如 `input.file-not-found` | `Issue` |
| Golden case | 一組經 owner 核准的輸入與預期輸出，逐位元組比對 | `testdata/golden/manifest.json` |

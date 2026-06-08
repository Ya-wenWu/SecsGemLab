# 任務：第一個測試

## 完成的定義
- [x] `dotnet build` 通過
- [x] `dotnet test` 通過
- [x] GitHub Actions CI 綠燈
- [x] spec 文件已寫入 specs/first-test/

## 任務
### T1: 建立專案結構（無依賴）
**檔案：** 多個
- [x] 建立 .NET 8.0 solution + console + xUnit test 專案
- [x] 加入 secs4net / NSubstitute / System.Linq.Async NuGet 套件

### T2: 實作 HSMS 連線測試（依賴：T1）
**檔案：** `tests/SecsGemLab.Tests/HsmsConnectionTests.cs`
- [x] 建立 Host (Active) 與 Equipment (Passive) 的 HsmsConnection
- [x] Host 發送 S1F13，Equipment 回覆 S1F14
- [x] 驗證 Stream/Function 編號與資料內容
- [x] 測試通過

### T3: 設定 CI/CD（依賴：T2）
**檔案：** `.github/workflows/ci.yml`
- [x] dotnet restore → build → test

### T4: 撰寫 spec 文件（可與 T3 並行）
**檔案：** `specs/first-test/`
- [x] requirements.md（需求/驗收條件/邊界情況）
- [x] design.md（技術架構/測試流程）
- [x] tasks.md（有序實作步驟）

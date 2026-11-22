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

### T5: 抽出交握實作並補強（依賴：T2）
**檔案：** `src/SecsGemLab.Console/Handshake/`、`tests/SecsGemLab.Tests/`、`.github/workflows/ci.yml`
- [x] EquipmentIdentity（MDLN/SOFTREV 驗證）、HandshakeMessages、HostSession、EquipmentResponder
- [x] 測試改為有逾時上限、不吞掉背景工作的例外、不寫死連接埠
- [x] 補邊界情況單元測試與整合測試
- [x] CI：修正弱點掃描判斷、加入 concurrency、逾時與覆蓋率輸出
- [ ] 在可還原 NuGet 的環境執行 `dotnet build`、`dotnet test` 確認通過
- [ ] 改用 PipeConnection 記憶體內連線
- [ ] Console 接上 Host/Equipment 模式


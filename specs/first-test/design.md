# 設計：第一個測試

## 技術棧
- .NET 8.0, C# 12, xUnit
- secs4net v3.0.1 (NuGet: Secs4Net)
- NSubstitute (mock ISecsGemLogger)
- System.Linq.Async (FirstAsync)

## 架構邊界
- 擁有：HSMS 連線建立與 S1F13/S1F14 交握測試
- 委派給：secs4net (PipeConnection + HsmsConnection + SecsGem)
- 不負責：真實設備通訊、GEM 狀態機、SECS-II 其他訊息

## 測試流程
```
Host (Active)           Equipment (Passive)
    │                         │
    │──── S1F13 ────────────►│
    │                         │  驗證: S=1, F=13
    │◄─── S1F14 ────────────│
    │                         │  驗證: MDLN, SOFTREV
    │                         │
```

## 檔案
- `src/SecsGemLab.Console/Handshake/`：`EquipmentIdentity`、`HandshakeMessages`（S1F13/S1F14 建構與解析）、`HostSession`、`EquipmentResponder`、`HandshakeException`
- `tests/SecsGemLab.Tests/HsmsConnectionTests.cs`：Host/Equipment 交握整合測試
- `tests/SecsGemLab.Tests/HandshakeMessagesTests.cs`、`EquipmentIdentityTests.cs`：不需連線的單元測試
- `tests/SecsGemLab.Tests/SecsGemLab.Tests.csproj`（NuGet 參考）

## 已知偏差（待處理）
- 需求要求 PipeConnection 記憶體內測試，目前整合測試仍走 127.0.0.1 的 TCP（HsmsConnection），只是改為執行時取得空閒埠。待確認 Secs4Net 3.0.1 的 PipeConnection API 後改寫。
- S1F14 本文依本規格為 `L[MDLN, SOFTREV]`；SEMI E5 標準格式還有前置的 COMMACK 項目。
- Console `Program.cs` 尚未接上 Host/Equipment 模式，因為需要實作 `ISecsGemLogger`。

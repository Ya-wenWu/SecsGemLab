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
- `tests/SecsGemLab.Tests/HsmsConnectionTests.cs`（新增）
- `tests/SecsGemLab.Tests/SecsGemLab.Tests.csproj`（NuGet 參考）

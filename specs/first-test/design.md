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

## Files
- `src/SecsGemLab.Console/Handshake/`: `EquipmentIdentity`, `HandshakeMessages` (build and parse S1F13/S1F14), `HostSession`, `EquipmentResponder`, `HandshakeException`
- `tests/SecsGemLab.Tests/HsmsConnectionTests.cs`: Host/Equipment handshake integration test
- `tests/SecsGemLab.Tests/HandshakeMessagesTests.cs`, `EquipmentIdentityTests.cs`: unit tests that need no connection
- `tests/SecsGemLab.Tests/SecsGemLab.Tests.csproj` (NuGet references)

## Known deviations (to do)
- The requirements ask for an in-memory test with PipeConnection. The integration test still uses TCP on 127.0.0.1 (HsmsConnection), only with a free port chosen at run time. Rewrite it once the Secs4Net 3.0.1 PipeConnection API is confirmed.
- The S1F14 body in this spec is `L[MDLN, SOFTREV]`. The SEMI E5 format also has a leading COMMACK item.
- Console `Program.cs` is not yet wired to a Host/Equipment mode because that needs an `ISecsGemLogger` implementation.

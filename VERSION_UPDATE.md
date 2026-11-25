# 版本更新記錄

| 日期 | 修改內容名稱 | 修改內容大綱 | 修改者 | 註記 |
|---|---|---|---|---|
| 2026-06-09 | v0.4.0 C# MES Interview Cheat Sheet | 新增 ISA-95 / OEE / 21 CFR Part 11 面試重點整理 2 頁 A4 | claudeyw | Root Cause: 準備 C# MES Staff 面試需要濃縮 domain 常識。建議: 面試前搭配 specs/csharp-mes-staff-interview/ 三份 SPEC 一起複習 |
| 2026-11-22 | v0.5.0 交握抽出與強化 | 新增 Handshake 類別（EquipmentIdentity / HandshakeMessages / HostSession / EquipmentResponder）、補測試、強化 CI | Claude | 尚未在可還原 NuGet 的環境 build/test；PipeConnection 與 Console 接線未完成，見 specs/first-test/design.md「已知偏差」 |
| 2026-11-24 | v0.6.0 SECS/GEM 領域知識地圖 | 新增 summary-secsgem.md（課程大綱 4 天對照標準、訊息、本專案進度）與 specs/secsgem-roadmap/ 路線圖 | Claude | 訊息編號與欄位依記憶整理，尚未對照 SEMI 標準全文，design.md 已列待確認項目 |
| 2026-11-25 | v0.7.0 IEC 62443 資安研究 | 新增 summary-iec62443.md（系列結構、SL、FR、4-2、SEMI E187/E188、對 SECS/GEM 的風險與本專案對照），路線圖加入 P5 資安 | Claude | 系列結構、SL、4-2、E187/E188 有來源；FR 名稱、CR 舉例、SECS/GEM 對應為凭記憶，需對照標準 |
| 2026-11-25 | v0.8.0 SECS/GEM 業務邏輯層 | 新增 Gem/：通訊與控制狀態機、HSMS 計時器、S9Fx 訊息、Report 登錄、設備常數、警報、限值監控、Spool、Clock、Trace、配方、E40/E94/E87 狀態機，共 145 個單元測試；HandshakeException 改 public（CA1064） | Claude | 在離線環境以替身 Secs4Net 與 SDK 內建分析器驗證，未跑 Roslynator 與真實 Secs4Net；訊息接線尚未做，進度見 specs/secsgem-roadmap/tasks.md |

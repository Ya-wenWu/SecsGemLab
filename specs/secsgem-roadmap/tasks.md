# 任務：SECS/GEM 領域能力路線圖

## 完成的定義
- [ ] 每個階段都有獨立 spec，並已實作、測試通過
- [ ] CI 綠燈

## 圖例
- [x] 完成並有測試
- [~] 業務邏輯完成並有測試，**尚未接上 SECS 訊息**（沒有 S-F 的建構、解析與 Handler）
- [ ] 未做

程式在 `src/SecsGemLab.Console/Gem/`，測試在 `tests/SecsGemLab.Tests/`。

## P0 基礎
- [ ] T0.1 SML 格式化：`SecsItem` / `SecsMessage` → SML 文字（需先確認 Secs4Net 的 Item 走訪 API）
- [~] T0.2 S9Fx：S9F1/3/5/7/9/11 訊息已可建構（`SystemErrorMessages`）；EquipmentResponder 尚未回 S9F3 / S9F5，因為取不到被拒訊息的 header；S9F13 未做
- [ ] T0.3 S1F14 補上 COMMACK（先確認標準）
- [~] T0.4 Communication State Model：`CommunicationStateMachine`（含 WAIT CRA / WAIT DELAY）；establish 計時器與 S1F13 重送尚未接上
- [~] T0.5 Control State Model：`ControlStateMachine`（含 S1F15–S1F18 的結果值與 `IsRemoteCommandAllowed`）；S1F15–S1F18 訊息未做
- [~] T0.6 HSMS 計時器：`HsmsTimeouts` 已定義與驗證；T5 / T6 / T7 / T8 的行為測試未做（需真實連線）
- [ ] T0.7 以 PipeConnection 取代 TCP 做內存測試

## P1 資料收集（依賴 P0）
- [ ] T1.1 SV：S1F3/F4、S1F11/F12，含巢狀 List（SV 登錄表尚未做）
- [~] T1.2 EC：`EquipmentConstantStore`（範圍檢查、整批套用、Busy）；S2F13–S2F16、S2F29/F30 訊息未做
- [~] T1.3 Collection Event 啟用：`ReportRegistry.SetEnabled`；S6F11/F12、S2F37 訊息未做
- [~] T1.4 Define Report / Link：`ReportRegistry`（DRACK、LRACK 規則）；S2F33–S2F36 訊息未做
- [ ] T1.5 EAP 端解析 S6F11 的 Event / Report
- [~] T1.6 Trace：`TraceSession`（取樣分組與結束）；S2F23/F24、S6F1/F2 訊息與計時排程未做
- [~] T1.7 Limit Monitoring：`LimitMonitor`（死區、多限值）；S2F45–S2F48 訊息與事件觸發未做

## P2 設備操作（依賴 P0、P1）
- [~] T2.1 配方：`RecipeStore`（詢問、下載、上傳、刪除、清單、容量）；S7F1–S7F20 訊息未做
- [~] T2.2 大型配方：單一配方與總量上限已做；T3 / T8 逾時與串流式處理未做
- [ ] T2.3 Remote Control：S2F41/F42、S2F49/F50（Control State 判斷已備好）
- [~] T2.4 Alarm：`AlarmManager`（ALCD 設定位元、啟用、清單）；S5F1–S5F8 訊息未做
- [~] T2.5 Clock：`SecsClock`（12 / 16 位格式與解析）；S2F17/F18、S2F31/F32 訊息未做
- [ ] T2.6 Terminal Service：S10F1–S10F8
- [~] T2.7 Spooling：`SpoolBuffer`（滿載策略、上限、清除）；S2F43/F44、S6F23/F24 與啟動條件未做

## P3 擴充
- [ ] T3.1 自定義 S/F 的 a、b、c、d 四種情境各一個範例與測試

## P4 物料與工作（依賴 P1、P2）
- [~] T4.1 E87：`CarrierStateMachine`（ID、Slot Map、Access）；Load Port 狀態、S3F17/F18、S3F25/F26 未做
- [~] T4.2 E40：`ProcessJobStateMachine`；S16F5/F11/F15/F17 訊息未做
- [~] T4.3 E94：`ControlJobStateMachine`（含與 Process Job 的完成關係）；S14F9、S16F27 訊息未做

## P5 資安（依賴 P0；對應 IEC 62443，見 summary-iec62443.md）
- [ ] T5.1 訊息大小與 List 巢狀深度上限，超過回 S9F11 並計數
- [ ] T5.2 來源 IP 白名單與連線數上限（Passive 端）
- [ ] T5.3 Primary 訊息稽核紀錄（時間、來源、S/F、結果；不寫配方內容）
- [ ] T5.4 高風險 S/F 分級，非 `ONLINE REMOTE` 或未授權來源一律拒絕並稽核
- [ ] T5.5 HSMS 傳輸層 TLS 雙向驗證（先確認 Secs4Net 能否接自訂串流）
- [ ] T5.6 Directory.Packages.props 固定套件版本，移除分析器的 `*` 浮動版本
- [ ] T5.7 CI 產生 SBOM；發佈物簽章
- [ ] T5.8 威脅模型文件（以 summary-iec62443.md 第 5 節為起點）

## 另案
- [ ] SECS-I：先評估第三方函式庫
- [ ] E84：以 I/O 訊號模擬器實作 Active / Passive 與 TP / TD / TA 計時

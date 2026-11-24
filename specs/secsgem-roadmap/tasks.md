# 任務：SECS/GEM 領域能力路線圖

## 完成的定義
- [ ] 每個階段都有獨立 spec，並已實作、測試通過
- [ ] CI 綠燈

## P0 基礎
- [ ] T0.1 SML 格式化：`SecsItem` / `SecsMessage` → SML 文字，用於 log
- [ ] T0.2 S9Fx：S9F1/3/5/7/9/11/13 的建構，EquipmentResponder 對未知 S/F 回 S9F3 / S9F5
- [ ] T0.3 S1F14 補上 COMMACK（先確認標準）
- [ ] T0.4 Communication State Model：DISABLED / ENABLED、WAIT CRA、WAIT DELAY、COMMUNICATING 與 establish 計時器
- [ ] T0.5 Control State Model：OFFLINE 三子狀態、ONLINE LOCAL / REMOTE；S1F15–S1F18
- [ ] T0.6 HSMS T5 / T6 / T7 / T8 逾時測試
- [ ] T0.7 以 PipeConnection 取代 TCP 做內存測試

## P1 資料收集（依賴 P0）
- [ ] T1.1 SV：S1F3/F4、S1F11/F12，含巢狀 List
- [ ] T1.2 EC：S2F13/F14、S2F15/F16、S2F29/F30，含範圍檢查與整批套用
- [ ] T1.3 Collection Event：S6F11/F12、S2F37
- [ ] T1.4 Define Report / Link：S2F33/F34、S2F35/F36
- [ ] T1.5 EAP 端解析 S6F11 的 Event / Report
- [ ] T1.6 Trace：S2F23/F24、S6F1/F2，EAP 端解析
- [ ] T1.7 Limit Monitoring：S2F45–S2F48，含死區

## P2 設備操作（依賴 P0、P1）
- [ ] T2.1 配方：S7F1/F2、F3/F4、F5/F6、F17/F18、F19/F20，含上傳、下載、刪除
- [ ] T2.2 大型配方：T3 / T8 逾時、不一次載入記憶體
- [ ] T2.3 Remote Control：S2F41/F42、S2F49/F50，受 Control State 限制
- [ ] T2.4 Alarm：S5F1–S5F8
- [ ] T2.5 Clock：S2F17/F18、S2F31/F32
- [ ] T2.6 Terminal Service：S10F1–S10F8
- [ ] T2.7 Spooling：S2F43/F44、S6F23/F24，滿載策略

## P3 擴充
- [ ] T3.1 自定義 S/F 的 a、b、c、d 四種情境各一個範例與測試

## P4 物料與工作（依賴 P1、P2）
- [ ] T4.1 E87：Carrier / Load Port 狀態模型、S3F17/F18、S3F25/F26
- [ ] T4.2 E40：Process Job 狀態模型、S16F5/F11/F15/F17
- [ ] T4.3 E94：Control Job 狀態模型、S14F9、S16F27

## 另案
- [ ] SECS-I：先評估第三方函式庫
- [ ] E84：以 I/O 訊號模擬器實作 Active / Passive 與 TP / TD / TA 計時

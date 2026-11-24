# SECS/GEM 領域知識摘要

> 來源：SEMI Standards「SECS/GEM」培訓課程大綱（4 天）對照整理
> 用途：本專案的 domain 知識地圖。每個主題標出對應標準、訊息、本專案目前進度
> 注意：訊息編號與欄位是依 SEMI E5 / E30 / E37 等標準整理，**實作前請對照正式標準**；標準需向 SEMI 購買，本文不是標準的替代品

---

## 0. 名詞與角色

| 名詞 | 意思 |
|------|------|
| EQP | Equipment，設備端（本專案的 Equipment / Passive） |
| EAP | Equipment Automation Program，設備自動化程式，Host 端的通訊程式（本專案的 Host / Active） |
| MES | Manufacturing Execution System，派工、物料、批次追蹤 |
| RMS | Recipe Management System，配方管理系統 |
| SML | SECS Message Language，SECS-II 訊息的人類可讀文字格式，用於 log 與除錯 |

標準分層：

```
E30 GEM        ← 設備「行為」規範（狀態機、能力）
E5  SECS-II    ← 訊息「內容」（Stream/Function、資料格式）
E37 HSMS       ← TCP/IP 傳輸（現代主流）
E4  SECS-I     ← RS-232 序列傳輸（舊設備）
```

---

## Day 1：連線、狀態、資料收集

### 1. SML 介紹
- 寫法：`S1F13 W <L>`（`W` 表示需要回覆）、`<A "MDLN">`、`<U4 100>`、`<L[2] ...>`
- 用途：log、除錯、設計文件、測試案例的描述
- **本專案：** 尚無 SML 輸出，Web 模擬服務是手寫字串。建議做 `Item` → SML 的格式化工具

### 2. HSMS Timeout（E37）

| 參數 | 名稱 | 預設值 | 觸發時 |
|------|------|--------|--------|
| T3 | Reply timeout | 45 s | 等不到 Reply，回報 S9F9 |
| T5 | Connect separation | 10 s | Active 端連線失敗後，隔多久才可再連 |
| T6 | Control transaction | 5 s | Select / Linktest 等控制訊息沒回應 |
| T7 | Not selected | 10 s | 連上 TCP 卻遲遲沒 Select，斷線 |
| T8 | Network intercharacter | 5 s | 同一則訊息的位元組之間間隔過長 |

另有 Linktest（定期探測連線是否活著）。連線狀態：`NOT CONNECTED → CONNECTED (NOT SELECTED) → SELECTED`。
- **本專案：** 測試已走到 SELECTED；T3 以 `HostSession` 的逾時代替。T5/T6/T7/T8 尚未測試

### 3. Communication State Model（E30）
- 狀態：`DISABLED` / `ENABLED`（`NOT COMMUNICATING` 含 `WAIT CRA`、`WAIT DELAY`，與 `COMMUNICATING`）
- 靠 **S1F13 / S1F14** 建立通訊；失敗時等 `EstablishCommunicationsTimeout`（預設 10 秒）後重試
- S1F14 標準格式：`L[2] { COMMACK, L[2] { MDLN, SOFTREV } }`。**COMMACK = 0 才代表接受**
- Host 發的 S1F13 內層是空 `L[0]`；Equipment 發的 S1F13 內層才帶 MDLN / SOFTREV
- **本專案：** 已做 S1F13/S1F14，但 S1F14 依本專案規格是 `L[MDLN, SOFTREV]`，**少了 COMMACK**（見 `specs/first-test/design.md` 已知偏差）。狀態機尚未實作

### 4. Control State Model（E30）

```
EQUIPMENT OFFLINE ─┬─ EQUIPMENT OFFLINE
                   ├─ ATTEMPT ONLINE
                   └─ HOST OFFLINE
ONLINE ────────────┬─ LOCAL     （操作員在機台控制）
                   └─ REMOTE    （Host 可下指令）
```
- 訊息：S1F15/F16 要求離線（OFLACK）、S1F17/F18 要求上線（ONLACK）
- 只有 `ONLINE REMOTE` 時，Host 的遠端指令才應被接受
- **本專案：** Web 模擬服務只有 `EQUIPMENT_ONLINE` / `EQUIPMENT_OFFLINE` 兩個字串，不是真正的狀態機

### 5. Status Data Collection
- **SV（Status Variable）**：設備的即時狀態值，以 SVID 辨識
- S1F3 / S1F4 查詢 SV 值；S1F11 / S1F12 查詢 SV 名稱清單
- EQP 端實作：SV 要登錄 SVID、名稱、單位、取值函式

### 6. EAP 端詢問與解巢狀 List SV Value
- S1F4 回傳 `L[n] { SV1, SV2, ... }`，**順序和 Host 請求的 SVID 順序一致**
- SV 本身可以是 List（巢狀），EAP 要遞迴解析，且要處理 `L[0]`（空值）
- 重點：不要用索引硬寫，應用 SVID 對應型別與名稱

### 7. Equipment Constants（ECV）
- **EC**：可被 Host 設定的設備參數，以 ECID 辨識
- S2F13/F14 讀取、S2F15/F16 設定（回 EAC）、S2F29/F30 查詢名稱與範圍
- 設定時要檢查範圍，超出回 `EAC ≠ 0`，並且是**整批全成功才套用**
- 與 SV 的差別：SV 唯讀、EC 可寫

### 8. 事件通知（Collection Event）
- 設備發生事件（如 `ProcessStarted`），以 CEID 辨識，用 **S6F11** 通知 Host，Host 回 **S6F12**（ACKC6）
- Host 以 **S2F37** 啟用或停用某些 CEID 的通知（CEED）

### 9. EAP 解 S6F11 的 Event Report 與巢狀 Report

```
S6F11 W
<L[3]
  <U4 DATAID>
  <U4 CEID>
  <L[a]                        // a = 此事件連結的 Report 數量
    <L[2] <U4 RPTID> <L[b] V1 V2 ... > >
  >
>
```
- EAP 要先用 CEID 查出已連結的 RPTID，再用 RPTID 查出 VID 清單，才能把值對應到變數名稱
- 回 S6F12 要快，否則設備會因 T3 逾時而重送或 spool

### 10. Define Report 與 Event Report Link
- **S2F33 / F34** Define Report：`RPTID → VID 清單`（DRACK）
- **S2F35 / F36** Link Event Report：`CEID → RPTID 清單`（LRACK）
- 標準做法順序：**先 S2F33 清空舊定義 → S2F33 定義 → S2F35 連結 → S2F37 啟用**
- EQP 端要保存這些定義，並在事件發生時依連結組出 S6F11

---

## Day 2：配方、控制、警報、追蹤

### 1~3. 配方管理（Process Program）
- **S7F1/F2** 詢問能不能收（PPGNT）→ **S7F3/F4** 上傳給設備（下載）→ **S7F5/F6** 要求設備送出（上傳）→ **S7F17/F18** 刪除 → **S7F19/F20** 查詢目前配方清單
- 下載 / 上傳 / 刪除 = 以 EAP 為視角：「下載」是 Host → EQP
- **大型配方**：HSMS 不會把訊息分塊（SECS-I 才有多區塊），所以重點是 T3 / T8 逾時要夠大、記憶體不能一次吃光、S7F1 要先帶長度讓設備評估
- RMS 角色：保存配方版本與核准流程，EAP 負責搬運

### 4. Remote Control
- **S2F41/F42** Host Command Send：`RCMD`（START / STOP / PAUSE / RESUME / ABORT…）加參數清單，回 `HCACK` 與每個參數的 CPACK
- **S2F49/F50** Enhanced Remote Command：多帶 DATAID、OBJSPEC
- 回 `HCACK = 4`（Acknowledge, will finish later）表示已接受、稍後以事件通知完成
- 受 Control State 限制：非 `ONLINE REMOTE` 時應拒絕

### 5. Alarm Management
- **S5F1/F2** 設備主動報警：`ALCD`（最高位元 1 = 發生，0 = 解除；低位元是類別）、`ALID`、`ALTX`
- **S5F3/F4** 啟用 / 停用某警報的通知、**S5F5/F6** 列出全部警報、**S5F7/F8** 列出已啟用的警報
- 警報解除也要通知，不是只通知發生

### 6~7. Trace Data Collection
- **S2F23/F24** Trace Initialize：`TRID, DSPER（取樣週期）, TOTSMP（總取樣數）, REPGSZ（每份報告筆數）, SVID 清單`
- 設備定期以 **S6F1** 送出（TRID, SMPLN, STIME, 一組 SV 值），Host 回 **S6F2**
- EAP 解 S6F1：`L[4] { TRID, SMPLN, STIME, L[n] { L[m] {SV...} } }`，同一份報告含多次取樣，要逐筆拆開

### 8. Limit Monitoring
- **S2F45/F46** Define Variable Limit Attributes：對某 VID 設 `LIMITID` 與 `UPPERDB / LOWERDB`（死區）
- **S2F47/F48** 查詢目前設定
- 變數越過界線時，設備觸發對應事件，以 S6F11 通知
- 死區（deadband）用來避免數值在界線附近來回震盪而不斷發事件

---

## Day 3：Spooling、時間、終端、自定義、錯誤

### 1. Spooling
- 通訊中斷時，設備把要送的 Primary 訊息存起來，恢復後補送，避免資料遺失
- **S2F43/F44** 設定哪些 Stream/Function 要 spool（RSPACK）
- **S6F23/F24** 要求補送（`RSDC = 0`）或清除（`RSDC = 1`）
- 相關參數：`EnableSpooling`、`MaxSpoolTransmit`、`MaxSpoolSize`、`OverWriteSpool`；事件 `SpoolActivated` / `SpoolDeactivated`
- 設計取捨：spool 滿了是丟舊的還是拒絕新的（`OverWriteSpool`）

### 2. Clock
- **S2F17/F18** 讀設備時間、**S2F31/F32** 設定設備時間（TIACK）
- 時間格式依 `TimeFormat` 有 12 或 16 字元（`YYMMDDhhmmss` / `YYYYMMDDhhmmsscc`）
- EAP 設定時間要考慮網路延遲與時區，統一用 UTC 或廠內標準時間

### 3. Terminal Service
- **S10F1/F2** 設備送訊息給 Host、**S10F3/F4** Host 顯示一則文字在設備螢幕
- **S10F5/F6** 多行顯示、**S10F7/F8** 設備不支援多行

### 4. 自定義 Stream / Function 的四種情境

| 情境 | 做法 |
|------|------|
| a. 傳送 SEMI 沒定義的 S/F | 自行定義訊息並約定格式，雙方文件化 |
| b. 接收 SEMI 沒定義的 S/F | Handler 要能辨識，其餘回 S9F5 / S9F3 |
| c. 收標準 S/F，但回覆非標準格式 | 攔截後自行組回覆；注意對方 EAP 是否容得下 |
| d. 覆寫 SEMI 標準 S/F 的處理 | 以自己的 Handler 取代預設行為；要確保不破壞 GEM 合規性 |

> SEMI E5 對使用者自定義的 Stream 範圍有規定，**請對照標準確認可用範圍**。
> **本專案：** `EquipmentResponder` 目前就是「S1F13 以外只計數、不回覆」，是 b、d 的雛形，但沒回 S9Fx

### 5. S9Fx System Error

| 訊息 | 意思 |
|------|------|
| S9F1 | Unrecognized Device ID |
| S9F3 | Unrecognized Stream Type |
| S9F5 | Unrecognized Function Type |
| S9F7 | Illegal Data |
| S9F9 | Transaction Timer Timeout（T3） |
| S9F11 | Data Too Long |
| S9F13 | Conversation Timeout |

- 本體多半是被拒絕訊息的 10 byte header（MHEAD）
- **本專案的 Web 模擬服務把「斷線通知」標成 S9F1，語意不對**，S9F1 是「不認識的 Device ID」

### 6. SECS-I（E4）
- RS-232 序列通訊，以區塊傳輸，最大 244 byte 資料／區塊，需 ENQ / EOT / ACK / NAK 交握
- Timeout：T1（字元間）、T2（協定）、T3（Reply）、T4（區塊間）
- 現代新機台幾乎都用 HSMS，但舊設備仍有；Secs4Net 不含 SECS-I，要另找函式庫或自寫

---

## Day 4：物料與工作管理標準

### 1. E87 Carrier Management（CMS）
- 管理 **Carrier**（FOUP / 卡匣）的進出站、ID 驗證、Slot Map 驗證
- 角色：**EQP** 提供 Load Port 與狀態、**EAP** 轉譯、**MES** 決定要不要放行、**RMS** 與配方相關
- 狀態模型：Carrier ID Status、Slot Map Status、Carrier Access Status，加上 Load Port 的 Transfer State、Reservation、Association
- 流程：Carrier 抵達 → 讀 ID → 驗證（Host 回 `ProceedWithCarrier` 或 `CancelCarrier`）→ 讀 Slot Map → 驗證 → 開始處理 → 處理完成 → `CarrierOut`
- 訊息：`S3F17/F18` Carrier Action Request；`S3F25/F26` Port Action Request；事件以 S6F11 通知

### 2. E40 Process Job
- **一個 Process Job = 對一批晶圓（Substrate）用某配方做加工**
- 與 Control Job 的關係：**一個 Control Job 包含一或多個 Process Job**
- 狀態：`QUEUED/POOLED → SETTING UP → WAITING FOR START → PROCESSING → PROCESS COMPLETE`，另有 `PAUSING / PAUSED / STOPPING / ABORTING`
- 屬性：`PRJobID`、配方（`RecID`）、`PRMtlNameList`（要處理的物料）、`PRProcessStart`（自動或手動開始）
- 訊息：`S16F11` PRJobCreateEnh、`S16F15` PRJobMultiCreate、`S16F5` PRJobCommand（START / PAUSE / RESUME / STOP / ABORT / CANCEL）、`S16F17` PRJobDequeue

### 3. E94 Control Job
- 管理一組 Process Job 的執行順序與物料來源
- 狀態：`QUEUED → SELECTED → WAITING FOR START → EXECUTING → COMPLETED`，另有 `PAUSED`
- 訊息：`S14F9` 建立 Control Job 物件（E39 物件服務）、`S16F27` ControlJobCommand（START / PAUSE / RESUME / STOP / ABORT / CANCEL / HOLD…）

### 4. E84 Parallel I/O（搬運載具與 Load Port 交握）
- **不是 SECS 訊息**，是 AGV / OHT 與設備 Load Port 之間用實體 I/O 訊號做的交握
- Active 端（搬運載具）訊號：`VALID`、`CS_0/CS_1`、`TR_REQ`、`BUSY`、`COMPT`、`CONT`…
- Passive 端（設備）訊號：`L_REQ`、`U_REQ`、`READY`、`HO_AVBL`、`ES`…
- 時序：載具選中 Load Port → 要求搬入 / 搬出 → 設備回 Ready → 載具開始動作 → 完成後釋放
- Timeout：`TP1~TP6`（每一步等待對方訊號的時間）、`TD0~TD3`（延遲）、`TA1~TA3`；**E84 controller** 是負責這層交握的硬體或軟體
- 與 E87 的關係：E84 完成實體交接後，E87 才進行 Carrier 的邏輯管理

---

## 本專案覆蓋進度

| 課程主題 | 標準 | 狀態 |
|---|---|---|
| SML | E5 | 未做 |
| HSMS Timeout | E37 | 部分（只有 T3 的逾時） |
| Communication State Model | E30 | 部分（S1F13/S1F14 已做，無狀態機、無 COMMACK） |
| Control State Model | E30 | 未做 |
| Status Data Collection | E30 | 未做 |
| Equipment Constants | E30 | 未做 |
| 事件通知 / Define Report / Link | E30 | 未做 |
| 配方管理 / 大型配方 | E5 S7 | 未做 |
| Remote Control | E30 S2F41/49 | 未做 |
| Alarm Management | E5 S5 | 未做 |
| Trace Data Collection | E30 S2F23 / S6F1 | 未做 |
| Limit Monitoring | E30 S2F45/47 | 未做 |
| Spooling | E30 | 未做 |
| Clock | E30 | 未做 |
| Terminal Service | E5 S10 | 未做 |
| 自定義 S/F 四情境 | E5 | 雛形（只計數未處理的訊息） |
| S9Fx System Error | E5 | 未做 |
| SECS-I | E4 | 未做（Secs4Net 不支援） |
| E87 Carrier Management | E87 | 未做 |
| E40 Process Job | E40 | 未做 |
| E94 Control Job | E94 | 未做 |
| E84 | E84 | 未做（非 SECS，需 I/O 模擬） |

實作順序與任務見 `specs/secsgem-roadmap/`。

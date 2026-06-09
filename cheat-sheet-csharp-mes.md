# C# MES 面試 Cheat Sheet（2 頁 A4）

---

## 第 1 頁：ISA-95 + OEE

### ISA-95 — MES ↔ ERP 怎麼整合（一句話）

工廠分 5 層，你最需要記 3↔4 中間那條線：

```
L4 (ERP)     SAP/Oracle → 接單、採購、會計
              ↕ ISA-95 定義的資料交換
L3 (MES)     你的系統 → 排產、追批號、收設備資料
L2 (Control) PLC/SCADA → 控制機台
```

**面試要講的兩句話：**
> ISA-95 定義了 ERP 跟 MES 之間該交換什麼資訊 — 生產排程、物料消耗、良率回報。實作上就是 ERP 丟 REST API / MQ 過來，你開工單；做完算完良率，call 回去。

**常見陷阱題：**
- Q: ERP 當機你的 MES 能不能繼續跑？
- A: 要。離線 Queue 住工單，ERP 回來再 sync。

---

### OEE — 設備綜合效率（公式）

**一句話：** `OEE = Availability × Performance × Quality`

**面試要能秒答的三個因子：**

| 因子 | 公式 | 你失去什麼 |
|------|------|-----------|
| **Availability** | 實際運轉 ÷ 計劃生產時間 | 當機、換線 |
| **Performance** | (理想週期 × 產量) ÷ 運轉時間 | 跑太慢、微停機 |
| **Quality** | 良品 ÷ 總產量 | 報廢、重工 |

**世界級 = 85%**（0.9 × 0.95 × 0.99）

**C# 實作陷阱題：**
- Q: 你怎麼即時算 OEE？
- A: SECS/GEM event 通知機台狀態變了，用 timestamp 算 Availability。每做完一片 wafer 報 count → 算 Performance + Quality。SignalR / WebSocket 推 Dashboard。

---

### 六大損失（Six Big Losses）— 知道加分

| OEE 因子 | 損失類型 |
|----------|---------|
| Availability ↓ | ① 設備故障（當機）② 換線/調機 |
| Performance ↓ | ③ 微停機/空轉 ④ 降速 |
| Quality ↓ | ⑤ 製程缺陷 ⑥  startup 降等 |

---

## 第 2 頁：21 CFR Part 11 + Domain 常識

### 21 CFR Part 11 — 電子記錄法規

**一句話：** FDA 規定你不能真的 DELETE，簽名要打密碼。

**面試的三個考點：**

**① Audit Trail（稽核軌跡）**
```sql
-- ❌ 錯
DELETE FROM lot_records WHERE id = 1234

-- ✅ 對
UPDATE lot_records
SET is_deleted = 1, deleted_by = 'user001',
    deleted_at = GETUTCDATE(),
    delete_reason = 'operator entered wrong lot number'
WHERE id = 1234

-- 同時寫 audit_log
INSERT INTO audit_log
VALUES ('lot_records', 1234, 'SOFT_DELETE', '{old json}', 'user001', GETUTCDATE())
```

**② Electronic Signature（電子簽章）**
- 只按一個鈕不夠 → 要 **user ID + password** 雙重驗證
- 連續簽名：第一次全驗，之後可只驗 password
- 簽名意涵要寫清楚：是「製作者」「審核者」還是「核准者」

**③ 面試常見題：**
Q: 你的 MES 有沒有碰過法規要求？
A: 我們的 audit log 不能 truncate，所有資料修改都有 before/after snapshot，簽名要帳號密碼雙因子，符合 21 CFR Part 11 §11.10 / §11.50 / §11.300。

---

### 其他 MES Domain 常識（一句話版）

- **SEMI E5 (SECS-II)** — 機台跟主機溝通的 message 格式（Stream/Function，如 S1F13 = 問候）
- **SEMI E30 (GEM)** — 定義機台的狀態機：Init → 線上可用 → Processing → 當機
- **SEMI E37 (HSMS)** — 走 TCP/IP 傳 SECS-II message
- **SEMI E40/E87/E90/E94 (GEM300)** — 300mm 晶圓廠的進階規範（批次管理、 Carrier 追蹤）
- **Data Integrity (ALCOA+)** — Attributable, Legible, Contemporaneous, Original, Accurate + Complete, Consistent, Enduring, Available
- **GxP** — Good Practice 縮寫（GMP 製造、GLP 實驗室、GCP 臨床）— 有 GxP 的東西就要符合 21 CFR Part 11

---

### 萬用面試句型

> **問為什麼要 MES：** MES bridges the gap between ERP business planning and shop floor execution — it tracks every lot, every wafer, every recipe step in real time.
>
> **問 ISA-95 心得：** It gives us a common language with ERP teams. Instead of arguing about data formats, we agree on the activity model first, then implement.
>
> **問 OEE 經驗：** We calculated OEE from SECS/GEM events — Availability from equipment state changes, Performance from ideal vs actual cycle time, Quality from test results at each operation.
>
> **問法規經驗：** Our MES implements soft delete with full audit trail, electronic signatures with user ID + password per 21 CFR Part 11, and supports ALCOA+ data integrity principles.

---

*Made with assistance from AI — verify against official standards before interview use.*

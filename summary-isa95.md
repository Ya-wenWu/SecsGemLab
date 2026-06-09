# ISA-95 摘要（2 頁 A4）

> 來源：ISA-95 White Paper for Beginners (WBF 2006) + IEC 62264-1:2003
> 用途：C# MES 面試 domain 常識速讀

---

## ISA-95 是什麼

ISA-95（IEC 62264）是企業控制系統整合的國際標準，定義了 **ERP（L4）跟 MES（L3）之間該交換什麼資訊**。它不是法規，而是產業共識的最佳做法。

適用範圍：批次（batch）、連續（continuous）、離散（discrete）製造都涵蓋。

---

## 五層功能模型（Functional Model）

| 層級 | 名稱 | 做什麼 | 時間尺度 |
|------|------|--------|---------|
| L4 | Enterprise (ERP) | 接單、採購、會計 | 月/週 |
| L3 | Manufacturing Operations (MES/MOM) | 排產、追批號、收設備資料 | 班/小時 |
| L2 | Control (SCADA/PLC) | 控制機台 | 秒/毫秒 |
| L1 | Physical Process | 感測器、致動器 | 即時 |
| L0 | Actual Production | 物理製程 | 即時 |

**ISA-95 主要處理 L3 ↔ L4 的介面。**

---

## 設備階層模型（Equipment Hierarchy）

```
Enterprise（公司）
  └── Site（廠區，如台中 Fab）
        └── Area（區域，如蝕刻區）
              └── Production Unit/Line/Cell（生產單元）
```

- 批次產業（Batch）：Process Cell → Unit → Equipment Module → Control Module
- 連續產業（Continuous）：Production Unit → Production Line
- 離散產業（Discrete）：Production Line → Work Cell

---

## 四大資訊類別（L3 ↔ L4 交換的內容）

ISA-95 Part 1 & 2 定義了 L4 與 L3 之間需要交換的四類資訊：

| 類別 | 方向 | 內容 |
|------|------|------|
| **Product Definition** | L4 → L3 | 要做什麼（配方、BOM、製程參數） |
| **Production Capability** | L3 → L4 | 產能（機台狀態、人員、物料） |
| **Production Schedule** | L4 → L3 | 什麼時候做什麼（工單） |
| **Production Performance** | L3 → L4 | 實際做出什麼（產量、良率、耗料） |

每類資訊都包含四種資源：**Personnel、Equipment、Material、Process Segment**。

---

## L3 製造作業四大活動模型

ISA-95 Part 3 定義了 L3 內部的四組活動：

```
Manufacturing Operations (L3)
├── Production Operations（生產作業）
│   ├── Detailed Scheduling → Dispatching → Execution → Data Collection → Tracking → Analysis
├── Maintenance Operations（維護作業）
│   └── 同上結構：排程 → 派工 → 執行 → 收集 → 追蹤 → 分析
├── Quality Operations（品質作業）
│   └── 同上結構
└── Inventory Operations（庫存作業）
    └── 同上結構
```

**面試重點：** MES 不只有生產，還包含維護、品質、庫存。每組作業都有完整的生命週期管理。

---

## B2MML — 實作標準

WBF 組織將 ISA-95 的資訊模型實作為 **B2MML（Business to Manufacturing Markup Language）**，用 XML Schema 定義了 ISA-95 的物件交換格式。C# 實作上可以用 XmlSerializer 或 JSON 序列化來產生/解析 B2MML 訊息。

---

## 面試萬用句

> ISA-95 把工廠分成五層，MES 在 L3，ERP 在 L4。兩邊交換的資訊標準化成四大類：Product Definition、Production Capability、Production Schedule、Production Performance。C# 實作上，我用過 B2MML XML 格式來跟 SAP 交換工單和良率資料 — ERP 丟 schedule 過來，MES 回 performance 回去。

> [被問到 ISA-95 心得] ISA-95 幫我們解決了 IT 跟 OT 之間的溝通問題。以前跟 ERP team 開會各講各的，用了 ISA-95 的術語後，大家知道 Production Schedule 是什麼、Production Performance 要回什麼。整合時間至少省一半。

---

## 延伸資源

- 完整標準：IEC 62264-1 Part 1: Models and Terminology（200+ 頁）
- 實作指南：Bianca Scholten《The Road to Integration》
- ISA 免費模組 IC55M01：https://programs.isa.org/free-training
- B2MML 規格：https://wbﬁ.org/b2mml

---

*摘要基於 ISA-95 White Paper for Beginners（WBF, 2006）及 IEC 62264-1:2003。面試前建議搭配 cheat-sheet-csharp-mes.md 快速複習。*

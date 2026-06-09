# OEE 摘要（2 頁 A4）

> 來源：Vorne OEE Guide + Factbird OEE Guide + TeepTrak OEE Guide (2026)
> 用途：C# MES 面試 domain 常識速讀

---

## OEE 是什麼

**Overall Equipment Effectiveness**（設備綜合效率）是用來衡量製造設備到底有多「賺」的指標。世界級目標 85%。

**一句話定義：** OEE = 做良品的速度 × 不做不良品 × 機台有在動

---

## 公式（必背）

```
OEE = Availability × Performance × Quality
```

每個因子都在 0~100%，三者相乘就是 OEE。

### 範例計算

| 因子 | 計算 | 結果 |
|------|------|------|
| Availability | 運轉 410 min ÷ 計劃生產 480 min | 85.4% |
| Performance | (理想週期 60s × 產量 312) ÷ 運轉 22,410s | 83.5% |
| Quality | 良品 298 ÷ 總產量 312 | 95.5% |
| **OEE** | **0.854 × 0.835 × 0.955** | **68.5%** |

---

## 三個因子詳細拆解

### Availability（可用率）
`Availability = Run Time ÷ Planned Production Time`

**Planned Production Time** = 總排班時間 − 計畫停機（休息、保養）
**Run Time** = Planned Production Time − 非計畫停機（故障、換線）

**常見陷阱：** 很多人只算大故障，忘了微停機和暖機時間 — 這會讓 Availability 虛高 5-10%。

### Performance（性能率）
`Performance = (Ideal Cycle Time × Total Pieces) ÷ Run Time`

**Ideal Cycle Time** = 原廠規格的理想週期（不可以自己調高來美化數字）

**常見陷阱：** 用「可達成的實際週期」取代原廠規格 → Performance 永遠接近 100%，失去意義。

### Quality（良率）
`Quality = Good Pieces ÷ Total Pieces`

**Good Pieces** = 第一次就做對的（first-time-right）
**常見陷阱：** 把重工後 pass 的也算進良品 → 應該是算 first-pass yield。

---

## 六大損失（Six Big Losses）

Nakajima 1988 年 TPM 框架，對應到三個 OEE 因子：

| OEE 因子 | 損失類型 | 例子 |
|----------|---------|------|
| **Availability ↓** | ① 設備故障 | 主軸卡住、感測器失效 |
| | ② 換線/調機 | 換型號、暖機、缺料 |
| **Performance ↓** | ③ 微停機/空轉 | 卡料、sensor 被擋、清潔 |
| | ④ 降速運轉 | 磨損、設定不正確 |
| **Quality ↓** | ⑤ startup 降等 | 開機首批不良 |
| | ⑥ 製程缺陷 | 持續性不良、重工 |

---

## 世界級基準（2026 產業中位數）

| 產業 | 中位數 OEE | 前 25% |
|------|-----------|--------|
| 汽車 Tier 1 | 75-85% | 87-92% |
| 半導體封測 | 76-84% | 87-90% |
| 半導體前段 EUV | 62-72% | 78-82% |
| 製藥 GMP | 62-72% | 78-85% |
| 離散製造（一般） | 65-75% | 78-85% |

**世界級 = 85%**。如果你的 baseline 是 60% 以下，這是正常 — 多數工廠第一次實測 OEE 都在這範圍。

---

## C# MES 實作要點

**即時 OEE 計算 pipeline：**
```
SECS/GEM Event (機台狀態變了)
  → 你的 MES 收 Event
  → 算 Availability（根據 state machine）
  → 算 Performance（根據 wafer count + cycle time）
  → 算 Quality（根據 test result）
  → SignalR / WebSocket 推 Dashboard
```

**關鍵設計決策：**
- 用原廠 spec cycle time，不要自己調
- 微停機（<30s）一定要算，不然 Availability 虛高
- 重工不列入良品（first-pass yield）
- OEE 按設備看，不要跨設備比較
- 先做手動 Excel 算 baseline，再自動化

**資料模型：**
```csharp
public class OeeRecord
{
    public string EquipmentId { get; set; }
    public DateTime ShiftStart { get; set; }
    public double PlannedMinutes { get; set; }
    public double DowntimeMinutes { get; set; }
    public int TotalPieces { get; set; }
    public int GoodPieces { get; set; }
    public double IdealCycleSeconds { get; set; }

    public double Availability => (PlannedMinutes - DowntimeMinutes) / PlannedMinutes;
    public double Performance => (IdealCycleSeconds * TotalPieces) / (PlannedMinutes - DowntimeMinutes) / 60;
    public double Quality => (double)GoodPieces / TotalPieces;
    public double Oee => Availability * Performance * Quality;
}
```

---

## 面試萬用句

> OEE is the standard metric for measuring manufacturing equipment effectiveness. It's calculated as Availability × Performance × Quality. World-class is around 85%. In my MES, I calculated OEE in real-time from SECS/GEM events — equipment state changes for availability, ideal vs actual cycle time for performance, and test results for quality. The dashboard was updated via SignalR so operators could see live OEE on the factory floor.

> [被問到 OEE 經驗] The most common mistake I've seen is using adjusted cycle times instead of manufacturer spec, which inflates Performance to nearly 100%. Another is ignoring micro-stops under 30 seconds, which can hide 5-10 points of Availability loss. I always recommend measuring baseline OEE honestly first, then improving from there.

---

## 延伸資源

- 免費 OEE Excel 計算機：https://teeptrak.com/en/free-oee-calculator-excel-template/
- Vorne OEE PDF：https://www.vorne.com/learn/tools/overall-equipment-effectiveness/
- Factbird OEE 指南：https://www.factbird.com/blog/quick-guide-to-oee
- ISO 22400-2:2014 — OEE 正式標準定義

---

*摘要基於 Vorne、Factbird、TeepTrak 的 OEE 指南 (2026)。面試前建議搭配 cheat-sheet-csharp-mes.md 快速複習。*

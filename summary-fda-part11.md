# 21 CFR Part 11 摘要（2 頁 A4）

> 來源：FDA Guidance for Industry — Part 11, Electronic Records; Electronic Signatures — Scope and Application (2003)
> 用途：C# MES 面試法規常識速讀

---

## 21 CFR Part 11 是什麼

FDA 在 1997 年制定的法規，規定**電子記錄和電子簽章**在什麼條件下可以等同於紙本記錄和手寫簽名。目的是讓藥廠/醫材廠可以合法使用電子系統替代紙本。

**不是所有電子記錄都適用** — 只有那些被「predicate rules」（如 CGMP、GLP、GCP）要求保存的記錄才適用。

---

## 核心三原則（面試必答）

### 1. Audit Trail（稽核軌跡） — §11.10(e)

不能讓使用者悄無聲息地改資料。每次 create / modify / delete 都要記錄：
- 誰做的
- 什麼時間
- 改了什麼（before / after）
- 為什麼改

**C# 實作要點：**
```sql
-- 不能 DELETE，只能軟刪除 + 寫 audit log
INSERT INTO audit_log
VALUES ('lot_record', 1234, 'UPDATE',
        '{old_quantity: 1000}',
        '{new_quantity: 950}',
        'operator001', GETUTCDATE(), 'scrap adjustment')
```

**FDA 裁量權說明：** 2003 年後 FDA 表示對 audit trail 條款採取**執法裁量**（不主動稽查），前提是 predicate rules 的要求有滿足。但不代表你可以不做 — 只是 FDA 暫時不把這當首要檢查重點。

### 2. Electronic Signature（電子簽章） — §11.50 / §11.200

| 要求 | 說明 |
|------|------|
| **唯一性** | 每個人有唯一的 user ID + password combo，不能重複使用（§11.100） |
| **身份驗證** | 簽名前要驗 identity（可離線驗證 HR 文件，§11.100(b)） |
| **雙因子** | 非生物特徵簽名：ID + password，至少兩個 distinct components（§11.200） |
| **連續簽名** | 同一 session 多次簽名：第一次全驗，後續可只驗一個 component（§11.200(a)(1)(i)） |
| **簽名意涵** | 簽名旁要標明意義：是「製作者」「審核者」還是「核准者」（§11.50(a)(3)） |
| **不可剪下複製** | 簽名必須 linked to record，不能剪下貼上偽造（§11.70） |

**C# entity 設計：**
```csharp
public class ElectronicSignature
{
    public string UserId { get; set; }
    public byte[] PasswordHash { get; set; }
    public string SignaturePurpose { get; set; } // review / approve / author
    public DateTime SignedAt { get; set; }
    public string RecordHash { get; set; } // 防止剪下貼上
}
```

### 3. 密碼控制（§11.300）

- 密碼要定期更換（password aging）
- 遺失要立即 deauthorize
- 檢測未授權使用並即時回報

---

## FDA 2003 年後的態度變化（面試加分題）

2003 年 FDA 發布了這份 Scope and Application 指導文件，核心立場三點：

**① 狹義解釋範圍**
- 只有「predicate rules 要求保留且用電子取代紙本」的記錄才適用 Part 11
- 如果你只是用電腦印出紙本、以紙本為準 → 不觸發 Part 11

**② 執法裁量（Enforcement Discretion）**
FDA 表示**暫時不主動稽查**以下四項：
- Validation（§11.10(a)）
- Audit Trail（§11.10(e)）
- Record Retention（§11.10(c)）
- Copies of Records（§11.10(b)）

但注意：predicate rules 本身的紀錄要求（如 CGMP 的 validation）**仍然要遵守**。

**③ Legacy Systems**
1997 年 8 月 20 日之前已經在用的系統 → FDA 不對任何 Part 11 條款執法，只要：
- 當時已符合 predicate rules
- 現在也符合 predicate rules
- 有文件證明系統 fit for intended use

---

## ALCOA+ 資料完整性原則（必考）

MES 領域面試常問的 data integrity 框架，跟 Part 11 高度相關：

| 字母 | 原則 | 中文 | MES 實作 |
|------|------|------|---------|
| A | Attributable | 可歸屬 | audit log 記誰做的 |
| L | Legible | 可讀 | UI 顯示清晰，不亂碼 |
| C | Contemporaneous | 同步 | timestamp 取自系統時間而非使用者輸入 |
| O | Original | 原始 | 不覆蓋原始資料，用版本控制 |
| A | Accurate | 精確 | 輸入驗證、公式驗證 |
| + | Complete | 完整 | 所有相關資料一起保存 |
| + | Consistent | 一致 | 時間戳用 UTC、格式統一 |
| + | Enduring | 持久 | 備份策略、保留期限 |
| + | Available | 可用 | 稽查時能快速調出 |

---

## 面試萬用句

> My MES system implements Part 11 compliant audit trails — all CUD operations go through a service layer that writes before/after snapshots to an immutable audit log. Electronic signatures require user ID + password with explicit purpose (review/approve/author). We follow ALCOA+ data integrity principles: every data point is attributable to a specific operator, with system-generated timestamps, and no hard deletes.

> [被問到 FDA 經驗] I've worked in FDA-regulated manufacturing environments. The key is that Part 11 is not just about compliance checkboxes — it's about designing your system so that every record's lifecycle is traceable. For example, our lot tracking system uses soft deletes with reason codes, and any recipe parameter change requires dual electronic signatures: one from the process engineer, one from QA.

---

*摘要基於 FDA Guidance for Industry (2003)。注意：FDA 可能在未來修訂 Part 11，面試時可補充說明你知道 FDA 正在重新審視此法規。*

# IEC 62443 摘要（含 SEMI E187 / E188）

> 用途：SECS/GEM 設備與 EAP 的資安 domain 知識，以及對本專案的具體要求
> 來源與可信度：
> - 有網頁來源：系列結構、安全等級定義、4-2 組件類型、SEMI E187/E188（來源列在文末）
> - **凭記憶整理，未經來源核對**：七項基礎要求的名稱、CR 編號舉例、對 SECS/GEM 的對應。標準全文需向 IEC / ISA / SEMI 購買，**實作與對外宣稱前請對照正式文件**

---

## 1. 這是什麼

IEC 62443（ISA/IEC 62443）是**工業自動化與控制系統（IACS）資安**的國際標準系列，把責任分給三種角色：

| 角色 | 做什麼 |
|------|--------|
| 資產擁有者（Asset Owner，如晶圓廠） | 建立資安計畫、風險評估、決定各區域要達到的安全等級 |
| 系統整合商（Integrator） | 照安全等級設計、設定、驗收整套系統 |
| 產品供應商（Product Supplier，如設備商、EAP 軟體商） | 以安全開發流程做出符合等級的元件 |

---

## 2. 系列結構

| 群組 | 部分 | 重點 |
|------|------|------|
| 1 通用 | 1-1 | 術語、概念、模型 |
| | 1-2 | 總詞彙表 |
| | 1-3 | 系統資安符合度量 |
| | 1-4 | IACS 資安生命週期與使用案例 |
| 2 政策與流程 | 2-1 | 建立 IACS 資安計畫（資產擁有者） |
| | 2-2 | 資安計畫評等 |
| | 2-3 | 修補程式管理 |
| | 2-4 | 服務提供者的資安計畫要求 |
| | 2-5 | 資產擁有者實作指引 |
| 3 系統 | 3-1 | IACS 資安技術 |
| | 3-2 | 系統設計用的資安風險評估，產出 **Zone and Conduit 模型**與目標安全等級 |
| | 3-3 | 系統資安要求與安全等級（SR） |
| 4 組件 | 4-1 | **安全產品開發生命週期**要求 |
| | 4-2 | **IACS 組件**的技術資安要求（CR） |

**對本專案最相關：4-1（我們怎麼開發）與 4-2（EAP / 設備軟體要具備什麼資安能力）。**

---

## 3. 核心概念

### 安全等級（Security Level）

| 等級 | 防護對象 |
|------|----------|
| SL 1 | 無意或意外的誤用 |
| SL 2 | 低資源、一般技能、低動機的**蓄意**簡單攻擊 |
| SL 3 | 中等資源、懂 IACS、有相當動機的複雜攻擊 |
| SL 4 | 大量資源、懂 IACS、高動機的複雜攻擊 |

三種「SL」不要混：

| 符號 | 意思 | 誰決定 |
|------|------|--------|
| SL-T（Target） | 某個區域想達到的等級 | 資產擁有者，依風險評估 |
| SL-C（Capability） | 系統或元件**能力上**做得到的等級 | 供應商，依 3-3 / 4-2 |
| SL-A（Achieved） | 實際部署後達到的等級 | 整合與驗收 |

### Zone 與 Conduit
- **Zone（區域）**：安全需求相同的資產群組
- **Conduit（管道）**：區域之間的通訊路徑，也要有自己的安全等級
- 3-2 要求：先切分 zone 與 conduit → 逐一做風險評估 → 各自訂 SL-T

### 七項基礎要求（FR）〔凭記憶〕

| FR | 名稱 | 白話 |
|----|------|------|
| FR 1 | 識別與認證控制（IAC） | 誰在連？要驗證 |
| FR 2 | 使用控制（UC） | 驗證後能做什麼？授權與稽核 |
| FR 3 | 系統完整性（SI） | 資料與軟體不被竄改 |
| FR 4 | 資料機密性（DC） | 傳輸與儲存要保密 |
| FR 5 | 受限資料流（RDF） | 網路分段，只放行必要流量 |
| FR 6 | 及時回應事件（TRE） | 記錄、監控、通報 |
| FR 7 | 資源可用性（RA） | 遭攻擊或異常仍要能運作，有備份 |

### 4-2 組件要求
- 組件分四類：**軟體應用（SA）**、**嵌入式裝置（ED）**、**主機裝置（HD）**、**網路裝置（ND）**
- 3-3 的系統要求（SR）在 4-2 變成**組件要求（CR）**；較高等級才需要的加強項稱 **IR**（有些文件稱 RE）
- 大部分 CR 四類通用，另有各類專屬要求：SAR、EDR、HDR、NDR
- 認證：ISASecure CSA，含開發流程評估（以 4-1 為基礎）、開發產出審查、功能性資安評估、弱點測試

---

## 4. SEMI E187 / E188（半導體廠實務）

| 標準 | 適用 | 內容 |
|------|------|------|
| **SEMI E187** 晶圓廠設備資安規格 | 新設備（開發階段） | 作業系統安全、網路安全、端點防護、安全監控；針對 Windows / Linux 設備 |
| **SEMI E188** | 新舊設備、運算元件（電腦、控制器、PLC） | **無惡意程式的部署流程**：惡意程式檢查、弱點檢查、網路防護；定義合格掃描工具、流程、標準化掃描報告 |

- 設備商須在出貨前實作 E187 功能；E188 的流程涵蓋交付、安裝、維護
- 2023 年起 E187 已成為部分晶圓廠採購合約的要求
- **E187 大改版（SNARF 7317，尚未定稿）** 分三級、可累加：
  - SL1 基線：廠商仍支援的 OS、出貨前弱點與惡意程式掃描、**網路流量加密**、基本存取控制
  - SL2 加強：有外部連線（如公網遠端維護）、對稼動或資料完整性要求高的設備
  - SL3 全面：高風險或安全關鍵；加上延伸升級規劃、可追溯性、完整技術文件
- 改版目標是與 IEC 62443 對齊；**來源沒有給出 E187 等級與 62443 SL 的對應**，不可自行假設對等
- 來源沒提到 E187/E188 與 SECS/GEM（E30）的關係

---

## 5. 對 SECS/GEM 的意義〔凭記憶，待對照標準〕

HSMS（E37）本身**沒有驗證與加密**：Select 只比對 Device ID，訊息是明文 TCP。所以資安要靠外層補。

| 風險 | 為什麼危險 | 對應 FR | 建議做法 |
|------|-----------|---------|----------|
| 任何人連上 HSMS 埠就能當 Host | 可下 S2F41 遠端指令、S7F3 下載配方、S2F15 改設備常數 | FR 1、2 | Passive 端 IP 白名單；上 TLS 並做雙向憑證驗證；Control State 限制遠端指令 |
| 明文傳輸，可竊聽與竄改 | 配方是智慧財產，指令可被竄改 | FR 3、4 | TLS 包住 HSMS 連線（自行 `SslStream` 或前置 TLS 終結） |
| 網路不分段 | 一台機被入侵可橫向移動 | FR 5 | 設備網段與 MES 網段分開，EAP↔EQP 只開 HSMS 埠 |
| 無稽核 | 事後查不出誰下過指令 | FR 2、6 | 記錄每一則 Primary 訊息：時間、來源、S/F、結果（配方內容不寫入 log） |
| 畸形或超大訊息、洪水 | 設備卡死或記憶體耗盡 | FR 7 | 訊息大小上限、List 巢狀深度上限、連線數與速率限制、逾時 |
| 高風險訊息無分級 | 所有 S/F 一視同仁 | FR 2 | 依風險分級：S2F41/49、S7F3/17、S2F15、S2F31、S10F3 需 `ONLINE REMOTE` 且來源已授權 |
| 秘密外洩 | 憑證與設定被推上 GitHub | FR 3 | 憑證不進版控；設定走環境變數或 secret store |
| 相依套件有弱點 | 供應鏈風險 | 4-1、2-3 | 持續弱點掃描、釘版本、SBOM |

---

## 6. 本專案對照（62443-4-1 安全開發實務）

| 實務 | 本專案現況 |
|------|-----------|
| 秘密掃描 | 有（CI 的 gitleaks） |
| 相依弱點掃描 | 有（CI，已修正判斷邏輯，並含間接相依） |
| 靜態分析 | 有（CodeQL、`AnalysisMode All`、TreatWarningsAsErrors） |
| 相依自動更新 | 有（dependabot） |
| 弱點回報流程 | 有（SECURITY.md） |
| 程式碼審查 | 有（CODEOWNERS、AI code review） |
| 套件版本固定 | **沒有**（分析器套件用 `*` 浮動版本） |
| SBOM | 沒有 |
| 發佈物簽章 | 沒有 |
| 威脅建模 | 沒有（本文件第 5 節是起點） |
| HSMS 傳輸安全（TLS / 來源限制） | 沒有 |
| 訊息稽核與輸入限制 | 沒有 |

**重要：本專案是學習用實驗專案，沒有通過任何 62443 或 E187 評估，不可對外宣稱符合。** 目標是讓設計方向與這些標準一致。

---

## 來源

- [Overview of the ISA/IEC 62443 Standards (Dragos)](https://www.dragos.com/blog/isa-iec-62443-overview)：系列結構
- [The Essential Guide to the IEC 62443 industrial cybersecurity standards (Industrial Cyber)](https://industrialcyber.co/features/the-essential-guide-to-the-iec-62443-industrial-cybersecurity-standards/)：SL 定義、SL-T / SL-C
- [IEC 62443-4-2, the need to secure components (INCIBE-CERT)](https://www.incibe.es/en/incibe-cert/blog/iec-62443-4-2-need-secure-components)：組件類型、CR / IR、ISASecure CSA
- [How SEMI E187 and E188 standards elevate cybersecurity (SEMI)](https://www.semi.org/en/standards-watch-2024-mar/how-semi-e187-and-e188-standards-elevate-cybersecurity)
- [E187 major revision: tiered security levels (SEMI, 2026-08)](https://www.semi.org/en/standards-watch-2026-aug/e187-major-revision-tiered-security-levels)
- [IEC 62443-4-2 (IEC Webstore)](https://webstore.iec.ch/en/publication/34421)：標準全文購買處

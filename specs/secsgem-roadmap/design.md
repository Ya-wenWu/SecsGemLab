# 設計：SECS/GEM 領域能力路線圖

## 架構邊界
- 擁有：GEM 狀態機、各能力的 EQP / EAP 實作、訊息建構與解析
- 委派給：Secs4Net（HSMS 傳輸與 SECS-II 資料項）
- 不負責：真實設備、SECS-I 序列通訊、E84 的實體 I/O

## 分層

```
EquipmentApp / HostApp          ← 組合各能力
  ├─ Capabilities/<Name>/       ← 每個能力一個資料夾
  │    ├─ <Name>Messages        ← 建構與解析（純函式，可單元測試）
  │    ├─ <Name>Equipment       ← EQP 端行為
  │    └─ <Name>Host            ← EAP 端行為
  └─ Core/                      ← 狀態機、ID 登錄（SVID/ECID/CEID/RPTID/ALID）、SML 格式化
```

延續現有 `Handshake/` 的做法：訊息類別是靜態純函式、行為類別接收 `SecsGem`。

## 資安層（P5，詳見 summary-iec62443.md）
```
Transport:  TCP ─► [TLS 雙向驗證] ─► HSMS          ← FR 3、4
Admission:  來源 IP 白名單、連線數上限             ← FR 1、5、7
Dispatch:   訊息大小 / List 深度上限 → 風險分級 → Handler   ← FR 2、7
Audit:      每則 Primary 訊息一筆紀錄（不含配方內容）      ← FR 6
```
高風險訊息（S2F15、S2F31、S2F41、S2F49、S7F3、S7F17、S10F3）僅在 `ONLINE REMOTE` 且來源已授權時處理。

## 相依順序
```
SML ─┬─ S9Fx ─┬─ Communication State ─ Control State ─┬─ SV / EC
     │        │                                       ├─ Collection Event → Report → Link → Trace / Limit
     │        │                                       ├─ Remote Control / Alarm / Clock / Terminal
     │        │                                       ├─ 配方管理
     │        │                                       └─ Spooling（需先有事件與主動訊息）
     └────────┴─ 自定義 S/F
E87 ─ E40 ─ E94（需先有事件、Remote Control、配方）
```

## 待確認（需對照 SEMI 標準）
- S1F14 是否加入 COMMACK（現行規格沒有）
- 使用者自定義 Stream / Function 的可用範圍（E5）
- E84 的 TA 計時器定義與預設值
- Secs4Net 3.0.1 的 `PipeConnection` API（現行測試仍走 TCP）
- Secs4Net 是否允許自訂底層串流（以便套 `SslStream`），否則需前置 TLS 終結
- 七項 FR 與 CR 編號對應（summary-iec62443.md 第 3、5 節為凭記憶整理）

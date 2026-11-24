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

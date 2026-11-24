# 需求：SECS/GEM 領域能力路線圖

## 目的
讓本專案涵蓋 SEMI SECS/GEM 培訓課程（4 天）的全部主題，領域知識見 `summary-secsgem.md`。
每個主題都要能以 Host（EAP）與 Equipment（EQP）兩端在同一個測試中互相驗證。

## 能力
- 必須：每個能力同時有 EQP 端與 EAP 端實作，並以整合測試證明兩端可互通
- 必須：訊息建構與解析與傳輸分離，可不連線就做單元測試（延續 `HandshakeMessages` 的做法）
- 必須：訊息內容以 SML 輸出，方便 log 與除錯
- 必須：失敗情境回對應的 S9Fx 或 ACK 碼，不得只丟例外或沉默
- 絕不能：使用真實設備或對外網路連線
- 絕不能：未對照 SEMI 標準就把訊息格式寫死；有疑義處記在對應 design.md 的「待確認」

## 範圍
| 階段 | 內容 |
|---|---|
| P0 基礎 | SML 輸出、S9Fx、HSMS Timeout、Communication State、Control State |
| P1 資料收集 | SV、EC、Collection Event、Define Report / Link、Trace、Limit Monitoring |
| P2 設備操作 | 配方管理（含大型配方）、Remote Control、Alarm、Clock、Terminal Service、Spooling |
| P3 擴充 | 自定義 Stream/Function 四情境 |
| P4 物料與工作 | E87、E40、E94 |
| 另案 | SECS-I（Secs4Net 不支援）、E84（非 SECS，需 I/O 模擬） |

## 驗收條件
- [ ] 每個階段都有獨立的 `specs/<name>/` 三份文件
- [ ] 每個能力至少一個正常流程與一個失敗流程的整合測試
- [ ] `dotnet build -warnaserror` 與 `dotnet test` 通過
- [ ] `summary-secsgem.md` 的「覆蓋進度」表隨實作更新

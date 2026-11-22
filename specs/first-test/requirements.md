# 需求：SecsGemLab 第一個測試

## 目的
驗證 secs4net 函式庫在 .NET 環境下能正常建立 HSMS 連線並完成 S1F13/S1F14 交握。

## 能力
- 必須：使用 secs4net (NuGet) 建立 Host (Active) 與 Equipment (Passive) 連線
- 必須：Host 發送 S1F13 (Establish Communications Request)
- 必須：Equipment 回應 S1F14 (Establish Communications Confirm) 含 MDLN 與 SOFTREV
- 必須：使用 PipeConnection 進行記憶體內測試（不依賴真實 TCP）
- 絕不能：使用真實設備或網路連線
- 絕不能：依賴外部服務

## 驗收條件
- [ ] Host 成功發送 S1F13
- [ ] Equipment 正確收到 S1F13 並辨識 Stream/Function
- [ ] Equipment 回應 S1F14 含 MDLN 與 SOFTREV
- [ ] Host 收到 S1F14 並能讀取 MDLN 與 SOFTREV 值
- [ ] 整個交握在 10 秒內完成

## 邊界情況
- S1F14 缺失（Equipment 未回覆）：Host 於逾時後丟出 TimeoutException，不得無限等待
- S1F14 格式錯誤（Stream/Function 不符、項目數不是 2、MDLN/SOFTREV 為空或非 ASCII）：Host 丟出 HandshakeException
- MDLN 與 SOFTREV 最長 20 個 ASCII 字元（SEMI E5）；超過或為空白時建構 EquipmentIdentity 即失敗
- Equipment 收到非 S1F13 的訊息：不回覆，只累計 UnhandledMessageCount
- 測試使用的連接埠於執行時取得空閒埠，不得寫死

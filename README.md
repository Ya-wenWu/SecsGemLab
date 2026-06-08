# SecsGemLab

SECS/GEM protocol learning lab on .NET — starting with the S1F13/S1F14 establish-communications handshake using `secs4net`.

## First Test

`HsmsConnectionTests.Host_EstablishesCommunication_WithEquipment`

Creates an in-memory HSMS connection between Host (Active) and Equipment (Passive), sends an Establish Communications Request (S1F13), verifies the Establish Communications Confirm reply (S1F14) with MDLN and SOFTREV.

## Tech Stack

- .NET 8.0, C# 12, xUnit
- secs4net v3.0.1 (NuGet: Secs4Net)
- NSubstitute + System.Linq.Async

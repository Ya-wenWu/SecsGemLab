# SECS/GEM Domain Knowledge Summary

> Source: the 4-day SEMI Standards SECS/GEM training course outline, mapped to standards, messages and this project
> Purpose: the domain knowledge map for this project. Each topic lists the standard, the messages, and how far the project has got.
> Caveat: message numbers and fields are summarized from SEMI E5 / E30 / E37 and **must be checked against the official standards before implementing**. The standards are sold by SEMI; this file is not a substitute.

---

## 0. Terms and roles

| Term | Meaning |
|------|---------|
| EQP | Equipment. The equipment side (Equipment / Passive in this project). |
| EAP | Equipment Automation Program. The host-side communication program (Host / Active in this project). |
| MES | Manufacturing Execution System: dispatching, material and lot tracking. |
| RMS | Recipe Management System. |
| SML | SECS Message Language: a human-readable text form of SECS-II messages, used for logs and debugging. |

Standard layers:

```
E30 GEM        <- equipment BEHAVIOR (state machines, capabilities)
E5  SECS-II    <- message CONTENT (Stream/Function, data formats)
E37 HSMS       <- TCP/IP transport (the modern default)
E4  SECS-I     <- RS-232 serial transport (older equipment)
```

---

## Day 1: connection, state, data collection

### 1. SML
- Syntax: `S1F13 W <L>` (`W` means a reply is expected), `<A "MDLN">`, `<U4 100>`, `<L[2] ...>`
- Used for logs, debugging, design documents and describing test cases
- **This project:** no SML output yet. The Web simulation builds its text by hand. A formatter from `Item` to SML is a good first tool.

### 2. HSMS timeouts (E37)

| Timer | Name | Default | When it fires |
|-------|------|---------|---------------|
| T3 | Reply timeout | 45 s | No reply arrived; reported with S9F9 |
| T5 | Connect separation | 10 s | Minimum wait before an active entity reconnects |
| T6 | Control transaction | 5 s | No answer to Select / Linktest and similar control messages |
| T7 | Not selected | 10 s | TCP connected but never selected; the connection is closed |
| T8 | Network intercharacter | 5 s | Too long a gap between bytes of one message |

Also Linktest (periodic check that the connection is alive). Connection states: `NOT CONNECTED -> CONNECTED (NOT SELECTED) -> SELECTED`.
- **This project:** tests reach SELECTED. T3 is approximated by the timeout in `HostSession`. `HsmsTimeouts` defines the parameters. Behavior tests for T5/T6/T7/T8 are not written.

### 3. Communication state model (E30)
- States: `DISABLED` / `ENABLED` (`NOT COMMUNICATING` with `WAIT CRA` and `WAIT DELAY`, and `COMMUNICATING`)
- Communication is established with **S1F13 / S1F14**. On failure the equipment waits `EstablishCommunicationsTimeout` (default 10 s) and retries.
- Standard S1F14: `L[2] { COMMACK, L[2] { MDLN, SOFTREV } }`. **Only COMMACK = 0 means accepted.**
- An S1F13 sent by the host carries an empty inner `L[0]`; an S1F13 sent by the equipment carries MDLN / SOFTREV.
- **This project:** S1F13/S1F14 works, but the S1F14 in this project's spec is `L[MDLN, SOFTREV]`, **missing COMMACK** (see "Known deviations" in `specs/first-test/design.md`). `CommunicationStateMachine` implements the states; it is not wired to messages.

### 4. Control state model (E30)

```
EQUIPMENT OFFLINE -+- EQUIPMENT OFFLINE
                   +- ATTEMPT ONLINE
                   +- HOST OFFLINE
ONLINE ------------+- LOCAL     (operator controls at the tool)
                   +- REMOTE    (host may send commands)
```
- Messages: S1F15/F16 request OFF-LINE (OFLACK), S1F17/F18 request ON-LINE (ONLACK)
- Host remote commands should only be accepted in `ONLINE REMOTE`
- **This project:** `ControlStateMachine` implements the model, including `IsRemoteCommandAllowed`. The S1F15-S1F18 messages are not written. The Web simulation still uses two plain strings, `EQUIPMENT_ONLINE` and `EQUIPMENT_OFFLINE`.

### 5. Status data collection
- **SV (Status Variable):** a live equipment value, identified by SVID
- S1F3 / S1F4 read SV values; S1F11 / S1F12 read the SV name list
- EQP side: register SVID, name, units and a getter for each SV

### 6. EAP side: querying and unpacking list SV values
- S1F4 returns `L[n] { SV1, SV2, ... }` in **the same order as the SVIDs the host asked for**
- An SV can itself be a list (nested), so the EAP must parse recursively and handle `L[0]` (empty)
- Do not hard-code indexes. Use the SVID to find each type and name.

### 7. Equipment constants (ECV)
- **EC:** an equipment parameter the host can set, identified by ECID
- S2F13/F14 read, S2F15/F16 write (answer EAC), S2F29/F30 list names and ranges
- A write must be range-checked, answer `EAC != 0` when out of range, and apply **only if the whole request is valid**
- Difference from SV: SV is read-only, EC is writable
- **This project:** `EquipmentConstantStore`

### 8. Event notification (collection events)
- When something happens (for example `ProcessStarted`) the equipment sends **S6F11** with a CEID, and the host answers **S6F12** (ACKC6)
- The host uses **S2F37** to enable or disable notification for CEIDs (CEED)

### 9. EAP side: unpacking S6F11 event reports and nested reports

```
S6F11 W
<L[3]
  <U4 DATAID>
  <U4 CEID>
  <L[a]                        // a = number of reports linked to this event
    <L[2] <U4 RPTID> <L[b] V1 V2 ... > >
  >
>
```
- The EAP first looks up the RPTIDs linked to the CEID, then the VID list of each RPTID, to map values to variable names
- Answer S6F12 quickly, otherwise the equipment hits T3 and may resend or start spooling

### 10. Define report and event report link
- **S2F33 / F34** Define Report: `RPTID -> list of VIDs` (DRACK)
- **S2F35 / F36** Link Event Report: `CEID -> list of RPTIDs` (LRACK)
- Standard order: **S2F33 with an empty list to clear -> S2F33 define -> S2F35 link -> S2F37 enable**
- The EQP keeps these definitions and builds an S6F11 from the links when an event occurs
- **This project:** `ReportRegistry` implements the DRACK / LRACK / ERACK rules

---

## Day 2: recipes, control, alarms, tracing

### 1-3. Recipe management (process programs)
- **S7F1/F2** may I send? (PPGNT) -> **S7F3/F4** send to equipment (download) -> **S7F5/F6** request from equipment (upload) -> **S7F17/F18** delete -> **S7F19/F20** list current recipes
- "Download / upload / delete" are from the EAP's point of view: "download" is Host -> EQP
- **Large recipes:** HSMS does not split messages into blocks (SECS-I does), so the concerns are large enough T3 / T8, not loading everything into memory at once, and sending the length in S7F1 so the equipment can decide
- RMS role: keeps recipe versions and approvals; the EAP moves them
- **This project:** `RecipeStore` (size limits, replace, atomic delete)

### 4. Remote control
- **S2F41/F42** Host Command Send: `RCMD` (START / STOP / PAUSE / RESUME / ABORT ...) with a parameter list; answers `HCACK` and a CPACK per parameter
- **S2F49/F50** Enhanced Remote Command: adds DATAID and OBJSPEC
- `HCACK = 4` (acknowledge, will finish later) means accepted; completion is reported by an event
- Restricted by control state: reject when not `ONLINE REMOTE`

### 5. Alarm management
- **S5F1/F2** equipment reports an alarm: `ALCD` (top bit 1 = set, 0 = cleared; the low bits are the category), `ALID`, `ALTX`
- **S5F3/F4** enable or disable reporting of an alarm, **S5F5/F6** list all alarms, **S5F7/F8** list enabled alarms
- Clearing an alarm must be reported too, not only setting it
- **This project:** `AlarmManager`

### 6-7. Trace data collection
- **S2F23/F24** Trace Initialize: `TRID, DSPER (sampling period), TOTSMP (total samples), REPGSZ (samples per report), list of SVIDs`
- The equipment sends **S6F1** periodically (TRID, SMPLN, STIME, a set of SV values); the host answers **S6F2**
- EAP unpacking of S6F1: `L[4] { TRID, SMPLN, STIME, L[n] { L[m] {SV...} } }`. One report holds several samples, so split them one by one.
- **This project:** `TraceSession`

### 8. Limit monitoring
- **S2F45/F46** Define Variable Limit Attributes: for a VID, set a `LIMITID` with `UPPERDB / LOWERDB` (dead band)
- **S2F47/F48** read the current settings
- When a variable crosses a limit the equipment raises the matching event, reported with S6F11
- The dead band keeps a value near a limit from raising events over and over
- **This project:** `LimitMonitor`. It reports every zone change; confirm the exact UPPERDB / LOWERDB semantics against the standard.

---

## Day 3: spooling, clock, terminal, custom messages, errors

### 1. Spooling
- While communication is down the equipment stores the primary messages it would have sent and delivers them after recovery, so no data is lost
- **S2F43/F44** choose which Stream/Function pairs to spool (RSPACK)
- **S6F23/F24** request delivery (`RSDC = 0`) or purge (`RSDC = 1`)
- Related parameters: `EnableSpooling`, `MaxSpoolTransmit`, `MaxSpoolSize`, `OverWriteSpool`; events `SpoolActivated` / `SpoolDeactivated`
- Design trade-off: when the spool is full, drop the oldest or refuse new messages (`OverWriteSpool`)
- **This project:** `SpoolBuffer<T>`

### 2. Clock
- **S2F17/F18** read the equipment time, **S2F31/F32** set the equipment time (TIACK)
- The time has 12 or 16 characters depending on `TimeFormat` (`YYMMDDhhmmss` / `YYYYMMDDhhmmsscc`)
- When the EAP sets the time, allow for network delay and time zone; use UTC or one plant standard
- **This project:** `SecsClock`

### 3. Terminal service
- **S10F1/F2** the equipment sends text to the host, **S10F3/F4** the host shows a line of text on the equipment screen
- **S10F5/F6** multi-line display, **S10F7/F8** the equipment does not support multi-line

### 4. The four custom Stream / Function scenarios

| Scenario | What to do |
|----------|-----------|
| a. Send an S/F that SEMI does not define | Define the message yourself and agree the format; document it on both sides |
| b. Receive an S/F that SEMI does not define | The handler must recognize it; reject the rest with S9F5 / S9F3 |
| c. Receive a standard S/F but reply in a non-standard form | Intercept it and build your own reply; check that the other EAP can accept it |
| d. Override how a standard S/F is handled | Replace the default behavior with your own handler; make sure GEM compliance is not broken |

> SEMI E5 sets the range of Streams available for user definition. **Check the standard for the allowed range.**
> **This project:** `EquipmentResponder` currently counts any message other than S1F13 and does not answer it. That is a start for b and d, but it does not send S9Fx.

### 5. S9Fx system errors

| Message | Meaning |
|---------|---------|
| S9F1 | Unrecognized device ID |
| S9F3 | Unrecognized stream type |
| S9F5 | Unrecognized function type |
| S9F7 | Illegal data |
| S9F9 | Transaction timer timeout (T3) |
| S9F11 | Data too long |
| S9F13 | Conversation timeout |

- The body is mostly the 10-byte header (MHEAD) of the rejected message
- **The Web simulation labels a "disconnect notice" as S9F1, which is wrong.** S9F1 means "unrecognized device ID".
- **This project:** `SystemErrorMessages` builds S9F1/3/5/7/9/11; S9F13 has a different body and is not covered

### 6. SECS-I (E4)
- RS-232 serial communication in blocks of up to 244 data bytes, with an ENQ / EOT / ACK / NAK handshake
- Timers: T1 (intercharacter), T2 (protocol), T3 (reply), T4 (interblock)
- New tools almost always use HSMS, but older equipment still uses SECS-I. Secs4Net does not include SECS-I, so another library or custom code is needed.

---

## Day 4: material and job management standards

### 1. E87 Carrier Management (CMS)
- Manages **carriers** (FOUP / cassette): arrival, ID verification, slot map verification
- Roles: **EQP** provides the load port and state, **EAP** translates, **MES** decides whether to release, **RMS** relates to recipes
- State models: Carrier ID Status, Slot Map Status, Carrier Access Status, plus the load port Transfer State, Reservation and Association
- Flow: carrier arrives -> read ID -> verify (host answers `ProceedWithCarrier` or `CancelCarrier`) -> read slot map -> verify -> start processing -> processing complete -> `CarrierOut`
- Messages: `S3F17/F18` Carrier Action Request; `S3F25/F26` Port Action Request; events are reported with S6F11
- **This project:** `CarrierStateMachine` covers ID, slot map and access; the load port transfer state is not covered

### 2. E40 Process Job
- **A process job applies one recipe to a group of wafers (substrates)**
- Relation to a control job: **one control job contains one or more process jobs**
- States: `QUEUED/POOLED -> SETTING UP -> WAITING FOR START -> PROCESSING -> PROCESS COMPLETE`, plus `PAUSING / PAUSED / STOPPING / ABORTING`
- Attributes: `PRJobID`, recipe (`RecID`), `PRMtlNameList` (material to process), `PRProcessStart` (automatic or manual start)
- Messages: `S16F11` PRJobCreateEnh, `S16F15` PRJobMultiCreate, `S16F5` PRJobCommand (START / PAUSE / RESUME / STOP / ABORT / CANCEL), `S16F17` PRJobDequeue
- **This project:** `ProcessJobStateMachine`

### 3. E94 Control Job
- Manages the order of execution and the material source of a set of process jobs
- States: `QUEUED -> SELECTED -> WAITING FOR START -> EXECUTING -> COMPLETED`, plus `PAUSED`
- Messages: `S14F9` create the control job object (E39 object services), `S16F27` ControlJobCommand (START / PAUSE / RESUME / STOP / ABORT / CANCEL / HOLD ...)
- **This project:** `ControlJobStateMachine`

### 4. E84 Parallel I/O (handoff between a transport vehicle and a load port)
- **Not a SECS message.** It is a handshake over physical I/O signals between an AGV / OHT and the equipment load port.
- Active side (the vehicle) signals: `VALID`, `CS_0/CS_1`, `TR_REQ`, `BUSY`, `COMPT`, `CONT` ...
- Passive side (the equipment) signals: `L_REQ`, `U_REQ`, `READY`, `HO_AVBL`, `ES` ...
- Sequence: vehicle selects the load port -> asks to load or unload -> equipment answers ready -> vehicle moves -> releases when done
- Timers: `TP1~TP6` (how long to wait for the other side at each step), `TD0~TD3` (delays), `TA1~TA3`; the **E84 controller** is the hardware or software that runs this layer
- Relation to E87: E87 manages the carrier logically after E84 has completed the physical handoff

---

## Project coverage

"Business logic" means state machines and rules are implemented and unit tested. "Message wiring" means the matching S-F message building, parsing and handlers.
**Only S1F13/S1F14 and the S9Fx builders have a message layer today.** Everything else is not yet wired.

| Course topic | Standard | Business logic | Message wiring |
|---|---|---|---|
| SML | E5 | Not done | Not done |
| HSMS timeouts | E37 | Timer parameters | T3 timeout (handshake); T5-T8 not done |
| Communication state model | E30 | Done | S1F13/S1F14 done; COMMACK missing |
| Control state model | E30 | Done | Not done |
| Status data collection | E30 | Not done | Not done |
| Equipment constants | E30 | Done | Not done |
| Event notification / define report / link | E30 | Done | Not done |
| Recipe management / large recipes | E5 S7 | Done (size limits) | Not done |
| Remote control | E30 S2F41/49 | Condition ready | Not done |
| Alarm management | E5 S5 | Done | Not done |
| Trace data collection | E30 S2F23 / S6F1 | Done | Not done |
| Limit monitoring | E30 S2F45/47 | Done | Not done |
| Spooling | E30 | Done | Not done |
| Clock | E30 | Done | Not done |
| Terminal service | E5 S10 | Not done | Not done |
| Custom S/F, four scenarios | E5 | Start (counts unhandled messages) | Not done |
| S9Fx system errors | E5 | n/a | Builders done; not used by the responder |
| SECS-I | E4 | Not done (Secs4Net does not support it) | Not done |
| E87 Carrier Management | E87 | ID / slot map / access | Not done |
| E40 Process Job | E40 | Done | Not done |
| E94 Control Job | E94 | Done | Not done |
| E84 | E84 | Not done (not SECS; needs I/O simulation) | n/a |

The implementation order and tasks are in `specs/secsgem-roadmap/`.

For the security side (HSMS has no authentication or encryption, IEC 62443, SEMI E187/E188) see `summary-iec62443.md`.

# Tasks: SECS/GEM capability roadmap

## Definition of done
- [ ] Every phase has its own spec, is implemented and tested
- [ ] CI is green

## Legend
- [x] Done and tested
- [~] Business logic done and tested, **not yet wired to SECS messages** (no S-F building, parsing or handler)
- [ ] Not done

Code is in `src/SecsGemLab.Console/Gem/`, tests in `tests/SecsGemLab.Tests/`.

## P0 Foundation
- [ ] T0.1 SML formatting: `SecsItem` / `SecsMessage` to SML text (first confirm how Secs4Net walks an Item)
- [~] T0.2 S9Fx: S9F1/3/5/7/9/11 can be built (`SystemErrorMessages`); `EquipmentResponder` does not send S9F3 / S9F5 yet because the header of the rejected message is not available; S9F13 not done
- [ ] T0.3 Add COMMACK to S1F14 (confirm against the standard first)
- [~] T0.4 Communication state model: `CommunicationStateMachine` (including WAIT CRA / WAIT DELAY); the establish timer and S1F13 resend are not wired
- [~] T0.5 Control state model: `ControlStateMachine` (result values of S1F15-S1F18 and `IsRemoteCommandAllowed`); the S1F15-S1F18 messages are not done
- [~] T0.6 HSMS timers: `HsmsTimeouts` is defined and validated; behavior tests for T5 / T6 / T7 / T8 are not done (they need a real connection)
- [ ] T0.7 Replace TCP with PipeConnection for in-memory tests

## P1 Data collection (depends on P0)
- [ ] T1.1 SV: S1F3/F4, S1F11/F12, including nested lists (no SV registry yet)
- [~] T1.2 EC: `EquipmentConstantStore` (range checks, all-or-nothing writes, busy); the S2F13-S2F16 and S2F29/F30 messages are not done
- [~] T1.3 Collection event enable: `ReportRegistry.SetEnabled`; the S6F11/F12 and S2F37 messages are not done
- [~] T1.4 Define report / link: `ReportRegistry` (DRACK and LRACK rules); the S2F33-S2F36 messages are not done
- [ ] T1.5 EAP side parsing of S6F11 events and reports
- [~] T1.6 Trace: `TraceSession` (sample grouping and end); the S2F23/F24 and S6F1/F2 messages and the sampling scheduler are not done
- [~] T1.7 Limit monitoring: `LimitMonitor` (dead band, several limits); the S2F45-S2F48 messages and event triggering are not done

## P2 Equipment operation (depends on P0, P1)
- [~] T2.1 Recipes: `RecipeStore` (inquire, download, upload, delete, list, capacity); the S7F1-S7F20 messages are not done
- [~] T2.2 Large recipes: per-recipe and total size limits done; T3 / T8 timeouts and streaming are not done
- [ ] T2.3 Remote control: S2F41/F42, S2F49/F50 (the control state check is ready)
- [~] T2.4 Alarms: `AlarmManager` (ALCD set bit, enable, lists); the S5F1-S5F8 messages are not done
- [~] T2.5 Clock: `SecsClock` (12 and 16 character formats and parsing); the S2F17/F18 and S2F31/F32 messages are not done
- [ ] T2.6 Terminal service: S10F1-S10F8
- [~] T2.7 Spooling: `SpoolBuffer` (full-spool policy, limits, purge); the S2F43/F44 and S6F23/F24 messages and activation conditions are not done

## P3 Extension
- [ ] T3.1 One example and test for each of the four custom S/F scenarios (a, b, c, d)

## P4 Material and jobs (depends on P1, P2)
- [~] T4.1 E87: `CarrierStateMachine` (ID, slot map, access); load port state, S3F17/F18 and S3F25/F26 are not done
- [~] T4.2 E40: `ProcessJobStateMachine`; the S16F5/F11/F15/F17 messages are not done
- [~] T4.3 E94: `ControlJobStateMachine` (including completion when all process jobs finish); the S14F9 and S16F27 messages are not done

## P5 Security (depends on P0; maps to IEC 62443, see summary-iec62443.md)
- [ ] T5.1 Message size and list nesting depth limits; answer S9F11 and count when exceeded
- [ ] T5.2 Source IP allow-list and connection limit (passive side)
- [ ] T5.3 Audit record for every primary message (time, source, S/F, result; never recipe content)
- [ ] T5.4 Risk tiers for high-risk S/F: reject and audit anything not in `ONLINE REMOTE` or from an unauthorized source
- [ ] T5.5 Mutual TLS on the HSMS transport (first confirm Secs4Net can take a custom stream)
- [ ] T5.6 Pin package versions in Directory.Packages.props and remove the floating `*` analyzer versions
- [ ] T5.7 Generate an SBOM in CI; sign release artifacts
- [ ] T5.8 Threat model document (start from section 5 of summary-iec62443.md)

## Separate
- [ ] SECS-I: evaluate third-party libraries first
- [ ] E84: implement Active / Passive with an I/O signal simulator and TP / TD / TA timers

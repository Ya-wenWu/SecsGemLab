# Requirements: SECS/GEM capability roadmap

## Purpose
Cover every topic of the SEMI SECS/GEM training course (4 days) in this project. The domain knowledge is in `summary-secsgem.md`.
Each topic must be verifiable with both a Host (EAP) and an Equipment (EQP) side in the same test.

## Capabilities
- Must: every capability has both an EQP side and an EAP side, proven to interoperate by an integration test
- Must: message building and parsing are separate from transport, so they can be unit tested without a connection (as `HandshakeMessages` already does)
- Must: message content can be printed as SML for logs and debugging
- Must: failure cases answer with the matching S9Fx or ACK code, not only an exception or silence
- Must never: use real equipment or an outside network connection
- Must never: hard-code a message format without checking the SEMI standard; record doubts under "To confirm" in the matching design.md
- Must never: claim compliance with IEC 62443 or SEMI E187 (this project has not been assessed)

## Scope
| Phase | Content |
|---|---|
| P0 Foundation | SML output, S9Fx, HSMS timeouts, communication state, control state |
| P1 Data collection | SV, EC, collection events, define report / link, trace, limit monitoring |
| P2 Equipment operation | Recipe management (including large recipes), remote control, alarms, clock, terminal service, spooling |
| P3 Extension | The four custom Stream/Function scenarios |
| P4 Material and jobs | E87, E40, E94 |
| P5 Security | Per `summary-iec62443.md`: transport security, source restriction, audit, input limits, risk tiers for messages |
| Separate | SECS-I (not supported by Secs4Net), E84 (not SECS; needs I/O simulation) |

## Acceptance criteria
- [ ] Each phase has its own `specs/<name>/` with the three documents
- [ ] Each capability has at least one normal-flow and one failure-flow integration test
- [ ] `dotnet build -warnaserror` and `dotnet test` pass
- [ ] The coverage table in `summary-secsgem.md` is updated as work lands

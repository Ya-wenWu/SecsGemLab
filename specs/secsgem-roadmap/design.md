# Design: SECS/GEM capability roadmap

## Architecture boundary
- Owns: GEM state machines, the EQP and EAP side of each capability, message building and parsing
- Delegates to: Secs4Net (HSMS transport and SECS-II data items)
- Not responsible for: real equipment, SECS-I serial communication, the physical I/O of E84

## Layers

```
EquipmentApp / HostApp          <- composes the capabilities
  +- Capabilities/<Name>/       <- one folder per capability
  |    +- <Name>Messages        <- building and parsing (pure functions, unit-testable)
  |    +- <Name>Equipment       <- EQP side behavior
  |    +- <Name>Host            <- EAP side behavior
  +- Core/                      <- state machines, ID registries (SVID/ECID/CEID/RPTID/ALID), SML formatter
```

Same approach as the existing `Handshake/`: message classes are static pure functions, behavior classes take a `SecsGem`.

**Current state:** the business logic (state machines and rules) sits in a flat `Gem/` folder and deliberately does not depend on Secs4Net, so it can be unit tested without a real connection. The message layer and handlers are not wired yet. They will be organized into `Capabilities/<Name>/` as shown above.

## Security layer (P5, see summary-iec62443.md)
```
Transport:  TCP -> [mutual TLS] -> HSMS                          <- FR 3, 4
Admission:  source IP allow-list, connection limit               <- FR 1, 5, 7
Dispatch:   size / list-depth limits -> risk tier -> handler     <- FR 2, 7
Audit:      one record per primary message (no recipe content)   <- FR 6
```
High-risk messages (S2F15, S2F31, S2F41, S2F49, S7F3, S7F17, S10F3) are handled only in `ONLINE REMOTE` and from an authorized source.

## Dependency order
```
SML -+- S9Fx -+- Communication state - Control state -+- SV / EC
     |        |                                       +- Collection event -> Report -> Link -> Trace / Limit
     |        |                                       +- Remote control / Alarm / Clock / Terminal
     |        |                                       +- Recipe management
     |        |                                       +- Spooling (needs events and unsolicited messages first)
     +--------+- Custom S/F
E87 - E40 - E94 (need events, remote control and recipes first)
```

## To confirm (check against the SEMI standards)
- Whether S1F14 should gain a COMMACK item (the current spec has none)
- The range of Stream / Function numbers available for user definition (E5)
- The definition and defaults of the E84 TA timers
- The `PipeConnection` API in Secs4Net 3.0.1 (the integration test still uses TCP)
- Whether Secs4Net allows a custom underlying stream (to wrap it in `SslStream`); otherwise a TLS-terminating front end is needed
- The mapping of the seven FRs and the CR numbers (sections 3 and 5 of summary-iec62443.md are from memory)

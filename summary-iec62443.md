# IEC 62443 Summary (including SEMI E187 / E188)

> Purpose: security domain knowledge for SECS/GEM equipment and EAP software, and what it means for this project
> Sources and confidence:
> - **Backed by a web source:** series structure, security level definitions, 62443-4-2 component types, SEMI E187/E188 (sources listed at the end)
> - **From memory, not checked against a source:** the names of the seven foundational requirements, the example CR numbers, and the mapping to SECS/GEM
> - The standards are sold by IEC / ISA / SEMI. **Check the official text before implementing or making any compliance claim.**

---

## 1. What it is

IEC 62443 (ISA/IEC 62443) is the international standard series for **industrial automation and control system (IACS) security**. It splits responsibility between three roles:

| Role | What they do |
|------|--------------|
| Asset owner (for example a fab) | Runs the security program, does risk assessment, decides the security level each zone must reach |
| System integrator | Designs, configures and accepts the whole system to the chosen security level |
| Product supplier (for example an equipment or EAP software vendor) | Builds components that meet the level using a secure development lifecycle |

---

## 2. Series structure

| Group | Part | Focus |
|-------|------|-------|
| 1 General | 1-1 | Terminology, concepts and models |
| | 1-2 | Master glossary |
| | 1-3 | System security conformance metrics |
| | 1-4 | IACS security lifecycle and use cases |
| 2 Policies and procedures | 2-1 | Establishing an IACS security program (asset owner) |
| | 2-2 | Security program ratings |
| | 2-3 | Patch management |
| | 2-4 | Security program requirements for service providers |
| | 2-5 | Implementation guidance for asset owners |
| 3 System | 3-1 | Security technologies for IACS |
| | 3-2 | Security risk assessment for system design; produces the **zone and conduit model** and target security levels |
| | 3-3 | System security requirements and security levels (SR) |
| 4 Component | 4-1 | **Secure product development lifecycle** requirements |
| | 4-2 | Technical security requirements for **IACS components** (CR) |

**Most relevant to this project: 4-1 (how we develop) and 4-2 (what security capabilities EAP / equipment software must have).**

---

## 3. Core concepts

### Security levels

| Level | Protects against |
|-------|------------------|
| SL 1 | Unintentional or accidental misuse |
| SL 2 | Intentional misuse by simple means, few resources, general skills, low motivation |
| SL 3 | Sophisticated means, moderate resources, IACS-specific knowledge, reasonable motivation |
| SL 4 | Sophisticated means, extensive resources, IACS-specific knowledge, high motivation |

Do not mix up the three "SL" terms:

| Symbol | Meaning | Who decides |
|--------|---------|-------------|
| SL-T (Target) | The level a zone should reach | Asset owner, from the risk assessment |
| SL-C (Capability) | The level a system or component is capable of | Supplier, per 3-3 / 4-2 |
| SL-A (Achieved) | The level actually reached once deployed | Integration and acceptance |

### Zones and conduits
- **Zone:** a group of assets with the same security needs
- **Conduit:** a communication path between zones, with its own security level
- 3-2 requires: partition the system into zones and conduits -> assess the risk of each -> set an SL-T for each

### The seven foundational requirements (FR) [from memory]

| FR | Name | In plain words |
|----|------|----------------|
| FR 1 | Identification and authentication control (IAC) | Who is connecting? Verify them |
| FR 2 | Use control (UC) | What may they do once verified? Authorization and audit |
| FR 3 | System integrity (SI) | Data and software are not tampered with |
| FR 4 | Data confidentiality (DC) | Protect data in transit and at rest |
| FR 5 | Restricted data flow (RDF) | Segment the network, allow only needed traffic |
| FR 6 | Timely response to events (TRE) | Log, monitor, report |
| FR 7 | Resource availability (RA) | Keep working under attack or fault, have backups |

### 62443-4-2 components
- Four component types: **software application (SA)**, **embedded device (ED)**, **host device (HD)**, **network device (ND)**
- The system requirements (SR) of 3-3 become **component requirements (CR)** in 4-2; extras needed only at higher levels are called **IR** (some documents say RE)
- Most CRs are shared by all four types; each type also has its own: SAR, EDR, HDR, NDR
- Certification: ISASecure CSA, including a development process assessment (based on 4-1), a review of development artifacts, a functional security assessment and vulnerability testing

---

## 4. SEMI E187 / E188 (semiconductor practice)

| Standard | Applies to | Content |
|----------|-----------|---------|
| **SEMI E187**, cybersecurity of fab equipment | New equipment (during development) | Operating system security, network security, endpoint protection, security monitoring; aimed at Windows / Linux equipment |
| **SEMI E188** | New and existing equipment, computing components (computers, controllers, PLCs) | **Malware-free deployment process:** malware checks, vulnerability checks, network protection; defines qualified scanning tools, procedures and standard scan reports |

- Suppliers must implement E187 features before shipping; the E188 process covers delivery, installation and maintenance
- Since 2023 E187 has been a requirement in some fabs' procurement contracts
- **The major E187 revision (SNARF 7317, not final)** has three cumulative levels:
  - SL1 baseline: vendor-supported OS, pre-shipment vulnerability and malware scans, **encrypted network traffic**, basic access control
  - SL2 enhanced: equipment with external connections (for example remote support over public networks) or higher uptime or data integrity needs
  - SL3 comprehensive: high-risk or safety-critical settings; adds extended upgrade planning, traceability and full technical documentation
- The revision aims to align with IEC 62443. **The source gives no mapping between E187 levels and 62443 SLs, so do not assume they are equivalent.**
- The sources do not mention how E187 / E188 relate to SECS/GEM (E30)

---

## 5. What it means for SECS/GEM [from memory, to be checked against the standards]

HSMS (E37) itself has **no authentication and no encryption**: Select only compares the device ID, and messages are plain TCP. Security has to be added around it.

| Risk | Why it is dangerous | FR | Suggested approach |
|------|--------------------|----|--------------------|
| Anyone who reaches the HSMS port can act as the host | Can send S2F41 remote commands, S7F3 recipe downloads, S2F15 constant changes | FR 1, 2 | IP allow-list on the passive side; TLS with mutual certificate authentication; limit remote commands by control state |
| Plain-text transport can be sniffed or altered | Recipes are intellectual property; commands can be changed | FR 3, 4 | Wrap the HSMS connection in TLS (`SslStream` or a TLS-terminating front end) |
| Flat network | One compromised machine can move sideways | FR 5 | Separate the equipment and MES network segments; open only the HSMS port between EAP and EQP |
| No audit trail | Cannot tell afterwards who sent a command | FR 2, 6 | Log every primary message: time, source, S/F, result (never log recipe contents) |
| Malformed or huge messages, floods | Equipment hangs or runs out of memory | FR 7 | Message size limit, list nesting depth limit, connection and rate limits, timeouts |
| No risk tiers for messages | Every S/F is treated alike | FR 2 | Tier by risk: S2F41/49, S7F3/17, S2F15, S2F31, S10F3 need `ONLINE REMOTE` and an authorized source |
| Leaked secrets | Certificates and settings pushed to GitHub | FR 3 | Keep certificates out of version control; use environment variables or a secret store |
| Vulnerable dependencies | Supply chain risk | 4-1, 2-3 | Continuous vulnerability scanning, pinned versions, SBOM |

---

## 6. This project against the 62443-4-1 secure development practices

| Practice | Current state |
|----------|---------------|
| Secret scanning | Yes (gitleaks in CI) |
| Dependency vulnerability scanning | Yes (CI; direct packages block, transitive packages are reported only) |
| Static analysis | Yes (CodeQL, `AnalysisMode All`, TreatWarningsAsErrors) |
| Automatic dependency updates | Yes (dependabot) |
| Vulnerability reporting process | Yes (SECURITY.md) |
| Code review | Yes (CODEOWNERS, AI code review) |
| Pinned package versions | **No** (analyzer packages use floating `*` versions) |
| SBOM | Generated on release; not in CI |
| Signed release artifacts | No |
| Threat model | No (section 5 above is a starting point) |
| HSMS transport security (TLS / source restriction) | No |
| Message audit and input limits | No |

**Important: this is a learning lab. It has not been assessed against any 62443 or E187 requirement and must not be described as compliant.** The aim is to keep the design direction consistent with these standards.

---

## Sources

- [Overview of the ISA/IEC 62443 Standards (Dragos)](https://www.dragos.com/blog/isa-iec-62443-overview): series structure
- [The Essential Guide to the IEC 62443 industrial cybersecurity standards (Industrial Cyber)](https://industrialcyber.co/features/the-essential-guide-to-the-iec-62443-industrial-cybersecurity-standards/): SL definitions, SL-T / SL-C
- [IEC 62443-4-2, the need to secure components (INCIBE-CERT)](https://www.incibe.es/en/incibe-cert/blog/iec-62443-4-2-need-secure-components): component types, CR / IR, ISASecure CSA
- [How SEMI E187 and E188 standards elevate cybersecurity (SEMI)](https://www.semi.org/en/standards-watch-2024-mar/how-semi-e187-and-e188-standards-elevate-cybersecurity)
- [E187 major revision: tiered security levels (SEMI, 2026-08)](https://www.semi.org/en/standards-watch-2026-aug/e187-major-revision-tiered-security-levels)
- [IEC 62443-4-2 (IEC Webstore)](https://webstore.iec.ch/en/publication/34421): where to buy the standard

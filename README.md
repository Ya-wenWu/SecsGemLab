# SecsGemLab

SECS/GEM protocol learning lab on .NET.

Implements HSMS (High-Speed SECS Message Services) communication patterns using
[Secs4Net](https://github.com/anonymous19840113/Secs4Net). Started with the
S1F13/S1F14 establish-communications handshake.

## Status

Early experiments — not production-ready. Contributions and experiments welcome.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

```bash
git clone https://github.com/Ya-wenWu/SecsGemLab.git
cd SecsGemLab
dotnet build
dotnet test
```

## Project Structure

```
SecsGemLab/
├── src/
│   └── SecsGemLab.Console/     # Console app (entry point)
├── tests/
│   └── SecsGemLab.Tests/       # xUnit tests
├── specs/                       # SDD specs (requirements/design/tasks)
├── .github/
│   ├── workflows/
│   │   ├── ci.yml              # CI: build + test
│   │   └── ai-code-review.yml  # AI-powered PR code review
│   └── scripts/
│       └── code_review.py      # Review script (Gemini API)
└── docs/                        # Reference summaries
```

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Language | C# 12 |
| Runtime | .NET 8.0 |
| Protocol | SECS/GEM via Secs4Net |
| Testing | xUnit + NSubstitute |
| AI Review | Gemini 2.0 Flash |

## License

MIT
# AI Code Review E2E test

# Contributing

## Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feat/your-idea`)
3. Make changes
4. Run `dotnet test` — all tests must pass
5. Commit with [conventional commit](https://www.conventionalcommits.org/) message
6. Push and open a Pull Request

## Standards

- All PRs require 1 approval
- CI checks must pass
- No hardcoded secrets (gitleaks blocks them)
- SBOM is generated on every tagged release

## Code Style

- Follow .editorconfig rules
- `dotnet format` before committing
- Enable nullable reference types

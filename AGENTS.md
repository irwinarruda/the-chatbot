# Agent Guidelines

## Commands

- Build: `dotnet build`
- Test all: `make test-local` (Local), `make test-dev` (Dev), `make test-prev` (Preview)
- Test watch: `cd Tests && make test-local` (Local), `make test-dev` (Dev), `make test-prev` (Preview)
- Single test: `dotnet test --filter "FullyQualifiedName=Namespace.TestClass.TestMethod"`
- Database: `make migrations-up` (apply), `make migrations-down` (revert), `make migrations-create name=Name` (create)
- Run locally: `make run-local` (starts API + ngrok)

## Code Style

- Indentation: 2 spaces
- Line endings: LF
- File encoding: UTF-8
- C# braces: Same line (no new line before braces)
- Imports: System directives first, separated groups
- Naming: PascalCase for types/methods, camelCase for parameters/variables
- Use dependency injection and interfaces for services
- Handle exceptions using custom Infra.Exception clasks
- Return types must be explicitly declared
- Use dependency injection via constructor parameters
- Follow Resource/Service/Controller architecture pattern

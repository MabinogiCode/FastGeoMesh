# Contributing Guide

Thanks for your interest in contributing!

## Workflow
1. Fork & create a feature branch (`feat/...`, `fix/...`).
2. Write/adjust tests (all existing tests must pass).
3. Ensure `dotnet build` and `dotnet test` succeed (local + via CI).
4. Submit a Pull Request with a clear description (link issue if applicable).

## Code Style
- C# 12/13 features allowed where they improve clarity.
- Use explicit `var` when the right-hand side type is obvious; otherwise full type.
- Keep public API XML documented.
- Follow existing naming (PascalCase for public; camelCase locals).

## Tests
- Place new tests under `tests/FastGeoMesh.Tests`.
- Favor deterministic checks (avoid random seeds unless fixed).
- Exporter tests: keep files in temp directory and delete afterwards.

## Performance
- Avoid premature allocation optimizations; open an issue first for large changes.
- Provide benchmarks (e.g. BenchmarkDotNet) for substantial perf PRs.

## Commit Messages
- Conventional prefix recommended: `feat:`, `fix:`, `docs:`, `test:`, `build:`, `chore:`.

## Security
- Report sensitive issues privately if any arise (no known attack surface currently).

## License
By contributing you agree your contributions are MIT licensed.

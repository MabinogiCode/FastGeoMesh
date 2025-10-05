# Changelog

## [Unreleased]

### Added
- Created `CHANGELOG.md` to document changes.
- Introduced a `Result` pattern for safer error handling, removing exceptions from the validation flow.
- Added `Core` directory for foundational code like the `Result` pattern.

### Changed
- **Breaking Change**: `MesherOptionsBuilder.Build()` now returns a `Result<MesherOptions>`.
- **Breaking Change**: `PrismMesher.Mesh()`, `MeshAsync()`, `MeshWithProgressAsync()`, and `MeshBatchAsync()` now return a `Result<T>`.
- Refactored the `Mesh` class to use `ImmutableList<T>`, making it inherently thread-safe and removing the need for locks.
- Introduced `EdgeLength` and `Tolerance` value objects to enforce domain invariants (e.g., positive, within a valid range) and improve code clarity.
- Refactored long methods `BuildZLevels` in `MeshStructureHelper` and `PointInPolygon` in `GeometryHelper` by extracting logic into smaller, more manageable private methods.
- Updated all relevant parts of the codebase, including tests, to align with these refactoring changes.

### Removed
- Removed manual locking from the `Mesh` class.
- Removed redundant validation logic from `MesherOptions` that is now handled by the `EdgeLength` and `Tolerance` value objects.

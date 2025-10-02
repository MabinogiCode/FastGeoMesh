using System.Diagnostics.CodeAnalysis;

// Suppress analyzer and compiler warnings introduced by stricter editorconfig for test sources.
// We intentionally suppress missing XML comment warnings and unnecessary using analyzer for tests.
[assembly: SuppressMessage("Style", "IDE0005:Remove unnecessary usings", Justification = "Auto-fixed by test suite helper; keep suppressed to avoid build break during refactor.")]
[assembly: SuppressMessage("Compiler", "CS1591", Justification = "Test project does not require XML documentation for public test methods.")]

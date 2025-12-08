# Lineage: Ancestral Legacies — Delivery Roadmap (Dev Branch)

## Guiding Goals
- Ship a stable, movement-system-agnostic core (IMovementController) with validated AI behaviors.
- Ensure IDE/Unity integration: debugging (coreclr), error surfacing, MCP/Pro Tools pipeline.
- Establish repeatable test coverage for movement, AI, and data components.
- Improve performance observability (logging/runtime diagnostics) and asset organization under `Assets/_Project`.

## Workstreams & Milestones

### 1) Movement & AI Stability
- **M1.1** Refine movement controllers: add obstacle handling in `GridPathfinder` (A* proper), tune acceleration/stopping thresholds.
- **M1.2** Expand movement tests: cover acceleration caps, stopping distance, disabled-state behavior.
- **M1.3** AI behaviors: verify Pop AI state transitions and resource gathering with movement controller swap (Simple vs Grid vs NavMesh).
- **M1.4** Regression pass: ensure legacy `Pop` and `PopController` compatibility paths still function.

### 2) Logging & Diagnostics
- **M2.1** Complete migration from `AdvancedLogger` to runtime `Log` in all non-Editor assemblies.
- **M2.2** Add log level configuration hooks (Scriptable config or ProjectSettings) and ensure file logging path sanity.
- **M2.3** Add lightweight on-screen debug overlay toggleable in builds (uses `Log` categories, not Editor-only code).

### 3) IDE/Debug Tooling
- **M3.1** Keep `.vscode/launch.json` clean of deprecated fields; validate attach-to-Unity on Windows.
- **M3.2** Problems panel parity: ensure task/problem matchers fire from Unity builds; document quick checks.
- **M3.3** MCP/Unity Pro Tools: smoke-test attach + codegen flows; document known-good settings.

### 4) Testing & Quality Gates
- **M4.1** Add playmode tests for movement (done: movement reach/stop). Add edge cases (disabled, rapid retarget, zero speed).
- **M4.2** Add AI playmode tests for Pop AI basic loops (Idle→Wander→Gather) with a mock resource node.
- **M4.3** Wire test execution into CI (GitHub Actions/Unity Test Runner) targeting playmode suite.

### 5) Performance & Content Safety
- **M5.1** Micro-prof file: baseline frame time with 50/100 Pops using Simple vs Grid controllers.
- **M5.2** Memory safety checks around EntityDataComponent initialization; guard nulls/late init.
- **M5.3** Asset hygiene: ensure all new scripts reside under `Assets/_Project/Systems/[Feature]/Scripts` and tests under `Assets/_Project/Systems/[Feature]/Tests`.

## Short-Term Execution Plan (next 1–2 weeks)
1) **Logging cleanup (runtime assemblies)**: Replace remaining `AdvancedLogger` usages outside Editor with `Log` API; add category alias where needed.
2) **Movement tests expansion**: Add playmode tests for disabled/retarget/zero-speed and stopping-distance compliance.
3) **AI smoke tests**: Minimal scene-less AI test harness with mock resource node + IMovementController stub.
4) **Debug config verification**: Re-run attach to Unity using coreclr configs; remove any lingering deprecated fields.
5) **CI prep**: Add Unity Test Runner workflow (playmode only) with cache of Library artifacts to reduce time.

## Risks & Mitigations
- **Editor-only API leakage**: Continue scanning for `AdvancedLogger` in runtime assemblies; enforce via tests or analyzers.
- **Movement regressions**: Use playmode tests plus manual scenes to validate pathfinding vs simple movement; fall back to SimpleMovementController as default.
- **Integration drift**: Document Unity Pro Tools + MCP steps; keep `.vscode` configs minimal and current.

## Definition of Done for This Roadmap Pass
- Runtime code free of `AdvancedLogger`; all runtime logs go through `Log`.
- Movement controllers covered by playmode tests for reach/stop/edge cases; AI basic loop covered by at least one test.
- VS Code attach-to-Unity works with coreclr configs; no deprecated launch settings remain.
- CI workflow exists to run playmode movement/AI tests.
- Folder structure compliance maintained under `Assets/_Project/Systems/...` for scripts and tests.

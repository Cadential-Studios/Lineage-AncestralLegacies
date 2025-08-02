# EntityDataComponent & GameData System: Task List

_Last updated: 2025-08-02_

This task list covers improvements, refactors, and missing features identified in the `EntityDataComponent` and related GameData systems. It is intended for use by developers working on the core entity/component architecture in **Lineages: Ancestral Legacies**.

---

## Core Refactor & Code Quality
- [ ] Remove any unused or legacy fields from `EntityDataComponent` (e.g., duplicated stat fields vs. data-driven approach).
- [ ] Ensure all stat and state logic is fully data-driven via ScriptableObjects, not hardcoded fields.
- [ ] Add XML documentation for all public methods and properties (ensure consistency).
- [ ] Audit and enforce null checks for all public APIs and MonoBehaviour lifecycle methods.
- [ ] Ensure all debug output uses the custom `Lineage.Debug.Log` system (audit for any missed cases).
- [ ] Refactor event signatures to use `EventHandler`/`Action<T>` patterns for clarity and extensibility.
- [ ] Add `[SerializeField]` to all fields that should be visible in the inspector; remove from those that should not.
- [ ] Remove any accidental duplicate using directives (e.g., `using Lineage.Debug;` inside class).

## Testing & Validation
- [ ] Create comprehensive Edit Mode tests for all stat, state, and buff logic (see `RecipeDefinitionSOTests.cs` for style).
- [ ] Add Play Mode tests for integration with population, needs, and UI systems.
- [ ] Add tests for edge cases: null data, stat overflows, invalid state transitions, etc.

## Needs & Buffs System
- [ ] Refactor needs decay logic to be modular and data-driven (ScriptableObject config for rates).
- [ ] Add support for dynamic needs (e.g., new needs types, modifiable at runtime).
- [ ] Implement event-driven notifications for critical needs (e.g., OnCriticalNeeds).
- [ ] Add support for stacking, timed, and conditional buffs.
- [ ] Document and test all buff/needs interactions.

## Entity Lifecycle & State
- [ ] Implement robust age tracking and lifecycle transitions (birth, maturity, death).
- [ ] Add hooks for population manager to respond to entity state changes (e.g., death, reproduction).
- [ ] Ensure all state transitions are validated and logged.

## Data & Inspector Experience
- [ ] Improve inspector UI for `EntityDataComponent` (custom editor, grouping, tooltips).
- [ ] Add validation attributes or runtime checks for all serialized fields.
- [ ] Document all inspector-exposed fields for designers.

## Documentation & Onboarding
- [ ] Update or create a developer guide for working with `EntityDataComponent` and GameData systems.
- [ ] Add code comments and doc links for onboarding new contributors.
- [ ] Ensure all changes are reflected in the system map and onboarding checklist.

---

**Note:**
- This list should be reviewed and updated after each major refactor or feature addition.
- For cross-system tasks (e.g., population, UI), coordinate with relevant system owners.

---

_This file is auto-generated and should be maintained in `Documents/02_Tasks/EntityDataComponent_Tasks.md`._

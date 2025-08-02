# Technical Debt Backlog & Remediation Plan

This backlog surfaces and prioritizes areas of technical debt, with recommended incremental remediation steps.

---

## High-Priority Debt
- **Empty/Unpopulated Test Folders**
  - Populate `Testing/` and `Tests/` with actual tests for all core systems
  - Add scaffolding and examples for Edit/Play Mode

- **Implicit Coupling in Managers**
  - Refactor managers to depend on interfaces or events, not concrete types
  - Add tests to validate decoupling

- **ScriptableObject Reference Hygiene**
  - Audit all ScriptableObject references for null checks and error handling
  - Add validation scripts to catch missing references

---

## Medium-Priority Debt
- **Documentation Drift**
  - Regularly update onboarding, system, and style docs as code evolves
  - Add doc generation/check scripts

- **Manual Asset Linking**
  - Automate asset linking where possible (e.g., via editor scripts)

---

## Low-Priority Debt
- **Folder/Asset Naming Inconsistencies**
  - Standardize folder and asset names per style guide

- **Legacy/Unused Scripts**
  - Audit and archive or remove unused scripts and assets

---

## Remediation Plan
- Address high-priority items in the next sprint/iteration
- Use incremental, low-risk refactors with tests
- Track progress in this backlog and update regularly

---

For more, see the audit changelog and style guide.

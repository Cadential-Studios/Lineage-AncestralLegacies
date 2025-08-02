# Project Structure Proposal: Optimized & Developer-Friendly Layout

This document proposes a canonical, developer-optimized folder structure for the Lineages: Ancestral Legacies Unity project. It is based on a full audit of the current codebase and best practices for Unity, C#, and collaborative game development.

---

## Canonical Folder Structure

### Assets/
- **Scripts/**
  - **Core/**: Pure data, interfaces, and foundational systems (no Unity dependencies)
  - **Entities/**: Entity data, logic, and subfolders (e.g., Pop/)
  - **Systems/**: Modular gameplay systems (TraitSystem, Needs, Inventory, etc.)
  - **Managers/**: Game-wide managers (Audio, Camera, Population, Resource, Save, Selection, Time)
  - **AI/**: State machines and AI logic
  - **UI/**: UI logic, panels, and controls
  - **Debug/**: Debugging, logging, and profiling tools
  - **Tools/**: Editor and developer tools
  - **Components/**: MonoBehaviour components for attaching to GameObjects
  - **GameDatabase/**: GameDataManager, ScriptableObject definitions, and data loading
  - **Utils/**: General-purpose utilities
  - **Editor/**: Custom Unity editor scripts (LineageScripts.Editor)
  - **Testing/**, **Tests/**: Edit/Play Mode tests (should be populated and organized)
  - **Examples/**: Example scripts and usage patterns
  - **VisualScripting/**: Visual scripting assets

- **Logic/**: High-level gameplay logic, state machines, and feature-specific logic (LineageBehavior)
- **Data/**: ScriptableObjects for all static game data (entities, traits, recipes, etc.)
- **Prefabs/**: Prefabricated GameObjects, organized by feature
- **Scenes/**: Main and test scenes
- **Resources/**, **SFX/**, **Sprites/**, **StreamingAssets/**: Art, audio, and runtime assets

### Documents/
- **01_Design/**: GDD, system/lore specs, feature specs
- **02_Tasks/**: Task lists, roadmaps, optimization plans
- **03_Research/**: Reference material
- **04_Guides/**: Onboarding, setup, and workflow guides
- **05_Archive/**: Archived docs
- **06_ChangeLogs/**: Changelogs and audit logs

### IDE Commands/
- PowerShell scripts for build, test, code navigation, and validation

---

## Key Principles
- **Separation of Concerns**: Runtime, editor, test, and data assets are clearly separated
- **Consistent Naming**: Folders and files use PascalCase or camelCase as appropriate
- **Discoverability**: Feature folders and system boundaries are explicit
- **Test Placement**: Tests are placed in `Testing/` or `Tests/` and clearly labeled as Edit or Play Mode
- **Editor Tools**: All editor scripts are in `Editor/` and use the `LineageScripts.Editor` assembly

---

## Migration Guidance
- Move any misplaced scripts or assets to their canonical locations
- Populate `Testing/` and `Tests/` with actual test files, organized by system
- Ensure all ScriptableObjects are in `Data/` and referenced via GameDataManager
- Use `Editor/` only for editor tooling, not runtime code
- Update documentation and onboarding guides to reflect this structure

---

## Example Tree (Partial)

```
Assets/
  Scripts/
    Core/
    Entities/
      Pop/
    Systems/
      TraitSystem/
      Needs/
      Inventory/
    Managers/
    AI/
    UI/
    Debug/
    Tools/
    Components/
    GameDatabase/
      Core/
      Definitions/
    Utils/
    Editor/
    Testing/
    Tests/
    Examples/
    VisualScripting/
  Logic/
  Data/
  Prefabs/
  Scenes/
  Resources/
  SFX/
  Sprites/
  StreamingAssets/
```

---

This structure should be enforced and referenced in all future documentation, onboarding, and code reviews.

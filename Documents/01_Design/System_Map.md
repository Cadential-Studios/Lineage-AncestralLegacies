# System Map: Lineages Ancestral Legacies

This document enumerates the key systems, their responsibilities, and where related code lives. Use this as a developer map for onboarding and navigation.

---

## Core Systems & Locations

- **Entity System**
  - Location: `Assets/Scripts/Entities/`
  - Purpose: Defines all in-game entities, their data, and logic (see `Entity.cs`, `Pop/` for population logic)

- **Game Data System**
  - Location: `Assets/Scripts/GameDatabase/`
  - Purpose: Centralized static data management (see `GameDataManager.cs`, ScriptableObject definitions in `Core/` and `Definitions/`)

- **Trait System**
  - Location: `Assets/Scripts/Systems/TraitSystem/`
  - Purpose: Trait definitions, evaluation, and application (see `TraitSO.cs`)

- **Needs System**
  - Location: `Assets/Scripts/Systems/Needs/`
  - Purpose: Populates and manages entity needs (hunger, thirst, etc.)

- **Inventory & Resource Systems**
  - Location: `Assets/Scripts/Systems/Inventory/`, `Assets/Scripts/Systems/Resource/`
  - Purpose: Inventory management, resource tracking, and item logic

- **Managers**
  - Location: `Assets/Scripts/Managers/`
  - Purpose: Game-wide managers (Audio, Camera, Population, Resource, Save, Selection, Time)

- **AI & FSMs**
  - Location: `Assets/Scripts/AI/`, `Assets/Logic/Behavior/`
  - Purpose: State machines and AI logic for autonomous pop behavior

- **UI System**
  - Location: `Assets/Scripts/UI/`, `Assets/UI/`
  - Purpose: UI panels, controls, and HUD logic

- **Debug & Profiling**
  - Location: `Assets/Scripts/Debug/`
  - Purpose: Debugging, logging, and profiling tools

- **Editor Tools**
  - Location: `Assets/Scripts/Editor/`
  - Purpose: Custom Unity editor scripts and integration helpers

- **Utilities**
  - Location: `Assets/Scripts/Utils/`
  - Purpose: General-purpose utilities and helpers

---

## Data & Content
- **ScriptableObjects**: All static game data in `Assets/Data/`, referenced via GameDataManager
- **Prefabs**: Reusable GameObjects in `Assets/Prefabs/`
- **Scenes**: Main and test scenes in `Assets/Scenes/`

---

## Entry Points
- **Game Initialization**: `Assets/Scripts/GameInitializer.cs`
- **Main Scene**: `Assets/Scenes/World.unity`

---

## Cross-System Integration
- All systems interact via interfaces, events, or ScriptableObject data
- GameDataManager is the canonical source for static data
- Managers coordinate high-level game state and system orchestration

---

For more details, see the `Codebase Breakdown.md` and onboarding guides in `Documents/04_Guides/`.

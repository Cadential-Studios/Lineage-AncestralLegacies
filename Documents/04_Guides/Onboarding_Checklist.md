# Onboarding Checklist: Lineages Ancestral Legacies

Follow this checklist to get set up and productive in the project.

---

## 1. Prerequisites
- Install Unity (version in `ProjectSettings/ProjectVersion.txt`)
- Install .NET SDK (if using external tools)
- Clone the repository

## 2. Initial Setup
- Open the project in Unity Hub
- Run `.\IDE Commands\dev-hub.ps1` for interactive setup and status
- Review `Documents/04_Guides/` for guides on systems and workflows

## 3. Build & Run
- Use PowerShell scripts in `IDE Commands/` for build, test, and validation:
  - `.\dev-quick.ps1 build` — build project
  - `.\test-tools.ps1 compile` — check for errors
- Open `Assets/Scenes/World.unity` (or main scene) and press Play

## 4. Explore the Codebase
- Review `Documents/Codebase Breakdown.md` and `Documents/01_Design/System_Map.md`
- Browse `Assets/Scripts/` for core systems, managers, and features
- See `Assets/Data/` for ScriptableObjects

## 5. Making Changes
- Follow the style guide in `Documents/01_Design/Style_Guide.md`
- Place new scripts in the correct feature/system folder
- Add or update tests in `Testing/` or `Tests/`
- Document new systems in `Documents/`

## 6. Validation
- Run tests before committing
- Use `.\git-flow.ps1` for git workflow
- Check for errors and warnings in the Unity Console

---

For more, see the main README and guides in `Documents/04_Guides/`.

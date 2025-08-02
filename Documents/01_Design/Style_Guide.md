# Style Guide: Lineages Ancestral Legacies

This style guide codifies naming, organization, and code conventions for the project. All contributors should follow these rules for consistency and maintainability.

---

## Naming Conventions
- **Namespaces**: `Lineage.Core`, `Lineage.Behavior`, `Lineage.Editor`
- **Classes/Methods/Properties**: PascalCase
- **Private fields**: `_underscorePrefix`, `[SerializeField]` fields have no underscore
- **Events**: PascalCase, descriptive (e.g., `OnPlayerHealthChanged`)
- **Folders/Files**: PascalCase for folders, match class name for files
- **Tests**: `[ClassName]Tests.cs`, `[MethodUnderTest]_[Scenario]_[ExpectedResult]`

## File & Folder Organization
- Group scripts by feature and system (see Project Structure Proposal)
- Place interfaces and pure data in `Core/`
- Place MonoBehaviours in feature/system folders
- Place editor scripts in `Editor/` only
- Place tests in `Testing/` or `Tests/` and label as Edit/Play Mode

## Logging & Error Handling
- Use descriptive logging with class/component name and context
- Guard public APIs and MonoBehaviour startup logic with null checks
- Use try-catch for operations that can throw, log errors, and fail gracefully

## ScriptableObjects
- All static data (entities, traits, recipes, etc.) defined as ScriptableObjects in `Assets/Data/`
- Reference ScriptableObjects via GameDataManager

## Dependency Patterns
- Use interfaces, events, or ScriptableObjects for decoupling
- Avoid direct references between high-level systems

## Testing
- Use Unity Test Framework with NUnit
- Place tests in correct folder and namespace
- Provide commentary for each test

---

For more, see `.github/copilot-instructions.md` and onboarding guides in `Documents/04_Guides/`.

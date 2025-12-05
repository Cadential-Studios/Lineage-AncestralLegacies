# GameData Resources

This folder contains all ScriptableObject game data assets loaded by `GameDataManager`.

## Structure
- **Tags/** - Tag definitions for categorizing game data
- **Entities/** - Entity definitions (Pops, Animals, Structures)
- **Items/** - Item definitions (Resources, Tools, Consumables)
- **Recipes/** - Crafting recipe definitions

## Usage
1. In Unity Editor, go to **Lineage > GameData > Bootstrap Sample Data** to create sample assets
2. Use **Lineage > GameData > Validate GameData Assets** to check for issues
3. Access data via `GameDataManager.Instance.GetDefinition<T>(uniqueID)`

## Creating New Data
- Right-click in Project window > Create > GameData > [Type] Definition
- Set a unique `uniqueID` for each asset
- Place assets in the appropriate subfolder

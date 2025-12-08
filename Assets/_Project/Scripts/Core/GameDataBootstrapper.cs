using UnityEditor;
using UnityEngine;
using System.IO;
using Lineage.Core;
using Lineage.Core.Entities;
using Lineage.Core.Items;
using Lineage.Core.Crafting;

namespace Lineage.Editor.StudioTools
{
    /// <summary>
    /// Editor utility for bootstrapping initial GameData ScriptableObject assets.
    /// Creates sample Tags, Entities, Items, and Recipes in Resources/GameData.
    /// </summary>
    public static class GameDataBootstrapper
    {
        private const string GAMEDATA_PATH = "Assets/Resources/GameData";
        private const string TAGS_PATH = GAMEDATA_PATH + "/Tags";
        private const string ENTITIES_PATH = GAMEDATA_PATH + "/Entities";
        private const string ITEMS_PATH = GAMEDATA_PATH + "/Items";
        private const string RECIPES_PATH = GAMEDATA_PATH + "/Recipes";

        [MenuItem("Lineage/GameData/Bootstrap Sample Data", false, 100)]
        public static void BootstrapSampleData()
        {
            EnsureDirectories();
            CreateSampleTags();
            CreateSampleItems();
            CreateSampleEntities();
            CreateSampleRecipes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[GameDataBootstrapper] Sample GameData assets created successfully!");
        }

        [MenuItem("Lineage/GameData/Validate GameData Assets", false, 101)]
        public static void ValidateGameDataAssets()
        {
            var allData = Resources.LoadAll<GameDataSO>("GameData");
            int validCount = 0;
            int invalidCount = 0;

            foreach (var data in allData)
            {
                if (string.IsNullOrEmpty(data.uniqueID))
                {
                    UnityEngine.Debug.LogWarning($"[Validation] Missing uniqueID: {data.name} at {AssetDatabase.GetAssetPath(data)}");
                    invalidCount++;
                }
                else
                {
                    validCount++;
                }
            }

            var allTags = Resources.LoadAll<Tag_SO>("GameData");
            foreach (var tag in allTags)
            {
                if (string.IsNullOrEmpty(tag.tagName))
                {
                    UnityEngine.Debug.LogWarning($"[Validation] Missing tagName: {tag.name} at {AssetDatabase.GetAssetPath(tag)}");
                    invalidCount++;
                }
                else
                {
                    validCount++;
                }
            }

            UnityEngine.Debug.Log($"[GameDataBootstrapper] Validation complete: {validCount} valid, {invalidCount} invalid.");
        }

        [MenuItem("Lineage/GameData/Open GameData Folder", false, 200)]
        public static void OpenGameDataFolder()
        {
            EnsureDirectories();
            EditorUtility.RevealInFinder(GAMEDATA_PATH);
        }

        private static void EnsureDirectories()
        {
            CreateDirectoryIfMissing(GAMEDATA_PATH);
            CreateDirectoryIfMissing(TAGS_PATH);
            CreateDirectoryIfMissing(ENTITIES_PATH);
            CreateDirectoryIfMissing(ITEMS_PATH);
            CreateDirectoryIfMissing(RECIPES_PATH);
        }

        private static void CreateDirectoryIfMissing(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static void CreateSampleTags()
        {
            CreateTag("Tag_Resource", "Resource");
            CreateTag("Tag_Gatherable", "Gatherable");
            CreateTag("Tag_Food", "Food");
            CreateTag("Tag_Material", "Material");
            CreateTag("Tag_Tool", "Tool");
            CreateTag("Tag_Weapon", "Weapon");
            CreateTag("Tag_Consumable", "Consumable");
            CreateTag("Tag_Entity_Pop", "Pop");
            CreateTag("Tag_Entity_Animal", "Animal");
            CreateTag("Tag_Entity_Structure", "Structure");
            CreateTag("Tag_Carnivore", "Carnivore");
            CreateTag("Tag_Herbivore", "Herbivore");
        }

        private static Tag_SO CreateTag(string fileName, string tagName)
        {
            string path = $"{TAGS_PATH}/{fileName}.asset";
            
            Tag_SO existing = AssetDatabase.LoadAssetAtPath<Tag_SO>(path);
            if (existing != null) return existing;

            Tag_SO tag = ScriptableObject.CreateInstance<Tag_SO>();
            tag.tagName = tagName;
            AssetDatabase.CreateAsset(tag, path);
            return tag;
        }

        private static void CreateSampleItems()
        {
            Tag_SO resourceTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Resource.asset");
            Tag_SO foodTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Food.asset");
            Tag_SO materialTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Material.asset");
            Tag_SO toolTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Tool.asset");
            Tag_SO consumableTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Consumable.asset");

            // Resources
            CreateItem("Item_Wood", "ITEM_WOOD", "Wood", "Raw wood gathered from trees.", 
                ItemCategory.Resource, 99, false, resourceTag, materialTag);
            CreateItem("Item_Stone", "ITEM_STONE", "Stone", "Basic stone gathered from the ground.", 
                ItemCategory.Resource, 99, false, resourceTag, materialTag);
            CreateItem("Item_Flint", "ITEM_FLINT", "Flint", "Sharp flint used for crafting.", 
                ItemCategory.Material, 50, false, resourceTag, materialTag);

            // Food
            CreateItem("Item_Berries", "ITEM_BERRIES", "Berries", "Edible berries that restore hunger.", 
                ItemCategory.Consumable, 50, true, resourceTag, foodTag, consumableTag);
            CreateItem("Item_Meat", "ITEM_MEAT", "Raw Meat", "Raw meat from hunted animals.", 
                ItemCategory.Consumable, 20, true, resourceTag, foodTag);

            // Tools
            CreateItem("Item_StoneAxe", "ITEM_STONE_AXE", "Stone Axe", "A primitive axe for cutting wood.", 
                ItemCategory.Tool, 1, false, toolTag);
            CreateItem("Item_StonePickaxe", "ITEM_STONE_PICKAXE", "Stone Pickaxe", "A primitive pickaxe for mining.", 
                ItemCategory.Tool, 1, false, toolTag);
        }

        private static ItemDefinitionSO CreateItem(string fileName, string uniqueId, string displayName, 
            string description, ItemCategory category, int maxStack, bool isConsumable, params Tag_SO[] tags)
        {
            string path = $"{ITEMS_PATH}/{fileName}.asset";
            
            ItemDefinitionSO existing = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(path);
            if (existing != null) return existing;

            ItemDefinitionSO item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            item.uniqueID = uniqueId;
            item.displayName = displayName;
            item.description = description;
            item.category = category;
            item.maxStackSize = maxStack;
            item.isConsumable = isConsumable;
            item.isCraftingMaterial = category == ItemCategory.Material || category == ItemCategory.Resource;
            
            if (isConsumable && displayName.Contains("Berries"))
            {
                item.hungerRestore = 10f;
            }
            else if (isConsumable && displayName.Contains("Meat"))
            {
                item.hungerRestore = 25f;
            }

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (tag != null) item.tags.Add(tag);
                }
            }
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static void CreateSampleEntities()
        {
            Tag_SO popTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Entity_Pop.asset");
            Tag_SO animalTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Entity_Animal.asset");
            Tag_SO structureTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Entity_Structure.asset");
            Tag_SO carnivoreTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Carnivore.asset");
            Tag_SO herbivoreTag = AssetDatabase.LoadAssetAtPath<Tag_SO>($"{TAGS_PATH}/Tag_Herbivore.asset");

            // Pops
            CreateEntity("Entity_Pop_Generic", "ENTITY_POP_GENERIC", "Pop", 
                "A basic pop unit that can gather, craft, and build.", 
                EntityCategory.Pop, true, true, true, popTag);

            // Animals
            CreateEntity("Entity_Deer", "ENTITY_DEER", "Deer", 
                "A wild deer that can be hunted for meat.", 
                EntityCategory.Animal, false, false, false, animalTag, herbivoreTag);
            CreateEntity("Entity_Wolf", "ENTITY_WOLF", "Wolf", 
                "A predatory wolf that hunts prey.", 
                EntityCategory.Animal, false, false, true, animalTag, carnivoreTag);
            CreateEntity("Entity_Rabbit", "ENTITY_RABBIT", "Rabbit", 
                "A small rabbit, easy to catch.", 
                EntityCategory.Animal, false, false, false, animalTag, herbivoreTag);

            // Structures
            CreateEntity("Entity_Campfire", "ENTITY_CAMPFIRE", "Campfire", 
                "A basic campfire for cooking and warmth.", 
                EntityCategory.Structure, false, false, false, structureTag);
            CreateEntity("Entity_Shelter", "ENTITY_SHELTER", "Shelter", 
                "A basic shelter for rest and protection.", 
                EntityCategory.Structure, false, false, false, structureTag);
        }

        private static EntityDefinitionSO CreateEntity(string fileName, string uniqueId, string displayName, 
            string description, EntityCategory category, bool canCraft, bool canGather, bool canHunt, params Tag_SO[] tags)
        {
            string path = $"{ENTITIES_PATH}/{fileName}.asset";
            
            EntityDefinitionSO existing = AssetDatabase.LoadAssetAtPath<EntityDefinitionSO>(path);
            if (existing != null) return existing;

            EntityDefinitionSO entity = ScriptableObject.CreateInstance<EntityDefinitionSO>();
            entity.uniqueID = uniqueId;
            entity.displayName = displayName;
            entity.description = description;
            entity.entityCategory = category;
            entity.canCraft = canCraft;
            entity.canGather = canGather;
            entity.canHunt = canHunt;
            
            // Set category-specific defaults
            switch (category)
            {
                case EntityCategory.Pop:
                    entity.hasNeedsDecay = true;
                    entity.hasAging = true;
                    entity.canSocialize = true;
                    entity.baseHealth = 100f;
                    entity.baseSpeed = 5f;
                    break;
                case EntityCategory.Animal:
                    entity.hasNeedsDecay = true;
                    entity.hasAging = false;
                    entity.canFlee = true;
                    entity.baseHealth = 50f;
                    entity.baseSpeed = 6f;
                    if (canHunt)
                    {
                        entity.aggressionLevel = 70f;
                        entity.behaviorSubtags.Add("carnivore");
                    }
                    else
                    {
                        entity.aggressionLevel = 10f;
                        entity.behaviorSubtags.Add("herbivore");
                    }
                    break;
                case EntityCategory.Structure:
                    entity.hasNeedsDecay = false;
                    entity.hasAging = false;
                    entity.canFlee = false;
                    entity.baseHealth = 200f;
                    break;
            }

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (tag != null) entity.tags.Add(tag);
                }
            }
            AssetDatabase.CreateAsset(entity, path);
            return entity;
        }

        private static void CreateSampleRecipes()
        {
            ItemDefinitionSO wood = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>($"{ITEMS_PATH}/Item_Wood.asset");
            ItemDefinitionSO stone = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>($"{ITEMS_PATH}/Item_Stone.asset");
            ItemDefinitionSO flint = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>($"{ITEMS_PATH}/Item_Flint.asset");
            ItemDefinitionSO stoneAxe = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>($"{ITEMS_PATH}/Item_StoneAxe.asset");
            ItemDefinitionSO stonePickaxe = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>($"{ITEMS_PATH}/Item_StonePickaxe.asset");

            if (wood != null && stone != null && stoneAxe != null)
            {
                CreateRecipe("Recipe_StoneAxe", "RECIPE_STONE_AXE", "Craft Stone Axe", 
                    "Craft a stone axe using wood and stone.", stoneAxe, 1, 5f,
                    (wood, 2), (stone, 1));
            }

            if (wood != null && stone != null && stonePickaxe != null)
            {
                CreateRecipe("Recipe_StonePickaxe", "RECIPE_STONE_PICKAXE", "Craft Stone Pickaxe", 
                    "Craft a stone pickaxe using wood and stone.", stonePickaxe, 1, 5f,
                    (wood, 2), (stone, 2));
            }
        }

        private static RecipeDefinitionSO CreateRecipe(string fileName, string uniqueId, string displayName, 
            string description, ItemDefinitionSO output, int outputQty, float craftTime,
            params (ItemDefinitionSO item, int qty)[] ingredients)
        {
            string path = $"{RECIPES_PATH}/{fileName}.asset";
            
            RecipeDefinitionSO existing = AssetDatabase.LoadAssetAtPath<RecipeDefinitionSO>(path);
            if (existing != null) return existing;

            RecipeDefinitionSO recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
            recipe.uniqueID = uniqueId;
            recipe.displayName = displayName;
            recipe.description = description;
            recipe.outputItem = output;
            recipe.outputQuantity = outputQty;
            recipe.craftingTimeSeconds = craftTime;

            foreach (var (item, qty) in ingredients)
            {
                if (item != null)
                {
                    recipe.ingredients.Add(new RecipeDefinitionSO.Ingredient 
                    { 
                        itemDefinition = item, 
                        quantity = qty 
                    });
                }
            }

            AssetDatabase.CreateAsset(recipe, path);
            return recipe;
        }
    }
}
// Pseudocode generated by codewrx.ai

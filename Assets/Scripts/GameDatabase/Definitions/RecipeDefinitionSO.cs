using System.Collections.Generic;
using UnityEngine;
using Lineage.Core.Items;

namespace Lineage.Core.Crafting
{
    /// <summary>
    /// Definition for a crafting recipe.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipeDef", menuName = "GameData/Recipes/Recipe Definition")]
    /// <summary>
    /// ScriptableObject representing a crafting recipe definition.
    /// </summary>
    public class RecipeDefinitionSO : Lineage.Core.GameDataSO
    {
        /// <summary>
        /// Ingredient required for crafting. Each must reference a valid item and positive quantity.
        /// </summary>
        [System.Serializable]
        public class Ingredient
        {
            public ItemDefinitionSO itemDefinition;
            public int quantity;

            /// <summary>
            /// Validates the ingredient for nulls and positive quantity.
            /// </summary>
            public bool IsValid(out string error)
            {
                if (itemDefinition == null)
                {
                    error = "Ingredient itemDefinition is null.";
                    return false;
                }
                if (quantity <= 0)
                {
                    error = $"Ingredient quantity for {itemDefinition.name} is not positive.";
                    return false;
                }
                error = null;
                return true;
            }
        }

        [Tooltip("List of required ingredients for this recipe.")]
        public List<Ingredient> ingredients = new List<Ingredient>();

        [Tooltip("The item produced by this recipe.")]
        public ItemDefinitionSO outputItem;

        [Tooltip("How many output items are produced per craft.")]
        public int outputQuantity = 1;

        [Tooltip("Time in seconds required to craft this recipe.")]
        public float craftingTimeSeconds;

        /// <summary>
        /// Validates the recipe for missing/invalid data. Returns true if valid, false otherwise.
        /// </summary>
        public bool IsValid(out string error)
        {
            if (outputItem == null)
            {
                error = $"Recipe '{name}' outputItem is null.";
                return false;
            }
            if (outputQuantity <= 0)
            {
                error = $"Recipe '{name}' outputQuantity is not positive.";
                return false;
            }
            if (ingredients == null || ingredients.Count == 0)
            {
                error = $"Recipe '{name}' has no ingredients.";
                return false;
            }
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (!ingredients[i].IsValid(out error))
                {
                    error = $"Recipe '{name}' ingredient {i}: {error}";
                    return false;
                }
            }
            error = null;
            return true;
        }
    }
}

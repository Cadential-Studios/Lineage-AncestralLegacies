using NUnit.Framework;
using Lineage.Core.Crafting;
using Lineage.Core.Items;
using UnityEngine;

public class RecipeDefinitionSOTests
{
    [Test]
    public void IsValid_ValidRecipe_ReturnsTrue()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputItem = item;
        recipe.outputQuantity = 1;
        var ingredient = new RecipeDefinitionSO.Ingredient { itemDefinition = item, quantity = 2 };
        recipe.ingredients.Add(ingredient);
        string error;
        Assert.IsTrue(recipe.IsValid(out error), error);
    }

    [Test]
    public void IsValid_NullOutputItem_ReturnsFalse()
    {
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputQuantity = 1;
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        recipe.ingredients.Add(new RecipeDefinitionSO.Ingredient { itemDefinition = item, quantity = 1 });
        string error;
        Assert.IsFalse(recipe.IsValid(out error));
        Assert.IsTrue(error.Contains("outputItem"));
    }

    [Test]
    public void IsValid_ZeroOutputQuantity_ReturnsFalse()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputItem = item;
        recipe.outputQuantity = 0;
        recipe.ingredients.Add(new RecipeDefinitionSO.Ingredient { itemDefinition = item, quantity = 1 });
        string error;
        Assert.IsFalse(recipe.IsValid(out error));
        Assert.IsTrue(error.Contains("outputQuantity"));
    }

    [Test]
    public void IsValid_NoIngredients_ReturnsFalse()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputItem = item;
        recipe.outputQuantity = 1;
        string error;
        Assert.IsFalse(recipe.IsValid(out error));
        Assert.IsTrue(error.Contains("no ingredients"));
    }

    [Test]
    public void IsValid_IngredientNullItem_ReturnsFalse()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputItem = item;
        recipe.outputQuantity = 1;
        recipe.ingredients.Add(new RecipeDefinitionSO.Ingredient { itemDefinition = null, quantity = 1 });
        string error;
        Assert.IsFalse(recipe.IsValid(out error));
        Assert.IsTrue(error.Contains("itemDefinition is null"));
    }

    [Test]
    public void IsValid_IngredientZeroQuantity_ReturnsFalse()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
        var recipe = ScriptableObject.CreateInstance<RecipeDefinitionSO>();
        recipe.outputItem = item;
        recipe.outputQuantity = 1;
        recipe.ingredients.Add(new RecipeDefinitionSO.Ingredient { itemDefinition = item, quantity = 0 });
        string error;
        Assert.IsFalse(recipe.IsValid(out error));
        Assert.IsTrue(error.Contains("not positive"));
    }
}

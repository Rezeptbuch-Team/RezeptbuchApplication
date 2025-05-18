using ApplicationCore.Common.Types;

namespace ApplicationCore.Tests;

/// <summary>
/// a recipe should return a list of ingredients with their respective amount
/// </summary>
[TestFixture]
public class RecipeGetIngredientsTests
{
    public Recipe baseRecipe;
    [SetUp]
    public void Setup()
    {
        baseRecipe = new()
        {
            Hash = "asd",
            Title = "Pasta",
            ImagePath = "pasta.png",
            Description = "Simple pasta recipe.",
            Servings = 2,
            CookingTime = 20,
            Categories = ["Pasta", "Vegan"]
        };
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_SimpleIngredients()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_SimpleIngredients_IfThereIsText()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_DuplicateIngredients_WithDifferentUnits()
    {
        throw new NotImplementedException();
    }
    
    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_DuplicateIngredients_AndSumAmount_WhenSameUnits()
    {
        throw new NotImplementedException();
    }
}
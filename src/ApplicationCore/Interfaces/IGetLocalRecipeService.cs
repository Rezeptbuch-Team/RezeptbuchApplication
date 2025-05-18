using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

/// <summary>
/// Should return all data required to show the recipe to the user
/// basics:
///     - title
///     - cooking time
///     - image path
///     - description
///     - categories
///     - servings
/// advanced:
///     - ingredient list (changeable through servings)
///     - publish button (logic what button/info should be shown to the user)
///     ? recipe steps
/// 
/// (maybe implement the Recipe as a class that has methods that give back recipe step, ingredient list etc.)
/// </summary>
public interface IGetLocalRecipeService
{
    public Task<Recipe> GetRecipe(string hash);
}
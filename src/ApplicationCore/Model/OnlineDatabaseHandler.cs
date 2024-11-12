using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Model;

public class OnlineDatabaseHandler : IOnlineDatabaseHandler
{
    public List<RecipeEntry> GetOnlineRecipeList(Filter filter) {
        List<string> categories = ["category1", "category2"];
        RecipeEntry recipeEntry1 = new("hash", "title", "description", "imagePath", categories, 15);
        RecipeEntry recipeEntry2 = new("hash2", "title2", "description2", "imagePath2", categories, 30);
        return [recipeEntry1, recipeEntry2];
    }
}

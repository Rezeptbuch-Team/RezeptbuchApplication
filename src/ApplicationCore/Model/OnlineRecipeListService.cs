using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Model;

public class OnlineRecipeListService : IOnlineRecipeListService
{
    public List<RecipeEntry> GetOnlineRecipeList(Filter filter) {
        return new List<RecipeEntry>();
    }
}

using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Model;

public class OnlineRecipeListService(IOnlineDatabaseHandler onlineDatabaseHandler) : IOnlineRecipeListService
{
    public List<RecipeEntry> GetOnlineRecipeList(Filter filter) {
        return onlineDatabaseHandler.GetOnlineRecipeList(filter);
    }
}

using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineAPIHandler {
    public Task<List<RecipeEntry>> GetOnlineRecipeList(Filter filter);
}
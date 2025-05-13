using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineRecipeListService {
    public string BuildListUrl(Filter filter);
    public Task<List<RecipeEntry>> GetOnlineRecipeList(Filter filter);
}
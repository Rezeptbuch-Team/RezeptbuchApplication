using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineRecipeListService {
    public List<RecipeEntry> GetOnlineRecipeList(Filter filter);
}
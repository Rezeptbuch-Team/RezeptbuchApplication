using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineDatabaseHandler {
    public List<RecipeEntry> GetOnlineRecipeList(Filter filter);
}
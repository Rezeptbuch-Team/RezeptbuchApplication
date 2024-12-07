using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface ILocalRecipeListService {
    public Task<List<RecipeEntry>> GetLocalRecipeList(Filter filter);
}
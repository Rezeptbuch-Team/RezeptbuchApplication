using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IRecipeListService {
    public Task<List<RecipeEntry>> GetRecipeList(Filter filter);
}
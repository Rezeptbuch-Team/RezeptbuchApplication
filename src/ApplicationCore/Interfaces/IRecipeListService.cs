using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IRecipeListService {
    public Task<List<RecipeEntry>> GetRecipeList(Filter filter);

    public Task<List<FilterOption>> GetCategories(FilterOptionOrderBy orderBy, Order order, int limit, int offset);
}
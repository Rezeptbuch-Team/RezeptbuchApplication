using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface ILocalRecipeListService {
    public Task<List<RecipeEntry>> GetLocalRecipeList(Filter filter);

    public Task<List<Category>> GetCategories(CategoryOrderBy orderBy, Order order, int limit, int offset);
}
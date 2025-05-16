using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface ILocalRecipeListService : IRecipeListService {
    public Task<List<FilterOption>> GetIngredients(FilterOptionOrderBy orderBy, Order order, int limit, int offset);
}
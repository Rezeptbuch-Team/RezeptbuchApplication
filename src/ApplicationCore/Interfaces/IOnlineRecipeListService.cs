using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineRecipeListService : IRecipeListService {
    public string BuildListUrl(Filter filter);
}
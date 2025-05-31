using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IOnlineRecipeListService : IRecipeListService {
    public string BuildListUrl(Filter filter);
    public Task<List<string>> GetCategories(int limit, int offset);

    public Task DownloadImage(string hash, string filePath);
}
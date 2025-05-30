using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IDownloadRecipeService
{
    public Task DownloadRecipe(string hash);
    public Task DownloadImage(string hash, string filePath);
    public Task AddRecipeToDatabase(Recipe recipe, string recipeFilePath);
    public Task<bool> IsHashInDatabase(string hash);
}
using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IGetRecipeFromFileService
{
    public Task<Recipe> GetRecipeFromFile(string filePath);
}
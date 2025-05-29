using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface IGetRecipeFromFileService
{
    public Recipe GetRecipeFromFile(string filePath);
}
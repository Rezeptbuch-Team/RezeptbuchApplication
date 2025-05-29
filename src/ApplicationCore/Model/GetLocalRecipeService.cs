using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class GetLocalRecipeService(IDatabaseService databaseService, IOnlineIdentificationService onlineIdentificationService, IGetRecipeFromFileService getRecipeFromFileService) : IGetLocalRecipeService
{

    public async Task<Recipe> GetRecipe(string hash)
    {
        #region request filepath and publishinformation from database
        string sql = @"SELECT file_path, is_download, is_published, is_modified
                        FROM recipes
                        WHERE hash = $hash;";

        Dictionary<string, object> parameters = new() {
            { "$hash", hash }
        };
        string filePath;
        PublishOption publishOption;
        string? uuid = await onlineIdentificationService.GetUUID();
        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await resultReader.ReadAsync())
            {
                try
                {
                    filePath = resultReader.GetString(0);
                    bool is_download = resultReader.GetInt32(1) > 0;
                    bool is_published = resultReader.GetInt32(2) > 0;
                    bool is_modified = resultReader.GetInt32(3) > 0;

                    if (is_download || uuid == null) publishOption = PublishOption.FORBIDDEN;
                    else if (!is_published) publishOption = PublishOption.NOT_PUBLISHED;
                    else if (is_published && is_modified) publishOption = PublishOption.OUTDATED;
                    else publishOption = PublishOption.PUBLISHED;
                }
                catch (InvalidCastException)
                {
                    throw new Exception("Failed to fetch file path for hash");
                }
            }
            else
            {
                throw new Exception("Recipe not found");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Failed to fetch file path for hash");
            }
        }
        #endregion

        Recipe recipe = getRecipeFromFileService.GetRecipeFromFile(filePath);
        recipe.PublishOption = publishOption;

        return recipe;
    }
}
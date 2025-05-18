using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class GetLocalRecipeService(IDatabaseService databaseService) : IGetLocalRecipeService
{
    public async Task<Recipe> GetRecipe(string hash)
    {
        #region request filepath from database
        string sql = @"SELECT file_path
                        FROM recipes
                        WHERE hash = $hash;";
                        
        Dictionary<string, object> parameters = new() {
            { "$hash", hash }
        };
        string filePath;
        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters))
        {
            if (await resultReader.ReadAsync())
            {
                try
                {
                    filePath = resultReader.GetString(0);
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

        return new Recipe();
    }
}
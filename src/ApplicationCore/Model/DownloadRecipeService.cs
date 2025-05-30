using System.Data.Common;
using System.Reflection.Metadata;
using System.Xml.Linq;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class DownloadRecipeService(HttpClient httpClient, IGetRecipeFromFileService getRecipeFromFileService, IDatabaseService databaseService, string appDataPath) : IDownloadRecipeService
{
    public async Task DownloadImage(string hash, string filePath)
    {
        string imageUrl = "/images/" + hash;
        HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

        response.EnsureSuccessStatusCode();

        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }

    public async Task<bool> IsHashInDatabase(string hash)
    {
        string sql = @"SELECT hash
                        FROM recipes
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new()
        {
            { "$hash", hash }
        };
        await using (DbDataReader reader = await databaseService.QueryAsync(sql, parameters))
        {
            return reader.HasRows;
        }
    }

    public async Task AddRecipeToDatabase(Recipe recipe, string recipeFilePath)
    {
        #region insert recipe
        Dictionary<string, object> insertRecipeParameters = new()
        {
            { "$hash", recipe.Hash },
            { "$title", recipe.Title },
            { "$description", recipe.Description },
            { "$image_path", recipe.ImagePath },
            { "$cooking_time", recipe.CookingTime },
            { "$file_path", recipeFilePath }
        };
        string insertRecipeSql = @"INSERT INTO recipes
                                    VALUES ($hash, 0, 0, 0, NULL, $title, $description, $image_path, $cooking_time, $file_path);";
        await databaseService.NonQueryAsync(insertRecipeSql, insertRecipeParameters);
        #endregion

        #region insert categories
        foreach (string categoryName in recipe.Categories)
        {
            Dictionary<string, object> parameters = new()
            {
                { "$name", categoryName }
            };
            string insertCategorySql = @"INSERT OR IGNORE INTO categories(name)
                                            VALUES($name);";
            await databaseService.NonQueryAsync(insertCategorySql, parameters);
            string getCategoryIdSql = @"SELECT id FROM categories
                                        WHERE name = $name;";
            long? categoryId = null;
            await using (DbDataReader reader = await databaseService.QueryAsync(getCategoryIdSql, parameters))
            {
                if (!await reader.ReadAsync()) continue;
                categoryId = reader.GetInt64(0);
            }
            if (categoryId == null) continue;
            string insertRecipeCategorySql = @"INSERT OR IGNORE INTO recipe_category(hash, category_id)
                                                VALUES ($hash, $catId);";
            Dictionary<string, object> insertRecipeCategoryParameters = new()
            {
                { "$hash", recipe.Hash },
                { "$catId", categoryId }
            };
            await databaseService.NonQueryAsync(insertRecipeCategorySql, insertRecipeCategoryParameters);
        }
        #endregion

        #region insert ingredients
        foreach (Ingredient ingredient in recipe.GetIngredients())
        {
            Dictionary<string, object> parameters = new()
            {
                { "$name", ingredient.Name }
            };
            string insertIngredientSql = @"INSERT OR IGNORE INTO ingredients(name)
                                            VALUES($name);";
            await databaseService.NonQueryAsync(insertIngredientSql, parameters);
            string getIngredientIdSql = @"SELECT id FROM ingredients
                                        WHERE name = $name;";
            long? ingId = null;
            await using (DbDataReader reader = await databaseService.QueryAsync(getIngredientIdSql, parameters))
            {
                if (!await reader.ReadAsync()) continue;
                ingId = reader.GetInt64(0);
            }
            if (ingId == null) continue;
            string insertRecipeIngredientSql = @"INSERT OR IGNORE INTO recipe_ingredient(hash, ingredient_id)
                                                VALUES ($hash, $ingId);";
            Dictionary<string, object> insertRecipeIngredientParameters = new()
            {
                { "$hash", recipe.Hash },
                { "$ingId", ingId }
            };
            await databaseService.NonQueryAsync(insertRecipeIngredientSql, insertRecipeIngredientParameters);
        }
        #endregion
    }

    public async Task DownloadRecipe(string hash)
    {
        if (await IsHashInDatabase(hash)) return;
        #region download recipe into appdata dir
        string url = $"recipes/{hash}";

        string recipeFilePath = Path.Combine(appDataPath, $"{hash}.xml");

        {
            await using var xmlStream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
            await using var fileStream = File.Create(recipeFilePath);
            await xmlStream.CopyToAsync(fileStream);
        }
        #endregion

        Recipe recipe = getRecipeFromFileService.GetRecipeFromFile(recipeFilePath);

        #region download image if needed
        // check if it exists because it has been downloaded for the online recipe list
        string imageFilePath = Path.Combine(appDataPath, $"{hash}.png");
        string newImageFilePath = Path.Combine(appDataPath, recipe.ImagePath);
        if (File.Exists(imageFilePath))
        {
            if (!File.Exists(newImageFilePath)) File.Move(imageFilePath, newImageFilePath);
            else File.Delete(imageFilePath);
        }

        recipe.ImagePath = newImageFilePath;

        if (!File.Exists(newImageFilePath))
        {
            try
            {
                await DownloadImage(hash, newImageFilePath);
            }
            catch
            {
                // its okay if there is no image
                recipe.ImagePath = "";
            }
        }
        #endregion

        #region  add recipe to database
        await AddRecipeToDatabase(recipe, recipeFilePath);
        #endregion
    }
}
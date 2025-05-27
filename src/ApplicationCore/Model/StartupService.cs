using System.Data.Common;
using System.Xml.Linq;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class StartupService(IDatabaseService databaseService, IGetRecipeFromFileService getRecipeFromFileService)
{
    #region Helper functions
    private async Task UpdateDatabaseWithModifiedHash(string hash, string updatedHash)
    {
        string sql = @"UPDATE recipes
                        SET hash = $new_hash, is_modified = 1
                        WHERE hash = $old_hash;
                        UPDATE recipe_category
                        SET hash = $new_hash
                        WHERE hash = $old_hash;
                        UPDATE recipe_ingredient
                        SET hash = $new_hash
                        WHERE hash = $old_hash;";
        Dictionary<string, object> parameters = new(){
            { "$new_hash", updatedHash },
            { "$old_hash", hash }
        };
        await databaseService.NonQueryAsync(sql, parameters);
    }

    private async Task UpdateDatabaseWithModifiedFilePath(string hash, string filePath)
    {
        string sql = @"UPDATE recipes
                        SET file_path = $file_path
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new(){
            { "$hash", hash },
            { "$file_path", filePath }
        };
        await databaseService.NonQueryAsync(sql, parameters);
    }

    private async Task CheckForConflictInFile(string filePath, string databaseHash)
    {
        try
        {
            Recipe recipe = await getRecipeFromFileService.GetRecipeFromFile(filePath);
            string calculatedHash = recipe.CalculateHash();
            // if hash of recipe does not fit entry in database
            if (calculatedHash != databaseHash)
            {
                await UpdateDatabaseWithModifiedHash(databaseHash, calculatedHash);

                #region update <hash> in file
                // doc.Root and hashElem will not be null, because that was just checked by the GetRecipeFromFileService
                XDocument doc = XDocument.Load(filePath);
                XElement hashElem = doc.Root!.Element("hash")!;
                hashElem.Value = calculatedHash;
                doc.Save(filePath);
                #endregion
            }
        }
        catch
        {
            // we do not care if there are errors during this
        }
    }

    private static IEnumerable<string> XmlFilesInDirectory()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        foreach (string filePath in Directory.EnumerateFiles(appDataPath, "*.xml", SearchOption.AllDirectories))
        {
            yield return filePath;
        }
    }

    private static string? FindFileWithHash(string hash)
    {
        foreach (string filePath in XmlFilesInDirectory())
        {
            XDocument doc = XDocument.Load(filePath);
            XElement? hashElem = doc.Root?.Element("hash");
            if (hashElem != null && hashElem.Value == hash)
            {
                return filePath;
            }
        }
        return null;
    }

    private async Task DeleteEntryFromDatabase(string hash)
    {
        string sql = @"DELETE FROM recipes
                        WHERE hash = $hash;";
        Dictionary<string, object> parameters = new()
        {
            { "$hash", hash }
        };

        await databaseService.NonQueryAsync(sql, parameters);
    }

    private async Task<bool> IsHashInDatabase(string hash)
    {
        string sql = @"SELECT COUNT(*)
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

    private async Task AddRecipeToDatabase(Recipe recipe, string filePath)
    {
        #region insert recipe
        Dictionary<string, object> insertRecipeParameters = new()
        {
            { "$hash", recipe.Hash },
            { "$title", recipe.Title },
            { "$description", recipe.Description },
            { "$image_path", recipe.ImagePath },
            { "$cooking_time", recipe.CookingTime },
            { "$file_path", filePath }
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
            await using (DbDataReader reader = await databaseService.QueryAsync(getCategoryIdSql, parameters))
            {
                if (await reader.ReadAsync())
                {
                    int? categoryId = reader.GetValue(0) as int?;
                    if (categoryId != null)
                    {
                        string insertRecipeCategorySql = @"INSERT OR IGNORE INTO recipe_category(hash, category_id)
                                                            VALUES ($hash, $catId);";
                        Dictionary<string, object> insertRecipeCategoryParameters = new()
                        {
                            { "$hash", recipe.Hash },
                            { "$catId", categoryId }
                        };
                        await databaseService.NonQueryAsync(insertRecipeCategorySql, insertRecipeCategoryParameters);
                    }
                }
            }
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
            await using (DbDataReader reader = await databaseService.QueryAsync(getIngredientIdSql, parameters))
            {
                if (await reader.ReadAsync())
                {
                    int? ingId = reader.GetValue(0) as int?;
                    if (ingId != null)
                    {
                        string insertRecipeIngredientSql = @"INSERT OR IGNORE INTO recipe_ingredient(hash, ingredient_id)
                                                            VALUES ($hash, $ingId);";
                        Dictionary<string, object> insertRecipeIngredientParameters = new()
                        {
                            { "$hash", recipe.Hash },
                            { "$ingId", ingId }
                        };
                        await databaseService.NonQueryAsync(insertRecipeIngredientSql, insertRecipeIngredientParameters);
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    public static void CreateAppDataFolder()
    {
        string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rezeptbuch");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
    }

    public async Task CheckForConflicts()
    {
        #region get all recipe file paths, hash
        string sql = "SELECT file_path, hash FROM recipes;";
        Dictionary<string, string> filePaths = [];

        await using (DbDataReader resultReader = await databaseService.QueryAsync(sql))
        {
            string? filePath;
            string? hash;
            if (await resultReader.ReadAsync())
            {
                filePath = resultReader.GetValue(0) as string;
                hash = resultReader.GetValue(1) as string;
                if (!string.IsNullOrWhiteSpace(filePath) && !string.IsNullOrWhiteSpace(hash))
                {
                    filePaths.Add(filePath, hash);
                }
            }
        }
        #endregion

        foreach (KeyValuePair<string, string> recipeEntry in filePaths)
        {
            if (File.Exists(recipeEntry.Key))
            {
                await CheckForConflictInFile(recipeEntry.Key, recipeEntry.Value);
            }
            else
            {
                string? foundFile = FindFileWithHash(recipeEntry.Value);
                if (foundFile != null)
                {
                    await UpdateDatabaseWithModifiedFilePath(recipeEntry.Value, foundFile);

                    await CheckForConflictInFile(foundFile, recipeEntry.Value);
                }
                else
                {
                    await DeleteEntryFromDatabase(recipeEntry.Value);
                }
            }
        }
    }

    public async Task CheckForOrphanedFiles()
    {
        foreach (string filePath in XmlFilesInDirectory())
        {
            try
            {
                Recipe recipe = await getRecipeFromFileService.GetRecipeFromFile(filePath);
                if (!await IsHashInDatabase(recipe.Hash))
                {
                    await AddRecipeToDatabase(recipe, filePath);
                }
            }
            catch
            {
                // probably means that there is an error loading the recipe which we do not care about
            }
        }
    }
}
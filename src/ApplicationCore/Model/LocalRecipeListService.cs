using System.Data.Common;
using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class LocalRecipeListService(IDatabaseService databaseService) : ILocalRecipeListService
{
    public async Task<List<RecipeEntry>> GetLocalRecipeList(Filter filter) {

        List<RecipeEntry> recipes = [];

        #region create the SQL query and parameters
        Dictionary<string, object> parameters = new() {
            { "$limit", filter.count },
            { "$offset", filter.offset }
        };

        string sql = @"SELECT r.hash AS hash, r.title AS title, 
                                r.description AS description, 
                                r.image_path AS image_path,
                                r.cooking_time AS cooking_time,
                                c.name AS category
                        FROM recipes r 
                        JOIN recipe_category rc ON r.hash = rc.recipe_hash 
                        JOIN categories c ON rc.category_id = c.id ";

        if (filter.categories.Count > 0) {
            sql += "WHERE c.name IN (";
            // add $cat1, $cat2, ... to the sql query
            for (int i = 0; i < filter.categories.Count; i++) {
                sql += $"$cat{i + 1}";
                if (i < filter.categories.Count - 1) {
                    sql += ", ";
                }

                parameters.Add($"$cat{i + 1}", filter.categories[i]);
            }   
            sql += ") ";
        }
        sql += @"ORDER BY r.title ASC
                LIMIT $limit 
                OFFSET $offset";
        #endregion
        
        #region execute the query and get the recipes
        using DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters);
        while (await resultReader.ReadAsync()) {
            string hash = resultReader.GetString(0);
            string title = resultReader.GetString(1);
            string description = resultReader.GetString(2);
            string imagePath = resultReader.GetString(3);
            int cookingTime = resultReader.GetInt32(4);
            string category = resultReader.GetString(5);

            try {
                // find the recipe by hash
                RecipeEntry existingRecipe = recipes.First(r => r.hash == hash);
                existingRecipe.categories.Add(category);
            } catch (InvalidOperationException) {
                // recipe not found, create a new recipe entry
                RecipeEntry recipeEntry = new(hash, title, description, imagePath, [category], cookingTime);
                recipes.Add(recipeEntry);
            }
        }
        #endregion

        return recipes;
    }
}

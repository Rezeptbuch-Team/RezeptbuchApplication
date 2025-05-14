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

        string sql = @"SELECT DISTINCT r.hash AS hash, r.title AS title, 
                                r.description AS description, 
                                r.image_path AS image_path,
                                r.cooking_time AS cooking_time,
                                c.name AS category
                        FROM recipes r 
                        JOIN recipe_category rc ON r.hash = rc.hash 
                        JOIN categories c ON rc.category_id = c.id ";
        if (filter.availableIngredients.Count > 0) {
            sql += "JOIN recipe_ingredient ri ON r.hash = ri.hash ";
            sql += "JOIN ingredients i ON ri.ingredient_id = i.id ";
        }

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
        if (filter.availableIngredients.Count > 0) {
            if (filter.categories.Count > 0) {
                sql += "AND ";
            } else {
                sql += "WHERE ";
            }
            sql += "i.name IN (";
            // add $ing1, $ing2, ... to the sql query
            for (int i = 0; i < filter.availableIngredients.Count; i++) {
                sql += $"$ing{i + 1}";
                if (i < filter.availableIngredients.Count - 1) {
                    sql += ", ";
                }

                parameters.Add($"$ing{i + 1}", filter.availableIngredients[i]);
            }   
            sql += ") ";
        }
        sql += "ORDER BY " + (filter.orderBy == OrderBy.TITLE ? "title " : "cooking_time ");
        sql += filter.order == Order.DESCENDING ? "DESC " : "ASC ";
        sql += @"LIMIT $limit 
                OFFSET $offset;";
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

    public async Task<List<Category>> GetCategories(CategoryOrderBy orderBy = CategoryOrderBy.TITLE, Order order = Order.DESCENDING, int limit = 100, int offset = 0) {
        List<Category> categories = [];

        #region create the SQL query and parameters
        Dictionary<string, object> parameters = new() {
            { "$limit", limit },
            { "$offset", offset }
        };
        string sql = @"SELECT c.name AS name, COUNT(rc.hash) AS count
                        FROM categories c
                        JOIN recipe_category rc ON c.id = rc.category_id
                        GROUP BY c.name ";
        sql += "ORDER BY " + (orderBy == CategoryOrderBy.TITLE ? "name " : "count ");
        sql += order == Order.DESCENDING ? "DESC " : "ASC ";
        sql += @"LIMIT $limit 
                OFFSET $offset;";
        #endregion

        #region execute the query and get the categories#
        //throw new Exception(sql);
        using DbDataReader resultReader = await databaseService.QueryAsync(sql, parameters);
        //throw new Exception(sql);
        while (await resultReader.ReadAsync()) {
            string name = resultReader.GetString(0);
            int count = resultReader.GetInt32(1);

            Category category = new(name, count);
            categories.Add(category);
        }
        #endregion

        return categories;
    }
}

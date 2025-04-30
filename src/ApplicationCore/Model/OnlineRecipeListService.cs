using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;
using System.Text.Json;

namespace ApplicationCore.Model;

// classes for json deserialization
public class Recipe
    {
        public required string hash { get; set; }
        public required string title { get; set; }
        public required string description { get; set; }
        public required string image_path { get; set; }
        public required List<string> categories { get; set; }
        public int cooking_time { get; set; }
    }

public class Root
{
    public required List<Recipe> recipes { get; set; }
}

public class OnlineRecipeListService(HttpClient httpClient) : IOnlineRecipeListService
{
    public string BuildUrl(Filter filter) {
        string url = "?";
        url += "count=" + filter.count.ToString() + "&";
        url += "offset=" + filter.offset.ToString();
        if (filter.orderBy == OrderBy.COOKINGTIME) {
            url += "&order_by=cooking_time&";
        }
        if (filter.order == Order.DESCENDING) {
            url += "&order=desc";
        }
        if (filter.categories.Count > 0) {
            url += "&categories=";
            for (int i = 0; i < filter.categories.Count; i++) {
                url += filter.categories[i];
                if (i < filter.categories.Count - 1) {
                    url += ",";
                }
            }
        }
        return url;
    }

    public async Task<List<RecipeEntry>> GetOnlineRecipeList(Filter filter) {
        string url = BuildUrl(filter);

        List<RecipeEntry> recipes = [];
        try {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                Root extractedRoot = JsonSerializer.Deserialize<Root>(json)!;
                
                foreach (Recipe recipeEntry in extractedRoot.recipes) {
                    recipes.Add(new RecipeEntry(recipeEntry.hash,
                    recipeEntry.title, recipeEntry.description, recipeEntry.image_path, recipeEntry.categories,
                    recipeEntry.cooking_time));
                }
            } else {
                throw new Exception("Request error");
            }
        } catch (HttpRequestException) {
            throw new Exception("API unrechable");
        }
        
        // download images and change the image path afterwards
        return recipes;
    }
}

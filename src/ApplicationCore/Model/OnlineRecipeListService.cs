using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationCore.Model;

// classes for json deserialization
public class Recipe
    {
        [JsonPropertyName("hash")]
        public required string Hash { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("description")]
        public required string Description { get; set; }
        [JsonPropertyName("image_path")]
        public required string ImagePath { get; set; }
        [JsonPropertyName("categories")]
        public required List<string> Categories { get; set; }
        [JsonPropertyName("cooking_time")]
        public int CookingTime { get; set; }
    }

public class Root
{
    [JsonPropertyName("recipes")]
    public required List<Recipe> Recipes { get; set; }
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

        List<RecipeEntry> recipesEntries = [];
        #region API-Request
        try {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                Root extractedRoot = JsonSerializer.Deserialize<Root>(json)!;

                foreach (Recipe recipe in extractedRoot.Recipes) {
                    recipesEntries.Add(new RecipeEntry(recipe.Hash,
                    recipe.Title, recipe.Description, recipe.ImagePath, recipe.Categories,
                    recipe.CookingTime));
                }
            } else {
                throw new Exception("Response error");
            }
        } catch (HttpRequestException) {
            throw new Exception("API unreachable");
        }
        #endregion
        
        // download images and change the image path afterwards

        return recipesEntries;
    }
}

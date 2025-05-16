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
    public string BuildListUrl(Filter filter)
    {
        string url = "/list?";
        url += "count=" + filter.count.ToString() + "&";
        url += "offset=" + filter.offset.ToString();
        if (filter.orderBy == OrderBy.COOKINGTIME)
        {
            url += "&order_by=cooking_time&";
        }
        if (filter.order == Order.DESCENDING)
        {
            url += "&order=desc";
        }
        if (filter.categories.Count > 0)
        {
            url += "&categories=";
            for (int i = 0; i < filter.categories.Count; i++)
            {
                url += filter.categories[i];
                if (i < filter.categories.Count - 1)
                {
                    url += ",";
                }
            }
        }
        return url;
    }

    public async Task<List<RecipeEntry>> GetRecipeList(Filter filter)
    {
        string listUrl = BuildListUrl(filter);

        List<RecipeEntry> recipesEntries = [];
        #region API-Request
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(listUrl);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Root extractedRoot = JsonSerializer.Deserialize<Root>(json)!;

                foreach (Recipe recipe in extractedRoot.Recipes)
                {
                    string imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), recipe.Hash + ".png");
                    recipesEntries.Add(new RecipeEntry(recipe.Hash,
                    recipe.Title, recipe.Description, imagePath, recipe.Categories,
                    recipe.CookingTime));
                }
            }
            else
            {
                throw new Exception("Response error. Status code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException)
        {
            throw new Exception("API unreachable");
        }
        #endregion

        #region download images
        if (recipesEntries.Count > 0)
        {
            foreach (RecipeEntry recipeEntry in recipesEntries)
            {
                string imageUrl = "/images/" + recipeEntry.hash + ".png";
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        using FileStream fileStream = new(recipeEntry.imagePath, FileMode.Create, FileAccess.Write, FileShare.None);
                        await response.Content.CopyToAsync(fileStream);
                    }
                    else
                    {
                        throw new Exception("Image download error. Status code: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException)
                {
                    throw new Exception("API unreachable");
                }
            }
        }
        #endregion

        return recipesEntries;
    }

    public async Task<List<FilterOption>> GetCategories(FilterOptionOrderBy orderBy, Order order, int limit, int offset)
    {
        throw new NotImplementedException("GetCategories is not implemented in OnlineRecipeListService.");
    }
}

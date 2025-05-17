using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApplicationCore.Model;

// classes for json deserialization
public class JsonRecipe
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

public class JsonRoot
{
    [JsonPropertyName("recipes")]
    public required List<JsonRecipe> Recipes { get; set; }
}

public class JsonCategories
{
    [JsonPropertyName("categories")]
    public required List<string> Categories { get; set; }
}

public class OnlineRecipeListService(HttpClient httpClient) : IOnlineRecipeListService
{
    public string BuildListUrl(Filter filter)
    {
        string url = "/list?";
        url += "count=" + filter.Count.ToString() + "&";
        url += "offset=" + filter.Offset.ToString();
        if (filter.OrderBy == OrderBy.COOKINGTIME)
        {
            url += "&order_by=cooking_time&";
        }
        if (filter.Order == Order.DESCENDING)
        {
            url += "&order=desc";
        }
        if (filter.Categories.Count > 0)
        {
            url += "&categories=";
            for (int i = 0; i < filter.Categories.Count; i++)
            {
                url += filter.Categories[i];
                if (i < filter.Categories.Count - 1)
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
                JsonRoot extractedRoot = JsonSerializer.Deserialize<JsonRoot>(json)!;

                foreach (JsonRecipe recipe in extractedRoot.Recipes)
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
                string imageUrl = "/images/" + recipeEntry.Hash + ".png";
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        using FileStream fileStream = new(recipeEntry.ImagePath, FileMode.Create, FileAccess.Write, FileShare.None);
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

    public async Task<List<string>> GetCategories(int limit, int offset)
    {
        string url = "/categories?count=" + limit.ToString() + "&offset=" + offset.ToString();

        #region API-Request
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                JsonCategories extractedCategories = JsonSerializer.Deserialize<JsonCategories>(json)!;

                return extractedCategories.Categories;
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
    }
}

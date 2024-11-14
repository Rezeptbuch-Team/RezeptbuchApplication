using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Model;

public class OnlineAPIHandler() : IOnlineAPIHandler
{

    public async Task<List<RecipeEntry>> GetOnlineRecipeList(Filter filter) {
        string baseUrl = "localhost";  // replace with configuration later on
        string endpointUrl = baseUrl + "\\list";
        
        string orderByParam;
        if (filter.orderBy == OrderBy.TITLE) {
            orderByParam = "title";
        } else {
            orderByParam = "cooking_time";
        }
        string url = $"{endpointUrl}\\list?order_by={orderByParam}";

        using HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.GetAsync(url);

        List<string> categories = ["category1", "category2"];
        RecipeEntry recipeEntry1 = new("hash", "title", "description", "imagePath", categories, 15);
        RecipeEntry recipeEntry2 = new("hash2", "title2", "description2", "imagePath2", categories, 30);
        return [recipeEntry1, recipeEntry2];
    }
}

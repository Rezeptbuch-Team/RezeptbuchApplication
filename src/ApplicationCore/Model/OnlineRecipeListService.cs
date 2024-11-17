using ApplicationCore.Interfaces;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Model;

public class OnlineRecipeListService(HttpClient httpClient) : IOnlineRecipeListService
{
    public string BuildUrl(Filter filter) {
        string url = "?";
        url += "count=" + filter.count.ToString() + "&";
        url += "offset=" + filter.offset.ToString() + "&";
        if (filter.orderBy == OrderBy.COOKINGTIME) {
            url += "order_by=cooking_time&";
        }
        if (filter.order == Order.DESCENDING) {
            url += "order=desc&";
        }
        if (filter.categories.Count > 0) {
            url += "categories=";
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

        HttpResponseMessage response = await httpClient.GetAsync(url);
        // json handling
        // extraction into recipe entries

        List<string> categories = ["category1", "category2"];
        RecipeEntry recipeEntry1 = new("hash", "title", "description", "imagePath", categories, 15);
        RecipeEntry recipeEntry2 = new("hash2", "title2", "description2", "imagePath2", categories, 30);
        return [recipeEntry1, recipeEntry2];
    }
}

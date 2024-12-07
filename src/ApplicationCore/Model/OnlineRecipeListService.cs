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
        // HttpResponseMessage response = await httpClient.GetAsync(url);
        // if (response.IsSuccessStatusCode) {
        //     string json = await response.Content.ReadAsStringAsync();
        //     Root extractedRoot = JsonSerializer.Deserialize<Root>(json)!;
            
        //     foreach (Recipe recipeEntry in extractedRoot.recipes) {
        //         recipes.Add(new RecipeEntry(recipeEntry.hash,
        //         recipeEntry.title, recipeEntry.description, recipeEntry.image_path, recipeEntry.categories,
        //         recipeEntry.cooking_time));
        //     }
        // } 
        
        // download images and change the image path afterwards

        // example data
        List<string> categories = ["category1", "category2"];
        recipes.Add(new("hj73js9sjd", "Spaghetti Bolognese", "Klassische Spaghetti mit würziger Hackfleischsoße", "imagePath1", categories, 25));
        recipes.Add(new("kd83k29fke", "Tomatensuppe", "Frische Tomatensuppe mit Basilikum und Croutons", "imagePath2", categories, 20));
        recipes.Add(new("po92md8fsl", "Chicken Curry", "Hähnchenbrust in cremiger Currysauce mit Reis", "imagePath3", categories, 35));
        recipes.Add(new("zj29ck3pwd", "Caesar Salad", "Knackiger Salat mit Hähnchenstreifen, Parmesan und Croutons", "imagePath4", categories, 15));
        recipes.Add(new("lm29fk3hsa", "Pancakes", "Lockere Pancakes mit Ahornsirup und Früchten", "imagePath5", categories, 20));
        recipes.Add(new("ns89dj4kas", "Vegetarische Lasagne", "Lasagne mit Gemüse, Tomatensauce und Käse", "imagePath6", categories, 40));
        recipes.Add(new("ql49xk5bfa", "Sushi", "Selbstgemachte Maki- und Nigiri-Rollen", "imagePath7", categories, 60));
        recipes.Add(new("vr83fj6hnd", "Griechischer Salat", "Salat mit Feta, Gurken, Tomaten und Oliven", "imagePath8", categories, 10));
        recipes.Add(new("pk47gm2hsa", "Pizza Margherita", "Knusprige Pizza mit Tomatensauce, Mozzarella und Basilikum", "imagePath9", categories, 30));
        recipes.Add(new("ab39dk2pla", "Ratatouille", "Französisches Gemüsegericht mit Zucchini, Aubergine und Paprika", "imagePath10", categories, 45));
        return recipes;
    }
}

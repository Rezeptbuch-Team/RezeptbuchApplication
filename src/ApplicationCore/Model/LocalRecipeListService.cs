using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class LocalRecipeListService() : ILocalRecipeListService
{

    public async Task<List<RecipeEntry>> GetLocalRecipeList(Filter filter) {

        List<RecipeEntry> recipes = [];
        // get from local database using filters

        // example data
        List<string> categories = ["category1", "category2"];
        recipes.Add(new("zx83hd9kwe", "Chili con Carne", "Würziges Hackfleischgericht mit Bohnen und Mais", "imagePath11", categories, 30));
        recipes.Add(new("pl92md4lkq", "Risotto", "Cremiges Risotto mit Pilzen und Parmesan", "imagePath12", categories, 35));
        recipes.Add(new("vk49pd3hma", "Burger", "Saftiger Rindfleisch-Burger mit Käse und Salat", "imagePath13", categories, 25));
        recipes.Add(new("qw82bf9skd", "Tacos", "Mexikanische Tacos mit Hähnchen und Avocado", "imagePath14", categories, 20));
        recipes.Add(new("ms94dk3pla", "Quiche Lorraine", "Herzhafter Kuchen mit Speck und Käse", "imagePath15", categories, 40));
        recipes.Add(new("bn23jf4xka", "Shakshuka", "Orientalisches Gericht mit Eiern in Tomatensauce", "imagePath16", categories, 25));
        recipes.Add(new("lk73hs9twd", "Frittata", "Italienisches Omelett mit Gemüse und Käse", "imagePath17", categories, 20));
        recipes.Add(new("cm92sk5xba", "Pho", "Vietnamesische Nudelsuppe mit Rindfleisch", "imagePath18", categories, 50));
        recipes.Add(new("xp84hs3nka", "Pad Thai", "Thailändische gebratene Nudeln mit Tofu und Garnelen", "imagePath19", categories, 30));
        recipes.Add(new("ft49dl2fqa", "Currywurst", "Gegrillte Wurst mit würziger Currysauce", "imagePath20", categories, 15));
        recipes.Add(new("mn63ks8hla", "Eintopf", "Deftiger Gemüseeintopf mit Würstchen", "imagePath21", categories, 40));
        recipes.Add(new("kl39fa4sqa", "Gulasch", "Herzhaftes Gulasch mit Paprika und Zwiebeln", "imagePath22", categories, 90));
        recipes.Add(new("sq29dn7kla", "Poke Bowl", "Frische Bowl mit Lachs, Reis und Gemüse", "imagePath23", categories, 20));
        recipes.Add(new("jx84md9zka", "Zitronenkuchen", "Saftiger Zitronenkuchen mit Zuckerguss", "imagePath24", categories, 50));
        recipes.Add(new("wr93hf6pla", "Moussaka", "Griechischer Auflauf mit Aubergine und Hackfleisch", "imagePath25", categories, 60));

        return recipes;
    }
}

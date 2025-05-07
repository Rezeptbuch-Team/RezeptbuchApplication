namespace ApplicationCore.Common.Types;

public class RecipeEntry(string hash, string title, string description, string imagePath, List<string> categories, int cookingTime) {
    public readonly string hash = hash;
    public readonly string title = title;
    public readonly string description = description;
    public readonly string imagePath = imagePath;
    public List<string> categories = categories;
    public readonly int cookingTime = cookingTime;
}
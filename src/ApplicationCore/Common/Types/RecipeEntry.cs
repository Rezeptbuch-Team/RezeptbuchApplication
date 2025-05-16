namespace ApplicationCore.Common.Types;

public struct RecipeEntry(string hash, string title, string description, string imagePath, List<string> categories, int cookingTime) {
    public string Hash { get; } = hash;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string ImagePath { get; } = imagePath;
    public List<string> Categories { get; set; } = categories;
    public int CookingTime { get; } = cookingTime;
}
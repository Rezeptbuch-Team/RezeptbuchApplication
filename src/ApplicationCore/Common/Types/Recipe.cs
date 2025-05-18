namespace ApplicationCore.Common.Types;

public struct Ingredient
{
    public string Name { get; }
    public int Amount { get; set; }
    public string Unit { get; }
}

public struct Instruction {
    /// <summary>
    /// A list of text and ingredients
    /// </summary>
    public IList<object> Items;
}

public struct Recipe(string hash, string title, string description, int cookingTime, IList<string> categories, IList<string> ingredients, string imagePath, int servings, IList<Instruction> instructions)
{
    public string Hash { get; } = hash;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public int CookingTime { get; } = cookingTime;
    public IList<string> Categories { get; } = categories;
    public IList<string> Ingredients { get; } = ingredients;
    public string ImagePath { get; } = imagePath;
    public int Servings { get; } = servings;
    public IList<Instruction> Instructions { get; } = instructions;
}
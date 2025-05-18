namespace ApplicationCore.Common.Types;

public struct Ingredient
{
    public string Name { get; set; }
    public int Amount { get; set; }
    public string Unit { get; set; }
}

public struct Instruction {
    /// <summary>
    /// A list of text and ingredients
    /// </summary>
    public List<object> Items { get; set; }
}

public class Recipe
{
    public required string Hash { get; set; }
    public required string Title { get; set; }
    public required string ImagePath { get; set; }
    public required string Description { get; set; }
    public required int Servings { get; set; }
    public required int CookingTime { get; set; }
    public required List<string> Categories { get; set; }
    public List<Instruction> Instructions { get; set; } = [];
}
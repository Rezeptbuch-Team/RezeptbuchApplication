namespace ApplicationCore.Common.Types;

public struct Recipe(string hash, string title, string description, int cookingTime, IList<string>[] categories, string imagePath, int servings, IList<Instruction> instructions)
{
    public string Hash { get; } = hash;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public int CookingTime { get; } = cookingTime;
    public IList<string>[] Categories { get; } = categories;
    public string ImagePath { get; } = imagePath;
    public int Servings { get; } = servings;
    public IList<Instruction> Instructions { get; } = instructions;
}
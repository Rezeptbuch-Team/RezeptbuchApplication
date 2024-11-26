namespace ApplicationCore.Common.Types;

public struct Recipe(string hash, string title, string description)
{
    public string Hash { get; set; } = hash;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
}
namespace ApplicationCore.Common.Types;

public struct Recipe(string hash, string title, string description)
{
    public string Hash = hash;
    public string Title = title;
    public string Description = description;
}
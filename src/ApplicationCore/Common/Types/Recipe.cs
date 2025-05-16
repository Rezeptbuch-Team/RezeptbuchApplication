namespace ApplicationCore.Common.Types;

public struct Recipe(string hash, string title, string description)
{
    public string Hash { get; }= hash;
    public string Title { get; }= title;
    public string Description { get; }= description;
}
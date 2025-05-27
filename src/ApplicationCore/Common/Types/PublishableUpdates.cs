namespace ApplicationCore.Common.Types;

public struct PublishableUpdate(string hash, string title)
{
    public string Hash { get; } = hash;

    public string Title { get; } = title;
}
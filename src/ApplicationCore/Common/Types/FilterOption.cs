namespace ApplicationCore.Common.Types;

public readonly struct FilterOption(string name, int count)
{
    public string Name { get; } = name;
    public int Count { get; }= count;
}
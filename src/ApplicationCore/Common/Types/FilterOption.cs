namespace ApplicationCore.Common.Types;

public readonly struct FilterOption(string name, int count)
{
    public readonly string Name = name;
    public readonly int Count = count;
}
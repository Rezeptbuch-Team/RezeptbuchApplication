namespace ApplicationCore.Common.Types;

public struct Ingredient(string name, int quantity)
{
    public string Name { get; }= name;
    public int Quantity { get; }= quantity;
}
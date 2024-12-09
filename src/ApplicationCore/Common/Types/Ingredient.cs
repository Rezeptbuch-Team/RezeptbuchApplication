namespace ApplicationCore.Common.Types;

public struct Ingredient(string name, int quantity)
{
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
}
namespace ApplicationCore.Common.Types;

public struct Ingredient(string name, int quantity)
{
    public string Name = name;
    public int Quantity = quantity;
}
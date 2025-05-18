namespace ApplicationCore.Common.Types;

public struct Ingredient {
    public string Name { get; }
    public int Amount { get; set; }
    public string Unit { get; }
}
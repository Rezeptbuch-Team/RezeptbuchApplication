using System.Security.Cryptography;
using System.Text;

namespace ApplicationCore.Common.Types;

public class Ingredient
{
    public required string Name { get; set; }
    public required int Amount { get; set; }
    public required string Unit { get; set; }

    public override bool Equals(object? obj) =>
        obj is Ingredient i &&
        Name == i.Name && Amount == i.Amount && Unit == i.Unit;

    public override int GetHashCode() =>  HashCode.Combine(Name, Amount, Unit);

    public override string ToString()
    {
        return Amount + " " + Unit + " " + Name;
    }
}

public class Instruction
{
    /// <summary>
    /// A list of text and ingredients
    /// </summary>
    public List<object> Items { get; set; } = [];

    public override string ToString()
    {
        return string.Join(' ', Items);
    }
}

public class Recipe
{
    public required string Hash { get; set; }
    public required PublishOption PublishOption { get; set; }
    public required string Title { get; set; }
    public required string ImagePath { get; set; }
    public required string Description { get; set; }
    public required int Servings { get; set; }
    public required int CookingTime { get; set; }
    public required List<string> Categories { get; set; }
    public List<Instruction> Instructions { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="servings">different serving size</param>
    /// <returns></returns>
    public List<Ingredient> GetIngredients(float? servings = null)
    {
        float amountFactor = 1;
        if (servings != null)
        {
            amountFactor = (float)servings / Servings;
        }

        List<Ingredient> ingredients = [];

        foreach (Instruction instruction in Instructions)
        {
            foreach (object item in instruction.Items)
            {
                if (item is Ingredient originalIngredient)
                {
                    Ingredient ingredient = new()
                    {
                        Name = originalIngredient.Name,
                        Amount = (int)(originalIngredient.Amount * amountFactor),
                        Unit = originalIngredient.Unit
                    };

                    Ingredient? existingIngredient = ingredients
                        .FirstOrDefault(i => new IngredientNameUnitComparer().Equals(i, ingredient));
                    if (existingIngredient != null)
                    {
                        existingIngredient.Amount += ingredient.Amount;
                    }
                    else
                    {
                        ingredients.Add(ingredient);
                    }
                }
            }
        }

        return ingredients.GroupBy(ing => ing.Name)
                            .SelectMany(group => group)
                            .ToList();
    }

    public class IngredientNameUnitComparer : IEqualityComparer<Ingredient>
    {
        public bool Equals(Ingredient? x, Ingredient? y)
            => x != null && y != null && x.Name == y.Name && x.Unit == y.Unit;

        public int GetHashCode(Ingredient obj)
        {
            return HashCode.Combine(obj.Name, obj.Unit);
        }
    }

    public List<Instruction> GetInstructions(float? servings = null)
    {
        if (servings == null) return Instructions;

        float amountFactor = (float)servings / Servings;

        List<Instruction> scaledInstructions = [];
        foreach (Instruction orginalInstruction in Instructions)
        {
            Instruction newInstruction = new();
            foreach (object item in orginalInstruction.Items)
            {
                if (item is string text)
                {
                    newInstruction.Items.Add(text);
                }
                else if (item is Ingredient orginalIngredient)
                {
                    Ingredient newIngredient = new()
                    {
                        Name = orginalIngredient.Name,
                        Amount = (int)(orginalIngredient.Amount * amountFactor),
                        Unit = orginalIngredient.Unit
                    };
                    newInstruction.Items.Add(newIngredient);
                }
            }
            scaledInstructions.Add(newInstruction);
        }

        return scaledInstructions;
    }

    public string CalculateHash()
    {
        string inputString = Title + ImagePath + Description + Servings.ToString() + CookingTime.ToString() + string.Join("", Categories) + string.Join("", Instructions);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        StringBuilder sb = new();
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
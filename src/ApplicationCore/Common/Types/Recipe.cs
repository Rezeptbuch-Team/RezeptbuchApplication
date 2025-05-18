namespace ApplicationCore.Common.Types;

public class Ingredient
{
    public required string Name { get; set; }
    public required int Amount { get; set; }
    public required string Unit { get; set; }

    public override bool Equals(object obj) =>
        obj is Ingredient i &&
        Name == i.Name && Amount == i.Amount && Unit == i.Unit;
}

public class Instruction {
    /// <summary>
    /// A list of text and ingredients
    /// </summary>
    public List<object> Items { get; set; } = [];
}

public class Recipe
{
    public required string Hash { get; set; }
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

        List <Ingredient> ingredients = [];

        foreach (Instruction instruction in Instructions)
        {
            foreach (object item in instruction.Items)
            {
                if (item is Ingredient ingredient)
                {
                    ingredient.Amount = (int)(ingredient.Amount * amountFactor);

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
        public bool Equals(Ingredient x, Ingredient y)
        {
            return x.Name == y.Name && x.Unit == y.Unit;
        }

        public int GetHashCode(Ingredient obj)
        {
            return HashCode.Combine(obj.Name, obj.Unit);
        }
    }
}
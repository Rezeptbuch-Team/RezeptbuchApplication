using ApplicationCore.Common.Types;

namespace ApplicationCore.Tests;

[TestFixture]
public class RecipeGetInstructionsTests
{
    // a recipe should have a function to get the recipe steps with the given amount of servings
    public Recipe baseRecipe;
    [SetUp]
    public void Setup()
    {
        baseRecipe = new()
        {
            Hash = "asd",
            PublishOption = PublishOption.PUBLISHED,
            Title = "Pasta",
            ImagePath = "pasta.png",
            Description = "Simple pasta recipe.",
            Servings = 2,
            CookingTime = 20,
            Categories = ["Pasta", "Vegan"]
        };
    }

    public class InstructionComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            if (x.Items.Count != y.Items.Count) return false;

            for (int i = 0; i < x.Items.Count; i++)
            {
                var itemX = x.Items[i];
                var itemY = y.Items[i];

                if (itemX is string strX && itemY is string strY)
                {
                    if (strX != strY) return false;
                }
                else if (itemX is Ingredient ingX && itemY is Ingredient ingY)
                {
                    if (!ingX.Equals(ingY)) return false;
                }
                else
                {
                    // Type mismatch (e.g. one is string, one is Ingredient)
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(Instruction obj)
        {
            int hash = 17;
            foreach (var item in obj.Items)
            {
                if (item is string s)
                    hash = hash * 23 + s.GetHashCode();
                else if (item is Ingredient ing)
                    hash = hash * 23 + HashCode.Combine(ing.Name, ing.Amount, ing.Unit);
            }
            return hash;
        }
    }

    [Test]
    public void WillReturnInstructions()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="pasta", Amount=200, Unit="g"
                    }
                ]
            },
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="basil", Amount=3, Unit="pieces"
                    }
                ]
            }
        ];
        List<Instruction> expectedInstructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="pasta", Amount=200, Unit="g"
                    }
                ]
            },
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="basil", Amount=3, Unit="pieces"
                    }
                ]
            }
        ];

        Assert.That(baseRecipe.GetInstructions(), Is.EqualTo(expectedInstructions).Using(new InstructionComparer()));
    }

    [Test]
    public void WillReturnInstructionsWithChangedAmounts_WhenGivenDifferentServingSize()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="pasta", Amount=200, Unit="g"
                    }
                ]
            },
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="basil", Amount=3, Unit="pieces"
                    }
                ]
            }
        ];
        List<Instruction> expectedInstructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=1200, Unit="ml"
                    },
                    new Ingredient{
                        Name="pasta", Amount=400, Unit="g"
                    }
                ]
            },
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="basil", Amount=6, Unit="pieces"
                    }
                ]
            }
        ];

        Assert.That(baseRecipe.GetInstructions(baseRecipe.Servings*2), Is.EqualTo(expectedInstructions).Using(new InstructionComparer()));
    }
}
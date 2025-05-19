using ApplicationCore.Common.Types;

namespace ApplicationCore.Tests;

/// <summary>
/// a recipe should return a list of ingredients with their respective amount
/// </summary>
[TestFixture]
public class RecipeGetIngredientsTests
{
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

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_SimpleIngredients()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
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
        List<Ingredient> expectedIngredients = [
            new Ingredient{
                Name="water", Amount=600, Unit="ml"
            },
            new Ingredient{
                Name="pasta", Amount=200, Unit="g"
            },
            new Ingredient{
                Name="basil", Amount=3, Unit="pieces"
            }
        ];

        Assert.That(baseRecipe.GetIngredients(), Is.EqualTo(expectedIngredients));
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_SimpleIngredients_IfThereIsText()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    "and then cook",
                    new Ingredient{
                        Name="pasta", Amount=200, Unit="g"
                    },
                    "until al dente."
                ]
            },
            new Instruction(){
                Items = [
                    "Now add some",
                    new Ingredient{
                        Name="basil", Amount=3, Unit="pieces"
                    },
                    "and serve"
                ]
            }
        ];
        List<Ingredient> expectedIngredients = [
            new Ingredient{
                Name="water", Amount=600, Unit="ml"
            },
            new Ingredient{
                Name="pasta", Amount=200, Unit="g"
            },
            new Ingredient{
                Name="basil", Amount=3, Unit="pieces"
            }
        ];

        Assert.That(baseRecipe.GetIngredients(), Is.EqualTo(expectedIngredients));
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_DuplicateIngredients_WithDifferentUnits()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="water", Amount=1, Unit="l"
                    }
                ]
            }
        ];
        List<Ingredient> expectedIngredients = [
            new Ingredient{
                Name="water", Amount=600, Unit="ml"
            },
            new Ingredient{
                Name="water", Amount=1, Unit="l"
            }
        ];

        Assert.That(baseRecipe.GetIngredients(), Is.EqualTo(expectedIngredients));
    }

    [Test]
    public void GetIngredients_ShouldCorrectlyExtract_DuplicateIngredients_AndSumAmount_WhenSameUnits()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="water", Amount=1, Unit="l"
                    },
                    new Ingredient{
                        Name="water", Amount=200, Unit="ml"
                    }
                ]
            }
        ];
        List<Ingredient> expectedIngredients = [
            new Ingredient{
                Name="water", Amount=800, Unit="ml"
            },
            new Ingredient{
                Name="water", Amount=1, Unit="l"
            }
        ];

        Assert.That(baseRecipe.GetIngredients(), Is.EqualTo(expectedIngredients));
    }

    [Test]
    public void GetIngredients_ChangesAmount_WhenDifferentServingIsSet()
    {
        baseRecipe.Instructions = [
            new Instruction(){
                Items = [
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    new Ingredient{
                        Name="water", Amount=1, Unit="l"
                    },
                    new Ingredient{
                        Name="water", Amount=200, Unit="ml"
                    }
                ]
            }
        ];
        List<Ingredient> expectedIngredients = [
            new Ingredient{
                Name="water", Amount=1600, Unit="ml"
            },
            new Ingredient{
                Name="water", Amount=2, Unit="l"
            }
        ];

        Assert.That(baseRecipe.GetIngredients(baseRecipe.Servings*2), Is.EqualTo(expectedIngredients));
    }
}
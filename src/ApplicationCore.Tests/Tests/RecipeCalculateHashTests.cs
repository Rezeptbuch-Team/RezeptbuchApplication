using System.Security.Cryptography;
using System.Text;
using ApplicationCore.Common.Types;

namespace ApplicationCore.Tests.Tests;

[TestFixture]
public class RecipeCalculateHashTests
{
    public Recipe baseRecipe;
    public List<Instruction> instructions;
    [SetUp]
    public void Setup()
    {
        instructions = [
            new Instruction(){
                Items = [
                    "Boil",
                    new Ingredient{
                        Name="water", Amount=600, Unit="ml"
                    },
                    "Add",
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
        baseRecipe = new()
        {
            Hash = "asd",
            PublishOption = PublishOption.PUBLISHED,
            Title = "Pasta",
            ImagePath = "pasta.png",
            Description = "Simple pasta recipe.",
            Servings = 2,
            CookingTime = 20,
            Categories = ["Pasta", "Vegan"],
            Instructions = instructions
        };
    }

    [Test]
    public void RecipeWillCalculateCorrectHash()
    {
        string expectedStringToConvert = "Pastapasta.pngSimple pasta recipe.220PastaVegan";
        foreach (Instruction instruction in instructions)
        {
            expectedStringToConvert += instruction.ToString();
        }

        byte[] inputBytes = Encoding.UTF8.GetBytes(expectedStringToConvert);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        StringBuilder sb = new();
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));
        string expectedHash = sb.ToString();

        Assert.That(baseRecipe.CalculateHash(), Is.EqualTo(expectedHash));
    }
}
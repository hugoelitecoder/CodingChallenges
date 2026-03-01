using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int availableEggs = int.Parse(inputs[0]);
        int availableFlour = int.Parse(inputs[1]);
        int availableSugar = int.Parse(inputs[2]);
        int availableButter = int.Parse(inputs[3]);

        Console.Error.WriteLine($"[DEBUG] Available - Eggs: {availableEggs}, Flour: {availableFlour}, Sugar: {availableSugar}, Butter: {availableButter}");

        var recipes = new List<DessertRecipe>
        {
            new DessertRecipe("Cake", 3, 180, 100, 100),
            new DessertRecipe("Cookie", 1, 100, 150, 50),
            new DessertRecipe("Muffin", 2, 150, 100, 150)
        };

        string bestRecipeName = "";
        int maxDessertCount = -1;

        foreach (var recipe in recipes)
        {
            int possibleYield = recipe.CalculateMaximumYield(availableEggs, availableFlour, availableSugar, availableButter);
            Console.Error.WriteLine($"[DEBUG] Recipe: {recipe.Name}, Yield: {possibleYield}");

            if (possibleYield > maxDessertCount)
            {
                maxDessertCount = possibleYield;
                bestRecipeName = recipe.Name;
            }
        }

        Console.WriteLine($"{maxDessertCount} {bestRecipeName}");
    }
}

class DessertRecipe
{
    public string Name { get; }
    private readonly int _eggsRequired;
    private readonly int _flourRequired;
    private readonly int _sugarRequired;
    private readonly int _butterRequired;

    public DessertRecipe(string name, int eggsRequired, int flourRequired, int sugarRequired, int butterRequired)
    {
        Name = name;
        _eggsRequired = eggsRequired;
        _flourRequired = flourRequired;
        _sugarRequired = sugarRequired;
        _butterRequired = butterRequired;
    }

    public int CalculateMaximumYield(int availableEggs, int availableFlour, int availableSugar, int availableButter)
    {
        int maxByEggs = availableEggs / _eggsRequired;
        int maxByFlour = availableFlour / _flourRequired;
        int maxBySugar = availableSugar / _sugarRequired;
        int maxByButter = availableButter / _butterRequired;

        int limitA = maxByEggs < maxByFlour ? maxByEggs : maxByFlour;
        int limitB = maxBySugar < maxByButter ? maxBySugar : maxByButter;

        return limitA < limitB ? limitA : limitB;
    }
}
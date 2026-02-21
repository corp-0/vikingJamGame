using System;

namespace VikingJamGame.Models;

public sealed class GameResources
{
    public int Population { get; private set; }
    public int Food { get; private set; }
    public int Gold { get; private set; }

    public event Action? GameResourcesChanged;
    public event Action<int>? SuppliesCostApplied;

    public void SetInitialResources(int population, int food, int gold)
    {
        Population = Math.Max(0, population);
        Food = Math.Max(0, food);
        Gold = Math.Max(0, gold);
        GameResourcesChanged?.Invoke();
    }

    public void ConsumeFoodForMovement(int amount)
    {
        SpendFood(amount);
        SuppliesCostApplied?.Invoke(amount);
    }

    public void AddFood(int amount) => Mutate(() => Food = Math.Max(0, Food + amount));
    public void SpendFood(int amount) => Mutate(() => Food = Math.Max(0, Food - amount));

    public void AddGold(int amount) => Mutate(() => Gold = Math.Max(0, Gold + amount));
    public void SpendGold(int amount) => Mutate(() => Gold = Math.Max(0, Gold - amount));

    public void AddPopulation(int amount) => Mutate(() => Population = Math.Max(0, Population + amount));
    public void RemovePopulation(int amount) => Mutate(() => Population = Math.Max(0, Population - amount));

    private void Mutate(Action mutation)
    {
        mutation();
        GameResourcesChanged?.Invoke();
    }
}

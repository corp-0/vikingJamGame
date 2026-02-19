namespace VikingJamGame.Models;

public sealed class GameState
{

    public int Population { get; private set; } = 0;
    public int Food { get; private set; } = 0;
    public int Gold { get; private set; } = 0;

    public int Strength { get; private set; } = 0;
    public int Honor { get; private set; } = 0;
    public int Feats { get; private set; } = 0;

    public bool GameOverReached { get; private set; }

    public void AddFood(int amount) => Food = Change(Food, +amount, min: 0);
    public void SpendFood(int amount) => Food = Change(Food, -amount, min: 0);

    public void AddGold(int amount) => Gold = Change(Gold, +amount, min: 0);
    public void SpendGold(int amount) => Gold = Change(Gold, -amount, min: 0);

    public void AddPopulation(int amount) => Population = Change(Population, +amount, min: 0);
    public void RemovePopulation(int amt)
    {
        Population = Change(Population, -amt, min: 0);
        CheckGameOver();
    }

    public void AddStrength(int amount) => Strength = Change(Strength, +amount, min: 0, max: 20);
    public void RemoveStrength(int amt)
    {
        Strength = Change(Strength, -amt, min: 0, max: 20);
        CheckGameOver();
    }

    public void AddHonor(int amount) => Honor = Change(Honor, +amount, min: 0, max: 20);
    public void RemoveHonor(int amount) => Honor = Change(Honor, -amount, min: 0, max: 20);

    public void AddFeats(int amount) => Feats = Change(Feats, +amount, min: 0, max: 20);
    public void RemoveFeats(int amount) => Feats = Change(Feats, -amount, min: 0, max: 20);

    private static int Change(int stat, int delta, int min, int max = int.MaxValue)
    {
        long next = (long)stat + delta;
        if (next < min) next = min;
        if (next > max) next = max;
        return (int)next;
    }

    private void CheckGameOver()
    {
        if (GameOverReached) return;

        if (Population == 0 || Strength == 0)
        {
            GameOverReached = true;
        }
    }
}

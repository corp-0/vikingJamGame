using System;
using VikingJamGame.TemplateUtils;

namespace VikingJamGame.Models;

public sealed class PlayerInfo
{
    public string Name { get; private set; } = "";
    public BirthChoice BirthChoice { get; private set; } = BirthChoice.Boy;
    public string Title { get; private set; } = "";
    public int Strength { get; private set; }
    public int Honor { get; private set; }
    public int Feats { get; private set; }

    public event Action? PlayerInfoChanged;

    public void SetInitialInfo(string name, BirthChoice birthChoice, string title, int strength, int honor, int feats)
    {
        Name = name;
        BirthChoice = birthChoice;
        Title = title;
        Strength = Math.Clamp(strength, 0, 20);
        Honor = Math.Clamp(honor, 0, 20);
        Feats = Math.Clamp(feats, 0, 20);
        PlayerInfoChanged?.Invoke();
    }

    public void AddStrength(int amount) => Mutate(() => Strength = Math.Clamp(Strength + amount, 0, 20));
    public void RemoveStrength(int amount) => Mutate(() => Strength = Math.Clamp(Strength - amount, 0, 20));

    public void AddHonor(int amount) => Mutate(() => Honor = Math.Clamp(Honor + amount, 0, 20));
    public void RemoveHonor(int amount) => Mutate(() => Honor = Math.Clamp(Honor - amount, 0, 20));

    public void AddFeats(int amount) => Mutate(() => Feats = Math.Clamp(Feats + amount, 0, 20));
    public void RemoveFeats(int amount) => Mutate(() => Feats = Math.Clamp(Feats - amount, 0, 20));

    private void Mutate(Action mutation)
    {
        mutation();
        PlayerInfoChanged?.Invoke();
    }
}

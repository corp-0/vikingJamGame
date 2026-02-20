using System;
using System.Collections.Generic;
using VikingJamGame.TemplateUtils;

namespace VikingJamGame.Models;

public record GameDataWrapper
{
    public required GameResources GameResources { get; init; }
    public required PlayerInfo PlayerInfo { get; init; }
}

public readonly record struct TitleDefinition(
    string Name,
    int Population,
    int Food,
    int Gold,
    int Strength,
    int Honor,
    int Feats);

public class InitialResourcesFactory
{
    public static IReadOnlyList<TitleDefinition> Titles { get; } = [
        new("the Ironborn", 24, 18, 8, 9, 6, 0),
        new("the Oathkeeper", 28, 16, 7, 6, 9, 0),
        new("the Sea Wolf", 21, 22, 9, 7, 6, 0),
        new("the Stormforged", 26, 17, 8, 10, 5, 0),
        new("the Hearth-Blessed", 35, 20, 5, 5, 8, 0),
        new("the Boneless", 18, 10, 4, 2, 3, 0),
        new("the Oathbreaker", 20, 12, 12, 5, 2, 0),
        new("the Starved", 17, 6, 9, 4, 4, 0),
        new("the Coward", 19, 14, 6, 3, 3, 0),
        new("the Ragged", 16, 9, 3, 4, 5, 0),
    ];

    public static TitleDefinition RollRandomTitle(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        int index = rng.Next(Titles.Count);
        return Titles[index];
    }
    
    public static GameDataWrapper FromPrologueData(BirthChoice gender, string name)
    {
        TitleDefinition title = RollRandomTitle();
        return FromPrologueData(gender, name, title);
    }

    public static GameDataWrapper FromPrologueData(BirthChoice gender, string name, TitleDefinition title)
    {
        string resolvedName = string.IsNullOrWhiteSpace(name) ? "Nameless" : name.Trim();

        var gameResources = new GameResources();
        gameResources.SetInitialResources(title.Population, title.Food, title.Gold);

        var playerInfo = new PlayerInfo();
        playerInfo.SetInitialInfo(
            resolvedName,
            gender,
            title.Name,
            title.Strength,
            title.Honor,
            title.Feats);

        return new GameDataWrapper
        {
            GameResources = gameResources,
            PlayerInfo = playerInfo,
        };
    }
}

using VikingJamGame.TemplateUtils;

namespace VikingJamGame.Models.GameEvents.Compilation;

public readonly record struct GameEventTemplateContext(
    BirthChoice BirthChoice,
    string Name,
    string Title)
{
    public static GameEventTemplateContext Default => new(
        BirthChoice.ChildOfOmen,
        "{Name}",
        "{Title}");
}

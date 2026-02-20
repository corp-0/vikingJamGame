namespace VikingJamGame.TemplateUtils;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum BirthChoice
{
    Boy,
    Girl,
    ChildOfOmen
}

public sealed record Pronouns(string Subj, string Obj, string PossAdj, string Refl)
{
    public string SubjCap => Capitalize(Subj);
    public string ObjCap => Capitalize(Obj);
    public string PossAdjCap => Capitalize(PossAdj);
    public string ReflCap => Capitalize(Refl);

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}

public static class Template
{
    private static readonly Regex TokenRegex = new(@"\{([A-Za-z]+)\}", RegexOptions.Compiled);

    public static string Render(string template, BirthChoice choice, string name, string title) =>
        Render(template, PronounsFor(choice), name, title);

    public static string Render(string template, Pronouns p, string name, string title)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["he"] = p.Subj,
            ["He"] = p.SubjCap,
            ["him"] = p.Obj,
            ["Him"] = p.ObjCap,
            ["his"] = p.PossAdj,
            ["His"] = p.PossAdjCap,
            ["himself"] = p.Refl,
            ["Himself"] = p.ReflCap,
            ["Name"] = name,
            ["Title"] = title,
        };

        return TokenRegex.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            return map.TryGetValue(key, out var value) ? value : m.Value; // leave unknown tokens untouched
        });
    }

    public static Pronouns PronounsFor(BirthChoice choice) => choice switch
    {
        BirthChoice.Boy => He,
        BirthChoice.Girl => She,
        BirthChoice.ChildOfOmen => They,
        _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null),
    };

    public static readonly Pronouns He = new("he", "him", "his", "himself");
    public static readonly Pronouns She = new("she", "her", "her", "herself");
    public static readonly Pronouns They = new("they", "them", "their", "themselves");
}

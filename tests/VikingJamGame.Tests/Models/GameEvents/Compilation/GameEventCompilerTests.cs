using System.Linq;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents.Compilation;
using VikingJamGame.Models.GameEvents.Conditions;
using VikingJamGame.Models.GameEvents.Definitions;
using VikingJamGame.Models.GameEvents.Effects;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.GameEvents.Stats;
using VikingJamGame.TemplateUtils;

namespace VikingJamGame.Tests.Models.GameEvents.Compilation;

public sealed class GameEventCompilerTests
{
    [Fact]
    public void Compile_SortsOptionsByOrder_AndSeparatesConditionsFromCosts()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.alpha",
            Name = "Alpha",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Second",
                    ResolutionText = "Second",
                    Order = 2
                },
                new GameEventOptionDefinition
                {
                    DisplayText = "First",
                    ResolutionText = "First",
                    Order = 1,
                    Conditions = ["food:1"],
                    Costs = ["food:3", "gold:2"]
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition);

        Assert.Equal([1, 2], compiled.Options.Select(option => option.Order).ToArray());
        GameEventOption first = compiled.Options.First();

        // Condition parsed as visibility condition only
        Assert.Single(first.VisibilityConditions);
        Assert.Contains(first.VisibilityConditions, c => c is StatThresholdCondition { Stat: StatId.Food, MinAmount: 1 });

        // Costs are separate and not merged with conditions
        Assert.Equal(2, first.Costs.Count);
        Assert.Contains(first.Costs, amount => amount is { Stat: StatId.Food, Amount: 3 });
        Assert.Contains(first.Costs, amount => amount is { Stat: StatId.Gold, Amount: 2 });
    }

    [Fact]
    public void Compile_ThrowsWhenOptionOrderIsDuplicated()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.dup",
            Name = "Dup",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "A",
                    ResolutionText = "A",
                    Order = 1
                },
                new GameEventOptionDefinition
                {
                    DisplayText = "B",
                    ResolutionText = "B",
                    Order = 1
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("duplicate option Order=1", exception.Message);
    }

    [Fact]
    public void Compile_HasNoEffectsWhenEffectFieldIsMissing()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.noop",
            Name = "Noop",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Wait",
                    ResolutionText = "Waited",
                    Order = 1
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition);

        Assert.Empty(compiled.Options.Single().Effects);
    }

    [Fact]
    public void Compile_ThrowsWhenConditionUsesUnknownStat()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badstat",
            Name = "Bad Stat",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Conditions = ["mystery:2"]
                }
            ]
        };

        InvalidOperationException exception = Assert.
            Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("unknown condition key 'mystery' in Condition", exception.Message);
        Assert.Contains("Valid keys are:", exception.Message);
        Assert.Contains("feats", exception.Message);
    }

    [Fact]
    public void Compile_UnknownConditionKey_SuggestsClosestKnownKey()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badconditionkey",
            Name = "Bad Condition Key",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Conditions = ["feat:2"]
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("unknown condition key 'feat' in Conditions", exception.Message);
        Assert.Contains("Valid keys are:", exception.Message);
        Assert.Contains("Did you mean 'feats'?", exception.Message);
    }

    [Fact]
    public void Compile_ThrowsWhenConditionAmountHasExplicitSign()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badamount",
            Name = "Bad Amount",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Conditions = ["strength:+3"]
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("bad amount '+3' in Conditions", exception.Message);
        Assert.Contains("do not allow a '+' sign", exception.Message);
        Assert.Contains("Use 'strength:3' instead", exception.Message);
    }

    [Fact]
    public void Compile_ThrowsWhenMultipleConditionEntriesAreInvalid()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.multiplebadconditions",
            Name = "Multiple Bad Conditions",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Conditions = ["strength:+3", "honor:-5"]
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("bad amount '+3' in Conditions", exception.Message);
        Assert.Contains("bad amount '-5' in Conditions", exception.Message);
        Assert.Contains("cannot be negative", exception.Message);
        Assert.Contains("Use Costs or Effects to spend or change stats", exception.Message);
    }

    [Fact]
    public void Compile_UnknownEffectToken_ListsValidEffectTokens()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badeffecttoken",
            Name = "Bad Effect Token",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Effects = ["strengt:+1"]
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("unknown effect token 'strengt' in Effects", exception.Message);
        Assert.Contains("Valid effect tokens are:", exception.Message);
        Assert.Contains("Did you mean 'strength'?", exception.Message);
    }

    [Fact]
    public void Compile_UnknownCostStat_ListsValidStatKeys()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badcoststat",
            Name = "Bad Cost Stat",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Costs = ["strengt:2"]
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition));

        Assert.Contains("unknown stat 'strengt' in Costs", exception.Message);
        Assert.Contains("Valid stat keys are:", exception.Message);
        Assert.Contains("Did you mean 'strength'?", exception.Message);
    }

    [Fact]
    public void Compile_ParsesSignedEffectPairs()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.effects",
            Name = "Effects",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Plunder",
                    ResolutionText = "Plundered",
                    Order = 1,
                    Effects = ["food:+5", "honor:-1"]
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition);

        GameEventOption option = compiled.Options.Single();
        Assert.Equal(2, option.Effects.Count);
        Assert.Contains(option.Effects, e => e is StatChangeEffect { Stat: StatId.Food, Amount: 5 });
        Assert.Contains(option.Effects, e => e is StatChangeEffect { Stat: StatId.Honor, Amount: -1 });
    }

    [Fact]
    public void Compile_RendersTemplatedTextWithPronouns()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.templated",
            Name = "{Title}",
            Description = "{He} sailed into the storm.",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Follow {him}",
                    ResolutionText = "{His} legend grows.",
                    Order = 1
                }
            ]
        };

        var templateContext = new GameEventTemplateContext(
            BirthChoice.Girl,
            "Astrid",
            "Stormborn");

        GameEvent compiled = GameEventCompiler.Compile(
            definition,
            templateContext);

        Assert.Equal("Stormborn", compiled.Name);
        Assert.Equal("She sailed into the storm.", compiled.Description);
        Assert.Equal("Follow her", compiled.Options[0].DisplayText);
        Assert.Equal("Her legend grows.", compiled.Options[0].ResolutionText);
    }
}

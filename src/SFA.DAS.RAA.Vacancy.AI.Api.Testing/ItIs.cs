using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using Moq;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Testing;

public static class ItIs
{
    private static bool MatchingAssertion(Action assertion)
    {
        using var assertionScope = new AssertionScope();
        assertion();
        return assertionScope.Discard().Length == 0;
    }

    public static T EquivalentTo<T>(T obj)
    {
        return EquivalentTo(obj, config => config);
    }

    public static T EquivalentTo<T>(T obj, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config)
    {
        return It.Is<T>(seq => MatchingAssertion(() => seq.Should().BeEquivalentTo(obj, config, "")));
    }
}
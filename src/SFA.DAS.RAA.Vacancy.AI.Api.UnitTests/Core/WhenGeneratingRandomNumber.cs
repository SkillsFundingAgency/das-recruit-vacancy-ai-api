
using SFA.DAS.RAA.Vacancy.AI.Api.Core;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Core;

public class WhenGeneratingRandomNumber
{
    [Test, MoqAutoData]
    public void Then_It_Generates_Random_Numbers_Between_0_And_1(RandomNumberGenerator sut)
    {
        // arrange
        List<double> items = [];

        // act
        for (var index=0; index<10; index++)
        {
            items.Add(sut.NextDouble());
        }

        // assert
        items.Should().AllSatisfy(x => x.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(1));
        items.Any(x => !x.Equals(items[0])).Should().BeTrue(); // not all the same number
    }
}
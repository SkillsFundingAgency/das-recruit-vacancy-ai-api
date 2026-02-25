namespace SFA.DAS.RAA.Vacancy.AI.Api.Core;

public interface IRandomNumberGenerator
{
    double NextDouble();
}

public class RandomNumberGenerator : IRandomNumberGenerator
{
    private static readonly Random Generator = new();
    
    public double NextDouble()
    {
        return Generator.NextDouble();
    }
}
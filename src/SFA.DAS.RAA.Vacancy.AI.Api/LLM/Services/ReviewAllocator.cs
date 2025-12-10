using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public class ReviewAllocator 
{
    private const double ProbAmber = 0.5F;
    private const double ProbGreen = 0.01F;
    public bool Allocator(TrafficLight traf) 
    {
        if (traf.TrafficLightRatingSystemEnum is <= 0 or > 3) 
        {
            return true; // always review - should never happen
        }

        var rnd = new Random();
        var rand =  rnd.NextDouble();
        return traf.TrafficLightRatingSystemEnum switch
        {
            3 => true,
            2 => rand <= ProbAmber,
            _ => rand <= ProbGreen
        };
    }
}
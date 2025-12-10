namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class TrafficLight 
{
    public int TrafficLightRatingSystemEnum { get; set; } = 0;
    public string TrafficLightRatingSystemDescription { get; set; } = "Green";

    public TrafficLight(int rating)
    {
        TrafficLightRatingSystemEnum = rating;
        TrafficLightRatingSystemDescription = rating switch
        {
            <= 0 => "Not Defined - ",
            1 => "Green",
            2 => "Amber",
            3 => "Red",
            _ => "Not Defined +"
        };
    }
} //TODO this look and feels like it should just be an enum
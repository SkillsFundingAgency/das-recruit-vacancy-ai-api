using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests;

public class TrafficLightTests
{
        
    [Test]
    public void TrafficLightVerification_RED()
    {
        TrafficLight traf = new(1);
        //Assert.Equal("Green", traf.traffic_light_rating_system_description);
        Assert.That(traf.TrafficLightRatingSystemDescription, Is.EqualTo("Green"));
    }
        
    [Test]
    public void TrafficLightVerification_AMBER()
    {
        TrafficLight traf1 = new(2);
        //Assert.Equal("Amber", traf1.Traffic_light_rating_system_description);
        Assert.That(traf1.TrafficLightRatingSystemDescription, Is.EqualTo("Amber"));
    }

    [Test]
    public void TrafficLightVerification_Green()
    {
        TrafficLight traf2 = new(3);
        //Assert.Equal("Red", traf2.traffic_light_rating_system_description);
        Assert.That(traf2.TrafficLightRatingSystemDescription, Is.EqualTo("Red"));
    }
    [Test]
    public void TrafficLightAssignment_RedDiscriminationIssue()
    {
        // verify that an issue of discrimination _does_ flag the required property
        TrafficLight traf_verif = new(3);

        List<AICheckOutput> outputs = [];
        AICheckOutput dummy_discrim_check = new(true, "-", "DiscriminationCheck");
        outputs.Add(dummy_discrim_check);

        PrioritisationSystem prio = new();
        TrafficLight trafval = prio.TrafficLightAssignment(outputs);
        // this should identify as RED
        Assert.That(trafval.TrafficLightRatingSystemEnum, Is.EqualTo(traf_verif.TrafficLightRatingSystemEnum));

    }

    [Test]
    public void TrafficLightAssignment_RedConsistencyIssue()
    {
        // verify that an issue of discrimination _does_ flag the required property
        TrafficLight traf_verif = new(3);

        List<AICheckOutput> outputs = [];
        AICheckOutput dummy_discrim_check = new(true, "-", "TextInconsistencyCheck");
        outputs.Add(dummy_discrim_check);

        PrioritisationSystem prio = new();
        TrafficLight trafval = prio.TrafficLightAssignment(outputs);
        // this should identify as RED
        Assert.That( trafval.TrafficLightRatingSystemEnum,Is.EqualTo(traf_verif.TrafficLightRatingSystemEnum));
    }

    [Test]
    public void TrafficLightAssignment_AmberSpellingVacancy()
    {
        TrafficLight traf_verif = new(2); // Expecting it to be amber

        List<AICheckOutput> outputs = [];
        AICheckOutput dummy_text_check = new(true, "-", "SpellingCheck-allfields");
        outputs.Add(dummy_text_check);
        PrioritisationSystem prio = new();

        TrafficLight trafval = prio.TrafficLightAssignment(outputs);
        Assert.That(trafval.TrafficLightRatingSystemEnum,Is.EqualTo(traf_verif.TrafficLightRatingSystemEnum));
    }
    [Test]
    public void TrafficLightAssignment_GreenVacancy()
    {
        TrafficLight traf_verif = new(1);

        AICheckOutput dummy_discrim_check = new(false, "-", "DiscriminationCheck");
        AICheckOutput dummy_text_check = new(false, "-", "TextInconsistencyCheck");
        AICheckOutput dummy_spelling_check = new(false, "-", "SpellingCheck-allfields");
        List<AICheckOutput> outputs = [];

        outputs.Add(dummy_discrim_check);
        outputs.Add(dummy_text_check);
        outputs.Add(dummy_spelling_check);

        PrioritisationSystem prio = new();
        TrafficLight traf_val = prio.TrafficLightAssignment(outputs);

        Assert.That(traf_val.TrafficLightRatingSystemEnum, Is.EqualTo(traf_verif.TrafficLightRatingSystemEnum));

    }

    [Test]
    public void PrioSystemRecommendReview_OnRed()
    {
        TrafficLight trafv = new(3);
        ReviewAllocator alloc = new();
        bool resp = alloc.Allocator(trafv);
        Assert.That(resp,Is.True);
    }

}
using SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

namespace UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DummyTest()
        {
            Assert.Pass();
        }
        
        [Test]
        public void TestOneEquality()
        {
            int one = 1;
            Assert.That(one,Is.EqualTo(1));
        }

        [Test]
        public void CheckLLMFlagging_YES()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("YES", false, false);
            Assert.That(output,Is.True); // expect true
        }
        [Test]
        public void CheckLLMFlagging_NO()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("No", false, false);

            
            Assert.That(output, Is.False);
        }

        [Test]
        public void CheckLLMFlagging_YES_INVERTED()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("YES", true, false);
            
            Assert.That(output, Is.False);
        }
        [Test]
        public void CheckLLMFlagging_NO_INVERTED()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("NO", true, false);
            
            Assert.That(output, Is.True);
        }

        [Test]
        public void CheckSpellingNo()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("None", false, true);
            
            Assert.That(output, Is.False);
        }

        [Test]
        public void CheckSpellingYes()
        {
            VacancyQA vacqa = new();
            bool output = vacqa.FlagifyLLMResponse("I've identified several spelling mistakes in this document", false, true);
            
            Assert.That(output, Is.True);
            
        }
        [Test]
        public void TrafficLightVerification_RED()
        {
            TrafficLight traf = new(1);
            //Assert.Equal("Green", traf.traffic_light_rating_system_description);
            Assert.That(traf.Traffic_light_rating_system_description, Is.EqualTo("Green"));
        }
        
        [Test]
        public void TrafficLightVerification_AMBER()
        {
            TrafficLight traf1 = new(2);
            //Assert.Equal("Amber", traf1.Traffic_light_rating_system_description);
            Assert.That(traf1.Traffic_light_rating_system_description, Is.EqualTo("Amber"));
        }

        [Test]
        public void TrafficLightVerification_Green()
        {
            TrafficLight traf2 = new(3);
            //Assert.Equal("Red", traf2.traffic_light_rating_system_description);
            Assert.That(traf2.Traffic_light_rating_system_description, Is.EqualTo("Red"));
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
            Assert.That(trafval.Traffic_light_rating_system_enum, Is.EqualTo(traf_verif.Traffic_light_rating_system_enum));

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
            Assert.That( trafval.Traffic_light_rating_system_enum,Is.EqualTo(traf_verif.Traffic_light_rating_system_enum));
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
            Assert.That(trafval.Traffic_light_rating_system_enum,Is.EqualTo(traf_verif.Traffic_light_rating_system_enum));
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

            Assert.That(traf_val.Traffic_light_rating_system_enum, Is.EqualTo(traf_verif.Traffic_light_rating_system_enum));

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
}
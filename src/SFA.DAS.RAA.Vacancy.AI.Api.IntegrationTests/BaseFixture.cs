namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests;

public abstract class BaseFixture
{
    protected TestServer Server;
    protected HttpClient Client;
    protected IFixture Fixture;
    
    [SetUp]
    public virtual void Setup()
    {
        Server = new TestServer();
        Client = Server.CreateClient();
        Fixture = new Fixture();
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [TearDown]
    public virtual void Teardown()
    {
        Client.Dispose();
        Server.Dispose();
    }
}
namespace Charging.IntegrationTests;

public class GuidFixture : IDisposable
{
    public Guid Guid = Guid.NewGuid();

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
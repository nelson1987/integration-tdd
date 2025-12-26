namespace Charging.IntegrationTests;

[CollectionDefinition("GuidCollection")]
public class GuidCollectionFixture :
    ICollectionFixture<GuidFixture>,
    ICollectionFixture<ApiFixture>
{
}
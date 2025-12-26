using Xunit.Abstractions;
namespace Charging.IntegrationTests;


[Collection("GuidCollection")]
public class IntegrationTests1
{
    private readonly ITestOutputHelper _output;
    private readonly Guid _guid;
    private readonly HttpClient _client;

    public IntegrationTests1(ITestOutputHelper output, GuidFixture guidFixture, ApiFixture apiFixture)
    {
        _guid = guidFixture.Guid;
        _output = output;
        _client = apiFixture._client;
    }

    [Fact]
    public async Task Test1()
    {
        _output.WriteLine("Hello World! {0}", _guid);
        await Task.Delay(1000);
    }

    [Fact]
    public async Task Test2()
    {
        _output.WriteLine("Hello World! {0}", _guid);
        await Task.Delay(2000);
    }

    [Fact]
    public async Task Test3()
    {
        _output.WriteLine("Hello World! {0}", _guid);
        await Task.Delay(3000);
    }
}
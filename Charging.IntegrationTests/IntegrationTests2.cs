using Xunit.Abstractions;

namespace Charging.IntegrationTests;

[Collection("GuidCollection")]
public class IntegrationTests2
{
    private readonly Guid _guid;
    private readonly ITestOutputHelper _output;

    public IntegrationTests2(ITestOutputHelper output, GuidFixture guidFixture)
    {
        _guid = guidFixture.Guid;
        _output = output;
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
using Shared.Bulk;
using Xunit.Abstractions;

namespace Shared.Tests;

public class BulkOperatorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BulkOperatorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestBulkOperator()
    {
        var cs = new CancellationTokenSource();
        var b = new BulkOperator<int>(10, TimeSpan.FromSeconds(1), cs.Token);
        b.BulkHandler += async (sender, args) =>
        {
   
            _testOutputHelper.WriteLine("bulk args:{0} {1}",DateTime.Now, args.Data.Length);
            await Task.CompletedTask;
        };
        _testOutputHelper.WriteLine("bulk test start:{0}",DateTime.Now);
        var job = Task.Run(async () =>
        {
            while (true)
            {
                if (cs.IsCancellationRequested)
                {
                    return;
                }       
                await b.WriteAsync(42);
                Task.Delay(10, cs.Token).Wait(cs.Token);
            }
        }, cs.Token);

        Task.Delay(10000).Wait();
        
        cs.Cancel();
    }
}
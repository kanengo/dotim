using Shared.Timer;
using Xunit.Abstractions;

namespace Shared.Tests;

public class TimeWheelTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TimeWheelTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestTimeWheel()
    {
        using var timer = new TimeWheel<int>(TimeSpan.FromMilliseconds(10));
        timer.OnTick +=  async (sender, args) =>
        {
            _testOutputHelper.WriteLine("OnTick {0} {1}", DateTime.Now, args.Data);
            var tw = sender as TimeWheel<int>;

            var tick = (double)args.Data;
            tw?.Timeout(TimeSpan.FromSeconds(tick), args.Data);
            
            // await Task.Delay(TimeSpan.FromSeconds(10));
        };
        
        try
        {
            timer.Start();
            _testOutputHelper.WriteLine("start {0}", DateTime.Now);
            // var r = new Random();
            // for (var i = 0; i < 100; i++)
            // {
            //     timer.Timeout(TimeSpan.FromSeconds(6), i);
            // }
        
            // timer.Timeout(TimeSpan.FromSeconds(4), 4);
            timer.Timeout(TimeSpan.FromSeconds(1), 1);
            // timer.Timeout(TimeSpan.FromSeconds(5), 5);
            timer.Timeout(TimeSpan.FromSeconds(2), 2);
            // timer.Timeout(TimeSpan.FromSeconds(6), 6);
            // timer.Timeout(TimeSpan.FromSeconds(9), 9);
            // timer.Timeout(TimeSpan.FromSeconds(7), 7);

            Task.Delay(TimeSpan.FromSeconds(10)).Wait();
        }
        catch (Exception e)
        {
            _testOutputHelper.WriteLine("e {0}",e);
            throw;
        }

       
    }
}
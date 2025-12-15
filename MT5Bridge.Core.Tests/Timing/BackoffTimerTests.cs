using MT5Bridge.Core.Timing;

namespace MT5Bridge.Core.Tests.Timing;

public class BackoffTimerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        Assert.Equal(100, timer.CurrentIntervalMs);
    }

    [Fact]
    public void Constructor_WithInvalidInitialInterval_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new BackoffTimer(0, 5000, 2.0));
        Assert.Throws<ArgumentException>(() => new BackoffTimer(-100, 5000, 2.0));
    }

    [Fact]
    public void Constructor_WithInvalidMaxInterval_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new BackoffTimer(100, 0, 2.0));
        Assert.Throws<ArgumentException>(() => new BackoffTimer(100, -5000, 2.0));
    }

    [Fact]
    public void Constructor_WithInvalidMultiplier_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new BackoffTimer(100, 5000, 1.0));
        Assert.Throws<ArgumentException>(() => new BackoffTimer(100, 5000, 0.5));
    }

    [Fact]
    public void CanExecute_FirstCall_ShouldReturnTrue()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        Assert.True(timer.CanExecute());
    }

    [Fact]
    public void CanExecute_ImmediateSecondCall_ShouldReturnFalse()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        timer.CanExecute(); // First call
        Assert.False(timer.CanExecute()); // Immediate second call
    }

    [Fact]
    public async Task CanExecute_AfterDelay_ShouldReturnTrue()
    {
        var timer = new BackoffTimer(50, 5000, 2.0);

        Assert.True(timer.CanExecute()); // First call
        Assert.False(timer.CanExecute()); // Immediate second call

        // Wait for doubled interval (100ms) plus buffer for timing precision
        await Task.Delay(120);

        Assert.True(timer.CanExecute()); // Should be ready now
    }

    [Fact]
    public async Task CurrentIntervalMs_ShouldIncreaseAfterExecution()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        Assert.Equal(100, timer.CurrentIntervalMs);

        timer.CanExecute(); // First execution
        Assert.Equal(200, timer.CurrentIntervalMs); // Should double

        await Task.Delay(210);
        timer.CanExecute(); // Second execution
        Assert.Equal(400, timer.CurrentIntervalMs); // Should double again
    }

    [Fact]
    public async Task CurrentIntervalMs_ShouldNotExceedMax()
    {
        var timer = new BackoffTimer(100, 300, 2.0);

        timer.CanExecute(); // 100 -> 200
        await Task.Delay(210);

        timer.CanExecute(); // 200 -> 300 (capped)
        Assert.Equal(300, timer.CurrentIntervalMs);

        await Task.Delay(310);
        timer.CanExecute(); // Should stay at 300
        Assert.Equal(300, timer.CurrentIntervalMs);
    }

    [Fact]
    public void Reset_ShouldResetToInitialInterval()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        timer.CanExecute(); // 100 -> 200
        Assert.Equal(200, timer.CurrentIntervalMs);

        timer.Reset();
        Assert.Equal(100, timer.CurrentIntervalMs);
        Assert.True(timer.CanExecute()); // Should be able to execute immediately after reset
    }

    [Fact]
    public void GetRemainingWaitTimeMs_AfterExecution_ShouldReturnApproximateInterval()
    {
        var timer = new BackoffTimer(1000, 5000, 2.0);

        timer.CanExecute();

        long remaining = timer.GetRemainingWaitTimeMs();

        // Should be close to 2000 (doubled interval) with some tolerance
        Assert.InRange(remaining, 1800, 2100);
    }

    [Fact]
    public async Task GetRemainingWaitTimeMs_ShouldDecrease()
    {
        var timer = new BackoffTimer(500, 5000, 2.0);

        timer.CanExecute();

        long remaining1 = timer.GetRemainingWaitTimeMs();
        await Task.Delay(100);
        long remaining2 = timer.GetRemainingWaitTimeMs();

        Assert.True(remaining2 < remaining1);
    }

    [Fact]
    public void GetRemainingWaitTimeMs_WhenReady_ShouldReturnZero()
    {
        var timer = new BackoffTimer(100, 5000, 2.0);

        long remaining = timer.GetRemainingWaitTimeMs();

        Assert.Equal(0, remaining); // Should be ready immediately
    }

    [Fact]
    public async Task RealWorldScenario_RetryWithBackoff()
    {
        var timer = new BackoffTimer(50, 200, 2.0);
        var executionCount = 0;
        var intervals = new List<double>();

        for (int i = 0; i < 5; i++)
        {
            while (!timer.CanExecute())
            {
                await Task.Delay(10);
            }

            intervals.Add(timer.CurrentIntervalMs);
            executionCount++;
        }

        Assert.Equal(5, executionCount);
        // Intervals should be: 100 (after 1st), 200 (after 2nd, capped), 200, 200
        Assert.Equal(100, intervals[0]);
        Assert.Equal(200, intervals[1]);
        Assert.Equal(200, intervals[2]);
    }
}

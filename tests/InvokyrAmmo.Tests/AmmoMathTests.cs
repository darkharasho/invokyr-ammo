using InvokyrAmmo;
using Xunit;

public class AmmoMathTests
{
    [Theory]
    [InlineData(6f, 1f, 6f)]
    [InlineData(6f, 2f, 12f)]
    [InlineData(6f, 1.5f, 9f)]
    [InlineData(5f, 1.5f, 8f)]   // 7.5 rounds up
    [InlineData(6f, 0.1f, 1f)]   // never below one shot
    [InlineData(6f, 1000f, 300f)] // clamped to 50x
    public void DiscreteRoundsToWholeShots(float baseCap, float mult, float expected) =>
        Assert.Equal(expected, AmmoMath.ScaledCapacity(baseCap, mult, discrete: true));

    [Fact]
    public void ContinuousKeepsFractions() =>
        Assert.Equal(15f, AmmoMath.ScaledCapacity(10f, 1.5f, discrete: false), 3);

    [Fact]
    public void ZeroCapacityAndNaNAreLeftAlone()
    {
        Assert.Equal(0f, AmmoMath.ScaledCapacity(0f, 2f, true));
        Assert.Equal(6f, AmmoMath.ScaledCapacity(6f, float.NaN, true));
    }

    [Theory]
    [InlineData(6f, 4f, 6f)]
    [InlineData(6f, 12f, 12f)]
    public void ClientCapacityFollowsHigherLevel(float local, float level, float expected) =>
        Assert.Equal(expected, AmmoMath.ClientCapacity(local, level));
}

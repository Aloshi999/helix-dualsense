using Helix.Services;

namespace Helix.Tests;

public class StickMapperTests
{
    [Fact]
    public void CenterIsNeutral()
    {
        var (x, y) = StickMapper.ToXInput(128, 128, 0.12, invertY: true);
        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [Fact]
    public void DeadzoneSwallowsSmallDeflection()
    {
        var (x, y) = StickMapper.ToXInput(140, 128, 0.12, invertY: true);
        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [Fact]
    public void InvertYMakesUpPositive()
    {
        var (_, y) = StickMapper.ToXInput(128, 0, 0, invertY: true);
        Assert.True(y > 30000);
    }
}

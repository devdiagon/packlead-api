using Packlead.Domain.Exceptions;
using Packlead.Domain.ValueObjects;

namespace Packlead.Domain.Tests.ValueObjects;

public class LocationTests
{
    // D.LOC.01
    [Fact]
    public void Constructor_WithinValidRange_CreatesInstance()
    {
        var location = new Location(4.71, -74.07);

        Assert.Equal(4.71, location.Lat);
        Assert.Equal(-74.07, location.Lng);
    }

    // D.LOC.02
    [Fact]
    public void Constructor_LatAboveMax_ThrowsInvalidLocationException()
    {
        Assert.Throws<InvalidLocationException>(() => new Location(95, 0));
    }

    // D.LOC.03
    [Fact]
    public void Constructor_LatBelowMin_ThrowsInvalidLocationException()
    {
        Assert.Throws<InvalidLocationException>(() => new Location(-95, 0));
    }

    // D.LOC.04
    [Fact]
    public void Constructor_LngAboveMax_ThrowsInvalidLocationException()
    {
        Assert.Throws<InvalidLocationException>(() => new Location(0, 185));
    }

    // D.LOC.05
    [Fact]
    public void Constructor_LngBelowMin_ThrowsInvalidLocationException()
    {
        Assert.Throws<InvalidLocationException>(() => new Location(0, -185));
    }

    // D.LOC.06
    [Fact]
    public void Equals_SameLatLng_ReturnsTrue()
    {
        var a = new Location(4.71, -74.07);
        var b = new Location(4.71, -74.07);

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }
}
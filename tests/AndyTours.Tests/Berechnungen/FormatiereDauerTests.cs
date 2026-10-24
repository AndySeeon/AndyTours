using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FormatiereDauer"/>.
/// </summary>
public class FormatiereDauerTests
{
    [Theory]
    [InlineData(0, 0, 0, "0:00:00")]
    [InlineData(0, 5, 7, "0:05:07")]
    [InlineData(3, 45, 12, "3:45:12")]
    [InlineData(30, 5, 7, "30:05:07")]
    [InlineData(512, 13, 45, "512:13:45")]
    public void Dauer_wird_als_Stunden_Minuten_Sekunden_formatiert(int stunden, int minuten, int sekunden, string erwartet)
    {
        Assert.Equal(erwartet, FormatiereDauer.Ausfuehren(new TimeSpan(stunden, minuten, sekunden)));
    }

    [Fact]
    public void Bruchteile_von_Sekunden_werden_gerundet()
    {
        Assert.Equal("0:01:00", FormatiereDauer.Ausfuehren(TimeSpan.FromSeconds(59.6)));
        Assert.Equal("0:00:59", FormatiereDauer.Ausfuehren(TimeSpan.FromSeconds(59.4)));
    }
}

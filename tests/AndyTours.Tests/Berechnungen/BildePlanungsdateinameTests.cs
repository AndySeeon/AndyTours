using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildePlanungsdateiname"/>.
/// </summary>
public class BildePlanungsdateinameTests
{
    [Fact]
    public void Planung_mit_Datum()
    {
        Assert.Equal("Planung 2026-03-07.gpx", BildePlanungsdateiname.Ausfuehren(new DateOnly(2026, 3, 7)));
    }
}

using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechneDurchschnittsgeschwindigkeit"/>.
/// </summary>
public class BerechneDurchschnittsgeschwindigkeitTests
{
    [Fact]
    public void Distanz_geteilt_durch_Fahrzeit()
    {
        Assert.Equal(25.0, BerechneDurchschnittsgeschwindigkeit.Ausfuehren(50, TimeSpan.FromHours(2)));
        Assert.Equal(20.0, BerechneDurchschnittsgeschwindigkeit.Ausfuehren(15, TimeSpan.FromMinutes(45))!.Value, 10);
    }

    [Fact]
    public void Ohne_Fahrzeit_gibt_es_keinen_Wert()
    {
        Assert.Null(BerechneDurchschnittsgeschwindigkeit.Ausfuehren(50, TimeSpan.Zero));
    }

    [Fact]
    public void Ohne_Distanz_gibt_es_keinen_Wert()
    {
        Assert.Null(BerechneDurchschnittsgeschwindigkeit.Ausfuehren(0, TimeSpan.FromHours(1)));
    }
}

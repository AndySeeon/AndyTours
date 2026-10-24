using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeTourdetail"/>.
/// </summary>
public class BeschreibeTourdetailTests
{
    [Fact]
    public void Kopfzeile_mit_allen_Angaben()
    {
        var einheit = new Trainingseinheit
        {
            Datum = new DateOnly(2026, 9, 26), Name = " Alpenrunde ", DistanzKm = 90.83, Hoehenmeter = 1206, MaxHoehe = 1379,
        };

        Assert.Equal("Sa, 26.09.2026 · Alpenrunde · 90,83 km · 1.206 hm · höchster Punkt 1.379 m", BeschreibeTourdetail.Ausfuehren(einheit));
    }

    [Fact]
    public void Ohne_Name_und_ohne_hoechsten_Punkt()
    {
        var einheit = new Trainingseinheit { Datum = new DateOnly(2026, 1, 5), DistanzKm = 30, Hoehenmeter = 0 };

        Assert.Equal("Mo, 05.01.2026 · 30,00 km · 0 hm", BeschreibeTourdetail.Ausfuehren(einheit));
    }
}

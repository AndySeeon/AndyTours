using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FindeAufzeichnung"/>.
/// </summary>
public class FindeAufzeichnungTests
{
    [Fact]
    public void Hoehenprofil_geht_vor_GPS_Track()
    {
        var einheit = new Trainingseinheit
        {
            Anhaenge =
            [
                new Anhang { Typ = AnhangTyp.GpsTrack, RelativerPfad = "2026/track.gpx" },
                new Anhang { Typ = AnhangTyp.Hoehenprofil, RelativerPfad = "2026/tour.fit" },
            ],
        };

        Assert.Equal("2026/tour.fit", FindeAufzeichnung.Ausfuehren(einheit));
    }

    [Fact]
    public void Ohne_Hoehenprofil_der_GPS_Track()
    {
        var einheit = new Trainingseinheit { Anhaenge = [new Anhang { Typ = AnhangTyp.GpsTrack, RelativerPfad = "2026/tour.fit" }] };

        Assert.Equal("2026/tour.fit", FindeAufzeichnung.Ausfuehren(einheit));
    }

    [Fact]
    public void Ohne_Aufzeichnung_nichts()
    {
        var nurDokumentation = new Trainingseinheit { Anhaenge = [new Anhang { Typ = AnhangTyp.Dokumentation, RelativerPfad = "bericht.htm" }] };
        var leererPfad = new Trainingseinheit { Anhaenge = [new Anhang { Typ = AnhangTyp.Hoehenprofil, RelativerPfad = " " }] };

        Assert.Null(FindeAufzeichnung.Ausfuehren(new Trainingseinheit()));
        Assert.Null(FindeAufzeichnung.Ausfuehren(nurDokumentation));
        Assert.Null(FindeAufzeichnung.Ausfuehren(leererPfad));
    }
}

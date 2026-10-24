using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleStrecke"/>.
/// </summary>
public class ErmittleStreckeTests
{
    [Fact]
    public void Punkte_in_Reihenfolge_und_umschliessendes_Rechteck()
    {
        Verlaufspunkt[] verlauf =
        [
            new() { Position = new Geoposition(47.80, 12.10) },
            new() { Position = new Geoposition(47.95, 11.90) },
            new() { Position = new Geoposition(47.70, 12.30) },
        ];

        Strecke? strecke = ErmittleStrecke.Ausfuehren(verlauf);

        Assert.NotNull(strecke);
        Assert.Equal([new(47.80, 12.10), new(47.95, 11.90), new(47.70, 12.30)], strecke.Punkte);
        Assert.Equal(new Geoposition(47.70, 11.90), strecke.SuedWest);
        Assert.Equal(new Geoposition(47.95, 12.30), strecke.NordOst);
    }

    [Fact]
    public void Punkte_ohne_Position_werden_ausgelassen()
    {
        Verlaufspunkt[] verlauf =
        [
            new() { Position = new Geoposition(47.80, 12.10) },
            new() { DistanzKm = 0.5 },
            new() { Position = new Geoposition(47.81, 12.11) },
        ];

        Strecke? strecke = ErmittleStrecke.Ausfuehren(verlauf);

        Assert.NotNull(strecke);
        Assert.Equal([new(47.80, 12.10), new(47.81, 12.11)], strecke.Punkte);
    }

    [Fact]
    public void Ohne_Positionen_keine_Strecke()
    {
        Assert.Null(ErmittleStrecke.Ausfuehren([new Verlaufspunkt { Hoehe = 500 }, new Verlaufspunkt { Hoehe = 510 }]));
        Assert.Null(ErmittleStrecke.Ausfuehren([]));
    }

    [Fact]
    public void Ein_einzelner_Punkt_ist_Start_und_Ziel()
    {
        Strecke? strecke = ErmittleStrecke.Ausfuehren([new Verlaufspunkt { Position = new Geoposition(47.8, 12.1) }]);

        Assert.NotNull(strecke);
        Assert.Single(strecke.Punkte);
        Assert.Equal(strecke.SuedWest, strecke.NordOst);
    }
}

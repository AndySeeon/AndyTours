using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BearbeiteTrack"/>.
/// </summary>
public class BearbeiteTrackTests
{
    /// <summary>Zehn Punkte entlang einer Linie nach Norden, je 0,1 km, mit steigender Höhe.</summary>
    private static List<Verlaufspunkt> Punkte() =>
        Enumerable.Range(0, 10)
            .Select(i => new Verlaufspunkt { DistanzKm = i * 0.1, Hoehe = 500 + i * 10, Position = new Geoposition(47 + i * 0.0009, 12) })
            .ToList();

    [Fact]
    public void Ausschnitt_zwischen_Start_und_Ziel_einschliesslich()
    {
        List<Verlaufspunkt> stueck = BearbeiteTrack.Ausfuehren(Punkte(), 2, 6, false, Glaettung.Aus);

        Assert.Equal([0.2, 0.3, 0.4, 0.5, 0.6], stueck.Select(p => Math.Round(p.DistanzKm, 1)));
    }

    [Fact]
    public void Umkehren_tauscht_Start_und_Ziel()
    {
        List<Verlaufspunkt> punkte = Punkte();

        List<Verlaufspunkt> umgekehrt = BearbeiteTrack.Ausfuehren(punkte, 0, 9, true, Glaettung.Aus);

        Assert.Equal(punkte[9], umgekehrt[0]);
        Assert.Equal(punkte[0], umgekehrt[^1]);
        Assert.Equal(Enumerable.Reverse(punkte), umgekehrt);
    }

    [Fact]
    public void Gekuerzt_geglaettet_und_umgekehrt_Hoehe_bleibt_je_Punkt()
    {
        List<Verlaufspunkt> punkte = Punkte();

        List<Verlaufspunkt> ergebnis = BearbeiteTrack.Ausfuehren(punkte, 1, 8, true, Glaettung.Stark);

        // Auf einer Geraden bleiben nach dem Glätten nur Start und Ziel – in umgekehrter Reihenfolge
        Assert.Equal([punkte[8], punkte[1]], ergebnis);
        Assert.Equal(580, ergebnis[0].Hoehe);
    }

    [Fact]
    public void Zwei_benachbarte_Punkte_ergeben_zwei_Punkte()
    {
        Assert.Equal(2, BearbeiteTrack.Ausfuehren(Punkte(), 4, 5, false, Glaettung.Stark).Count);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(3, 10)]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    public void Ungueltige_Grenzen_werfen_eine_Ausnahme(int start, int ziel)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BearbeiteTrack.Ausfuehren(Punkte(), start, ziel, false, Glaettung.Aus));
    }

    [Fact]
    public void Verlauf_bleibt_unveraendert()
    {
        List<Verlaufspunkt> punkte = Punkte();
        List<Verlaufspunkt> kopie = [.. punkte];

        BearbeiteTrack.Ausfuehren(punkte, 1, 8, true, Glaettung.Stark);

        Assert.Equal(kopie, punkte);
    }
}

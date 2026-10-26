using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="WaehleStuetzpunkte"/>.
/// </summary>
public class WaehleStuetzpunkteTests
{
    /// <summary>Breitengrade je 100 m.</summary>
    private const double Hundert = 0.1 / 111.195;

    /// <summary>Eine gerade Spur nach Norden mit einem Punkt alle 100 m.</summary>
    private static List<Geoposition> Gerade(int punkte, double laenge = 11.0) =>
        [.. Enumerable.Range(0, punkte).Select(i => new Geoposition(48.0 + i * Hundert, laenge))];

    private static List<Geoposition> Alle(IReadOnlyList<IReadOnlyList<Geoposition>> pakete) =>
        [.. pakete.SelectMany((p, i) => i == 0 ? p : p.Skip(1))];

    [Fact]
    public void Ohne_zwei_Positionen_keine_Pakete()
    {
        Assert.Empty(WaehleStuetzpunkte.Ausfuehren([]));
        Assert.Empty(WaehleStuetzpunkte.Ausfuehren([new Geoposition(48, 11)]));
    }

    [Fact]
    public void Kurze_gerade_Spur_ergibt_Anfang_und_Ende()
    {
        List<Geoposition> spur = Gerade(6);

        IReadOnlyList<Geoposition> paket = Assert.Single(WaehleStuetzpunkte.Ausfuehren(spur));

        Assert.Equal([spur[0], spur[^1]], paket);
    }

    [Fact]
    public void Lange_gerade_Spur_hat_hoechstens_einen_Kilometer_zwischen_den_Stuetzpunkten()
    {
        List<Geoposition> spur = Gerade(51);

        List<Geoposition> stuetzen = Alle(WaehleStuetzpunkte.Ausfuehren(spur));

        Assert.Equal(spur[0], stuetzen[0]);
        Assert.Equal(spur[^1], stuetzen[^1]);
        for (int i = 1; i < stuetzen.Count; i++)
        {
            Assert.InRange(Geoabstand.Meter(stuetzen[i - 1], stuetzen[i]), 1, WaehleStuetzpunkte.HoechstabstandMeter + 1);
        }

        Assert.InRange(stuetzen.Count, 6, 8);
    }

    [Fact]
    public void Abzweigung_bleibt_als_Stuetzpunkt_erhalten()
    {
        // 500 m nach Norden, dann 500 m nach Osten
        List<Geoposition> spur = Gerade(6);
        Geoposition ecke = spur[^1];
        double osten = Hundert / Math.Cos(48.0 * Math.PI / 180);
        spur.AddRange(Enumerable.Range(1, 5).Select(i => new Geoposition(ecke.Breite, ecke.Laenge + i * osten)));

        List<Geoposition> stuetzen = Alle(WaehleStuetzpunkte.Ausfuehren(spur));

        Assert.Equal([spur[0], ecke, spur[^1]], stuetzen);
    }

    [Fact]
    public void Kleine_Abweichungen_fallen_weg()
    {
        // Seitlich um 5 m zitternde Spur
        double fuenfMeter = 0.005 / 111.195 / Math.Cos(48.0 * Math.PI / 180);
        List<Geoposition> spur = [.. Gerade(6).Select((p, i) => p with { Laenge = p.Laenge + (i % 2) * fuenfMeter })];

        Assert.Equal(2, Alle(WaehleStuetzpunkte.Ausfuehren(spur)).Count);
    }

    [Fact]
    public void Pakete_haben_hoechstens_25_Punkte_und_teilen_sich_den_Randpunkt()
    {
        // 60 km Zickzack: jede Ecke ist ein Stützpunkt
        double osten = 2 * Hundert / Math.Cos(48.0 * Math.PI / 180);
        List<Geoposition> spur = [.. Enumerable.Range(0, 60).Select(i => new Geoposition(48.0 + i * Hundert * 5, 11.0 + (i % 2) * osten))];

        IReadOnlyList<IReadOnlyList<Geoposition>> pakete = WaehleStuetzpunkte.Ausfuehren(spur);

        Assert.Equal([25, 25, 12], pakete.Select(p => p.Count));
        Assert.Equal(pakete[0][^1], pakete[1][0]);
        Assert.Equal(pakete[1][^1], pakete[2][0]);
        Assert.Equal(spur, Alle(pakete));
    }
}

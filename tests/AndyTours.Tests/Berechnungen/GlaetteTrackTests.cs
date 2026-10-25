using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="GlaetteTrack"/>.
/// </summary>
public class GlaetteTrackTests
{
    /// <summary>
    /// Ein Viertelkreis (Radius rund 500 m) aus 200 Punkten mit seitlichem Zittern von ±4 m.
    /// </summary>
    private static List<Verlaufspunkt> ZitterndeKurve()
    {
        const double gradJeMeter = 1 / 111_320.0;
        return Enumerable.Range(0, 200).Select(i =>
        {
            double winkel = i / 199.0 * Math.PI / 2;
            double radius = 500 + (i % 2 == 0 ? 4 : -4);
            return new Verlaufspunkt
            {
                DistanzKm = i * 0.004,
                Hoehe = 600 + i,
                Position = new Geoposition(47 + radius * Math.Sin(winkel) * gradJeMeter, 12 + radius * (1 - Math.Cos(winkel)) * gradJeMeter * 1.47),
            };
        }).ToList();
    }

    /// <summary>
    /// Maß des Zitterns: größte Abweichung eines Punkts von der Kurve ohne Zittern (Radius 500 m), in Metern.
    /// </summary>
    private static double Zittern(List<Verlaufspunkt> punkte) =>
        punkte.Max(p =>
        {
            // Kreismittelpunkt bei (47, 12 + 500 m); Rückrechnung derselben Näherung wie in ZitterndeKurve
            double nord = (p.Position!.Breite - 47) * 111_320;
            double ost = (p.Position.Laenge - 12) * 111_320 / 1.47 - 500;
            return Math.Abs(Math.Sqrt(nord * nord + ost * ost) - 500);
        });

    [Fact]
    public void Aus_liefert_die_Punkte_unveraendert()
    {
        List<Verlaufspunkt> punkte = ZitterndeKurve();

        Assert.Equal(punkte, GlaetteTrack.Ausfuehren(punkte, Glaettung.Aus));
    }

    [Fact]
    public void Stark_glaettet_mehr_als_schwach_und_beide_haben_weniger_Punkte()
    {
        List<Verlaufspunkt> punkte = ZitterndeKurve();

        List<Verlaufspunkt> schwach = GlaetteTrack.Ausfuehren(punkte, Glaettung.Schwach);
        List<Verlaufspunkt> stark = GlaetteTrack.Ausfuehren(punkte, Glaettung.Stark);

        Assert.True(schwach.Count < punkte.Count);
        Assert.True(stark.Count < schwach.Count);
        Assert.True(Zittern(schwach) < Zittern(punkte));
        Assert.True(Zittern(stark) <= Zittern(schwach));
    }

    [Theory]
    [InlineData(Glaettung.Schwach)]
    [InlineData(Glaettung.Stark)]
    public void Start_und_Ziel_bleiben_unveraendert_Hoehe_bleibt_je_Punkt(Glaettung glaettung)
    {
        List<Verlaufspunkt> punkte = ZitterndeKurve();

        List<Verlaufspunkt> geglaettet = GlaetteTrack.Ausfuehren(punkte, glaettung);

        Assert.Equal(punkte[0], geglaettet[0]);
        Assert.Equal(punkte[^1], geglaettet[^1]);
        // Jeder verbliebene Punkt trägt die Höhe und Distanz seines Originals
        Assert.All(geglaettet, p => Assert.Contains(punkte, o => o.DistanzKm == p.DistanzKm && o.Hoehe == p.Hoehe));
    }

    [Fact]
    public void Wenige_Punkte_bleiben_unveraendert()
    {
        List<Verlaufspunkt> zwei = ZitterndeKurve().Take(2).ToList();

        Assert.Equal(zwei, GlaetteTrack.Ausfuehren(zwei, Glaettung.Stark));
    }

    [Fact]
    public void Uebergebene_Liste_bleibt_unveraendert()
    {
        List<Verlaufspunkt> punkte = ZitterndeKurve();
        List<Verlaufspunkt> kopie = [.. punkte];

        GlaetteTrack.Ausfuehren(punkte, Glaettung.Stark);

        Assert.Equal(kopie, punkte);
    }
}

using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="VerbindeVerlaeufe"/>.
/// </summary>
public class VerbindeVerlaeufeTests
{
    /// <summary>Ein Messpunkt mit Distanz, Fahrzeit (Minuten) und Höhe.</summary>
    private static Verlaufspunkt Punkt(double km, int minuten, double hoehe) =>
        new() { DistanzKm = km, Fahrzeit = TimeSpan.FromMinutes(minuten), Hoehe = hoehe };

    [Fact]
    public void Distanz_und_Fahrzeit_zaehlen_ueber_die_Etappen_weiter()
    {
        var erste = new Etappe(new Trainingseinheit { Id = 1 }, [Punkt(0, 0, 500), Punkt(40, 120, 900)]);
        var zweite = new Etappe(new Trainingseinheit { Id = 2 }, [Punkt(0, 0, 900), Punkt(10, 30, 1200), Punkt(30, 90, 600)]);

        Mehrtagesverlauf verlauf = VerbindeVerlaeufe.Ausfuehren([erste, zweite]);

        Assert.Equal([0, 40, 40, 50, 70], verlauf.Punkte.Select(p => p.DistanzKm));
        Assert.Equal([0, 120, 120, 150, 210], verlauf.Punkte.Select(p => (int)p.Fahrzeit.TotalMinutes));
        Assert.Equal([500, 900, 900, 1200, 600], verlauf.Punkte.Select(p => p.Hoehe!.Value));
        Assert.Equal([0, 2], verlauf.Beginn);
        Assert.Equal([erste, zweite], verlauf.Etappen);
    }

    [Fact]
    public void Etappen_ohne_Verlauf_entfallen()
    {
        var ohne = new Etappe(new Trainingseinheit { Id = 1 }, null);
        var leer = new Etappe(new Trainingseinheit { Id = 2 }, []);
        var mit = new Etappe(new Trainingseinheit { Id = 3 }, [Punkt(0, 0, 500), Punkt(5, 15, 520)]);
        var danach = new Etappe(new Trainingseinheit { Id = 4 }, [Punkt(0, 0, 520), Punkt(8, 20, 540)]);

        Mehrtagesverlauf verlauf = VerbindeVerlaeufe.Ausfuehren([ohne, mit, leer, danach]);

        Assert.Equal([mit, danach], verlauf.Etappen);
        Assert.Equal([0, 2], verlauf.Beginn);
        Assert.Equal(13, verlauf.Punkte[^1].DistanzKm);
    }

    [Fact]
    public void Eine_Etappe_bleibt_unveraendert()
    {
        Verlaufspunkt[] punkte = [Punkt(0, 0, 500), Punkt(12.5, 40, 650)];

        Mehrtagesverlauf verlauf = VerbindeVerlaeufe.Ausfuehren([new Etappe(new Trainingseinheit(), punkte)]);

        Assert.Equal(punkte, verlauf.Punkte);
        Assert.Equal([0], verlauf.Beginn);
    }

    [Fact]
    public void Ohne_Aufzeichnungen_kein_Verlauf()
    {
        Mehrtagesverlauf verlauf = VerbindeVerlaeufe.Ausfuehren([new Etappe(new Trainingseinheit(), null)]);

        Assert.Empty(verlauf.Punkte);
        Assert.Empty(verlauf.Etappen);
    }
}

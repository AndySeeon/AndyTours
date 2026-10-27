using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PruefeFensterposition"/>.
/// </summary>
public class PruefeFensterpositionTests
{
    /// <summary>Ein Bildschirm 1920 × 1080; der Arbeitsbereich ohne Taskleiste ist 1040 hoch.</summary>
    private static readonly Bildschirmbereich EinBildschirm = new(0, 0, 1920, 1080);

    /// <summary>Arbeitsbereich des Hauptbildschirms.</summary>
    private static readonly Bildschirmbereich Arbeitsbereich = new(0, 0, 1920, 1040);

    [Fact]
    public void Sichtbare_Position_bleibt_unveraendert()
    {
        var gemerkt = new Fensterposition(100, 50, 1200, 800, false);

        Assert.Equal(gemerkt, PruefeFensterposition.Ausfuehren(gemerkt, EinBildschirm, Arbeitsbereich));
    }

    [Fact]
    public void Teilweise_herausragendes_Fenster_bleibt_wenn_die_Titelleiste_greifbar_ist()
    {
        var gemerkt = new Fensterposition(1700, 900, 1200, 800, false);

        Assert.Equal(gemerkt, PruefeFensterposition.Ausfuehren(gemerkt, EinBildschirm, Arbeitsbereich));
    }

    [Fact]
    public void Position_auf_fehlendem_zweiten_Bildschirm_wird_mittig_auf_den_Hauptbildschirm_gesetzt()
    {
        var gemerkt = new Fensterposition(2100, 100, 1200, 800, true);

        Fensterposition ergebnis = PruefeFensterposition.Ausfuehren(gemerkt, EinBildschirm, Arbeitsbereich);

        Assert.Equal(new Fensterposition(360, 120, 1200, 800, true), ergebnis);
    }

    [Fact]
    public void Titelleiste_oberhalb_des_Bildschirms_gilt_als_nicht_greifbar()
    {
        var gemerkt = new Fensterposition(100, -500, 1200, 800, false);

        Fensterposition ergebnis = PruefeFensterposition.Ausfuehren(gemerkt, EinBildschirm, Arbeitsbereich);

        Assert.Equal(120, ergebnis.Oben);
    }

    [Fact]
    public void Zu_grosses_Fenster_wird_auf_den_Arbeitsbereich_begrenzt()
    {
        var gemerkt = new Fensterposition(-3000, 0, 2560, 1400, false);

        Fensterposition ergebnis = PruefeFensterposition.Ausfuehren(gemerkt, EinBildschirm, Arbeitsbereich);

        Assert.Equal(new Fensterposition(0, 0, 1920, 1040, false), ergebnis);
    }

    [Fact]
    public void Zweiter_Bildschirm_links_vom_Hauptbildschirm_wird_beruecksichtigt()
    {
        var zweiBildschirme = new Bildschirmbereich(-1920, 0, 3840, 1080);
        var gemerkt = new Fensterposition(-1800, 100, 1200, 800, false);

        Assert.Equal(gemerkt, PruefeFensterposition.Ausfuehren(gemerkt, zweiBildschirme, Arbeitsbereich));
    }
}

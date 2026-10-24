using AndyTours.Core.Modell;
using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="LeseFitVerlauf"/> mit Dateien aus <see cref="FitTestdatei"/>.
/// </summary>
public class LeseFitVerlaufTests
{
    /// <summary>Start 26.09.2026 06:02:49 UTC.</summary>
    private static readonly System.DateTime StartUtc = new(2026, 9, 26, 6, 2, 49, DateTimeKind.Utc);

    /// <summary>Schreibt eine Testdatei und liest ihren Verlauf.</summary>
    private static (List<Verlaufspunkt>? Verlauf, string? Fehler) SchreibeUndLies(TempOrdner temp, FitTestdatei.Werte werte)
    {
        string pfad = Path.Combine(temp.Pfad, "2026-09-26-08-02-49.fit");
        FitTestdatei.Schreibe(pfad, werte);
        return LeseFitVerlauf.Ausfuehren(pfad);
    }

    [Fact]
    public void Messpunkte_mit_Distanz_in_km_Hoehe_in_m_und_Zusatzwerten()
    {
        using var temp = new TempOrdner();

        (List<Verlaufspunkt>? verlauf, string? fehler) = SchreibeUndLies(temp, new(StartUtc));

        Assert.Null(fehler);
        Assert.NotNull(verlauf);
        Assert.Equal([0.0, 0.5, 1.0], verlauf.Select(p => p.DistanzKm));
        Assert.Equal(900, verlauf[1].Hoehe!.Value, 1);
        Assert.Equal(92, verlauf[1].Puls);
        Assert.Equal(21.6, verlauf[1].Geschwindigkeit);
        Assert.Equal(85, verlauf[1].Trittfrequenz);
        Assert.Equal(15, verlauf[1].Temperatur);
    }

    [Fact]
    public void Puls_0_gilt_als_nicht_erfasst()
    {
        using var temp = new TempOrdner();

        Assert.Null(SchreibeUndLies(temp, new(StartUtc)).Verlauf?[0].Puls);
    }

    [Fact]
    public void Ohne_Pause_laeuft_die_Fahrzeit_durch()
    {
        using var temp = new TempOrdner();

        List<Verlaufspunkt>? verlauf = SchreibeUndLies(temp, new(StartUtc)).Verlauf;

        Assert.Equal([0, 60, 120], verlauf!.Select(p => (int)p.Fahrzeit.TotalSeconds));
    }

    [Fact]
    public void Pausen_der_Zeitmessung_werden_herausgelassen()
    {
        using var temp = new TempOrdner();

        List<Verlaufspunkt>? verlauf = SchreibeUndLies(temp, new(StartUtc, MitPause: true)).Verlauf;

        // Stopp nach 60 s, Start nach 100 s: Der dritte Punkt (120 s) hat nur 60 + 20 s Fahrzeit
        Assert.Equal([0, 60, 80], verlauf!.Select(p => (int)p.Fahrzeit.TotalSeconds));
    }

    [Fact]
    public void Datei_ohne_Hoehe_liefert_Punkte_ohne_Hoehe()
    {
        using var temp = new TempOrdner();

        List<Verlaufspunkt>? verlauf = SchreibeUndLies(temp, new(StartUtc, OhneHoehe: true)).Verlauf;

        Assert.Equal(3, verlauf!.Count);
        Assert.All(verlauf, p => Assert.Null(p.Hoehe));
    }

    [Fact]
    public void Positionen_in_Grad_und_Punkt_ohne_Position()
    {
        using var temp = new TempOrdner();

        List<Verlaufspunkt>? verlauf = SchreibeUndLies(temp, new(StartUtc)).Verlauf;

        // Semicircles sind auf rund 1 cm genau; 7 Nachkommastellen reichen
        Assert.Equal(FitTestdatei.ErstePosition.Breite, verlauf![0].Position!.Breite, 7);
        Assert.Equal(FitTestdatei.ErstePosition.Laenge, verlauf[0].Position!.Laenge, 7);
        Assert.Null(verlauf[1].Position);
        Assert.Equal(FitTestdatei.DrittePosition.Breite, verlauf[2].Position!.Breite, 7);
        Assert.Equal(FitTestdatei.DrittePosition.Laenge, verlauf[2].Position!.Laenge, 7);
    }

    [Fact]
    public void Datei_ohne_GPS_liefert_Punkte_ohne_Position()
    {
        using var temp = new TempOrdner();

        List<Verlaufspunkt>? verlauf = SchreibeUndLies(temp, new(StartUtc, OhneGps: true)).Verlauf;

        Assert.Equal(3, verlauf!.Count);
        Assert.All(verlauf, p => Assert.Null(p.Position));
    }

    [Fact]
    public void Beschaedigte_Datei_liefert_einen_Fehler()
    {
        using var temp = new TempOrdner();
        string pfad = Path.Combine(temp.Pfad, "kaputt.fit");
        FitTestdatei.Schreibe(pfad, new(StartUtc));
        byte[] inhalt = File.ReadAllBytes(pfad);
        inhalt[inhalt.Length / 2] ^= 0xFF;
        File.WriteAllBytes(pfad, inhalt);

        (List<Verlaufspunkt>? verlauf, string? fehler) = LeseFitVerlauf.Ausfuehren(pfad);

        Assert.Null(verlauf);
        Assert.False(string.IsNullOrEmpty(fehler));
    }

    [Fact]
    public void Keine_FIT_Datei_liefert_einen_Fehler()
    {
        using var temp = new TempOrdner();
        string pfad = Path.Combine(temp.Pfad, "text.fit");
        File.WriteAllText(pfad, "keine Aufzeichnung");

        (List<Verlaufspunkt>? verlauf, string? fehler) = LeseFitVerlauf.Ausfuehren(pfad);

        Assert.Null(verlauf);
        Assert.Equal("keine gültige FIT-Datei", fehler);
    }
}

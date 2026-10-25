using AndyTours.Core.Modell;
using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;
using Dynastream.Fit;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="LeseFitAufzeichnung"/> mit Dateien aus <see cref="FitTestdatei"/>.
/// </summary>
/// <remarks>
/// Läuft in der Testgruppe <see cref="FitTestdatei.Testgruppe"/> nicht gleichzeitig mit anderen Tests, die
/// FIT-Dateien schreiben und lesen (siehe dort).
/// </remarks>
[Collection(FitTestdatei.Testgruppe)]
public class LeseFitAufzeichnungTests
{
    /// <summary>Start 26.09.2026 06:02:49 UTC, also 08:02:49 Ortszeit bei zwei Stunden Verschiebung.</summary>
    private static readonly System.DateTime StartUtc = new(2026, 9, 26, 6, 2, 49, DateTimeKind.Utc);

    /// <summary>Erwartete Sensoren: Geschwindigkeit und Trittfrequenz, jeder nur einmal.</summary>
    private static readonly string[] ErwarteteSensoren = ["3452708077", "3452713120"];

    /// <summary>Schreibt eine Testdatei und liest sie wieder.</summary>
    private static (Aufzeichnung? Aufzeichnung, string? Fehler) SchreibeUndLies(TempOrdner temp, FitTestdatei.Werte werte)
    {
        string pfad = Path.Combine(temp.Pfad, "2026-09-26-08-02-49.fit");
        FitTestdatei.Schreibe(pfad, werte);
        return LeseFitAufzeichnung.Ausfuehren(pfad);
    }

    [Fact]
    public void Werte_der_Sitzung_werden_uebernommen()
    {
        using var temp = new TempOrdner();

        (Aufzeichnung? aufzeichnung, string? fehler) = SchreibeUndLies(temp, new(StartUtc));

        Assert.Null(fehler);
        Assert.NotNull(aufzeichnung);
        Assert.Equal("2026-09-26-08-02-49.fit", aufzeichnung.Dateiname);
        Assert.Equal("Edge 1040", aufzeichnung.Geraet);
        Assert.Equal(90.83, aufzeichnung.DistanzKm);
        Assert.Equal(1206, aufzeichnung.Hoehenmeter);
        Assert.Equal(1379, aufzeichnung.MaxHoehe);
        Assert.Equal(540, aufzeichnung.StartHoehe);
        Assert.Equal(52.23, aufzeichnung.MaxGeschwindigkeit);
        Assert.Equal((130, 154, 75), (aufzeichnung.HfDurchschnitt, aufzeichnung.HfMax, aufzeichnung.HfMin));
        Assert.Equal(75, aufzeichnung.Trittfrequenz);
        Assert.Equal(18, aufzeichnung.Temperatur);
        Assert.Equal(180, aufzeichnung.LeistungWatt);
        Assert.Equal("MTB", aufzeichnung.Profil);
    }

    [Fact]
    public void Ohne_Angaben_der_Sitzung_Puls_und_Temperatur_aus_den_Messpunkten()
    {
        using var temp = new TempOrdner();

        Aufzeichnung? aufzeichnung = SchreibeUndLies(temp, new(StartUtc, MitPulsUndTemperaturInSitzung: false)).Aufzeichnung;

        // niedrigster Puls über 0; Durchschnitt aus 14, 15 und 16 °C
        Assert.Equal(92, aufzeichnung?.HfMin);
        Assert.Equal(15, aufzeichnung?.Temperatur);
    }

    [Fact]
    public void Datum_und_Startzeit_in_Ortszeit_der_Tour()
    {
        using var temp = new TempOrdner();

        Aufzeichnung? aufzeichnung = SchreibeUndLies(temp, new(StartUtc, VerschiebungStunden: 2)).Aufzeichnung;

        Assert.Equal(new DateOnly(2026, 9, 26), aufzeichnung?.Datum);
        Assert.Equal(new TimeOnly(8, 2, 49), aufzeichnung?.Startzeit);
    }

    [Fact]
    public void Fahrzeit_ist_die_gestoppte_Zeit_ohne_Pausen()
    {
        using var temp = new TempOrdner();

        Aufzeichnung? aufzeichnung = SchreibeUndLies(temp, new(StartUtc)).Aufzeichnung;

        // gestoppt 20.407,785 s (Gesamtzeit 23.995 s); abgeschnitten wie in der Anzeige des Edge
        Assert.Equal(new TimeSpan(5, 40, 7), aufzeichnung?.Fahrzeit);
    }

    [Fact]
    public void Rad_Kennung_aus_Nachricht_526()
    {
        using var temp = new TempOrdner();

        Aufzeichnung? aufzeichnung = SchreibeUndLies(temp, new(StartUtc, RadKennung: "7af22db6-d829-4395-9e42-a200867c4abf")).Aufzeichnung;

        Assert.Equal("7af22db6-d829-4395-9e42-a200867c4abf", aufzeichnung?.RadKennung);
    }

    [Fact]
    public void Ohne_Nachricht_526_bleibt_die_Rad_Kennung_leer()
    {
        using var temp = new TempOrdner();

        Assert.Equal("", SchreibeUndLies(temp, new(StartUtc)).Aufzeichnung?.RadKennung);
    }

    [Fact]
    public void Nur_Geschwindigkeits_und_Trittfrequenzsensoren_zaehlen()
    {
        using var temp = new TempOrdner();
        FitTestdatei.Werte werte = new(StartUtc, Sensoren:
        [
            (AntplusDeviceType.HeartRate, 78209),
            (AntplusDeviceType.BikeSpeed, 3452708077),
            (AntplusDeviceType.BikeCadence, 3452713120),
            (AntplusDeviceType.BikePower, 4774),
            (AntplusDeviceType.BikeSpeed, 3452708077),
        ]);

        Aufzeichnung? aufzeichnung = SchreibeUndLies(temp, werte).Aufzeichnung;

        Assert.Equal(ErwarteteSensoren, aufzeichnung?.Sensoren);
    }

    [Fact]
    public void Beschaedigte_Datei_liefert_einen_Fehler()
    {
        using var temp = new TempOrdner();
        string pfad = Path.Combine(temp.Pfad, "kaputt.fit");
        FitTestdatei.Schreibe(pfad, new(StartUtc));
        byte[] inhalt = System.IO.File.ReadAllBytes(pfad);
        inhalt[inhalt.Length / 2] ^= 0xFF;
        System.IO.File.WriteAllBytes(pfad, inhalt);

        (Aufzeichnung? aufzeichnung, string? fehler) = LeseFitAufzeichnung.Ausfuehren(pfad);

        Assert.Null(aufzeichnung);
        Assert.False(string.IsNullOrEmpty(fehler));
    }

    [Fact]
    public void Keine_FIT_Datei_liefert_einen_Fehler()
    {
        using var temp = new TempOrdner();
        string pfad = Path.Combine(temp.Pfad, "text.fit");
        System.IO.File.WriteAllText(pfad, "keine Aufzeichnung");

        (Aufzeichnung? aufzeichnung, string? fehler) = LeseFitAufzeichnung.Ausfuehren(pfad);

        Assert.Null(aufzeichnung);
        Assert.Equal("keine gültige FIT-Datei", fehler);
    }
}

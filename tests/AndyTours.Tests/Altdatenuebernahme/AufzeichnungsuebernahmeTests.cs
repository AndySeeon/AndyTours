using Microsoft.Data.Sqlite;
using AndyTours.Altdatenuebernahme;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Altdaten;
using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;
using AndyTours.Tests.Garmin;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests der Übernahme von Aufzeichnungen als Touren (Spezifikation 033): erkennen → Zwischenordner aufbauen →
/// Zielordner austauschen, wie im Programm.
/// </summary>
/// <remarks>
/// Die Aufzeichnungen erzeugt <see cref="FitTestdatei"/>; deshalb laufen die Tests in derselben Testgruppe wie die
/// übrigen FIT-Tests nacheinander.
/// </remarks>
[Collection(FitTestdatei.Testgruppe)]
public class AufzeichnungsuebernahmeTests
{
    /// <summary>Ordner mit Aufzeichnungen: zwei Touren, eine davon doppelt, dazu eine beschädigte Datei.</summary>
    private sealed class Testordner : IDisposable
    {
        private readonly TempOrdner _temp = new();

        public Testordner()
        {
            Quelle = Directory.CreateDirectory(Path.Combine(_temp.Pfad, "Fit")).FullName;
            Ziel = Path.Combine(_temp.Pfad, "AndyTours");

            FitTestdatei.Schreibe(Path.Combine(Quelle, "a.fit"), new FitTestdatei.Werte(new DateTime(2024, 5, 1, 6, 0, 0, DateTimeKind.Utc)));
            Directory.CreateDirectory(Path.Combine(Quelle, "2024", "Juni"));
            FitTestdatei.Schreibe(Path.Combine(Quelle, "2024", "Juni", "b.fit"), new FitTestdatei.Werte(new DateTime(2024, 6, 1, 7, 30, 0, DateTimeKind.Utc)));
            // Dieselbe Tour wie a.fit, z. B. von einem zweiten Gerät; a.fit wird zuerst einsortiert und übernommen
            Directory.CreateDirectory(Path.Combine(Quelle, "Zweitgeraet"));
            FitTestdatei.Schreibe(Path.Combine(Quelle, "Zweitgeraet", "doppelt.fit"), new FitTestdatei.Werte(new DateTime(2024, 5, 1, 6, 0, 0, DateTimeKind.Utc)));
            File.WriteAllBytes(Path.Combine(Quelle, "kaputt.fit"), [1, 2, 3, 4, 5, 6, 7, 8]);
        }

        public string Pfad => _temp.Pfad;

        public string Quelle { get; }

        public string Ziel { get; }

        public void Dispose() => _temp.Dispose();
    }

    /// <summary>Führt die Übernahme so aus wie das Programm (ohne Konsole).</summary>
    private static Aufzeichnungsergebnis Uebernehmen(string quelle, string ziel)
    {
        Altdatenfund fund = ErkenneAltdaten.Ausfuehren(quelle);
        (string zwischenordner, Aufzeichnungsergebnis ergebnis) =
            ErstelleAufzeichnungsordner.Ausfuehren(fund.Aufzeichnungen, quelle, ziel, DateTime.Now);
        TauscheZielordnerAus.Ausfuehren(zwischenordner, ziel);
        return ergebnis;
    }

    /// <summary>Lädt den Datenbestand aus dem Zielordner.</summary>
    private static Datenbestand LadeZiel(string ziel)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(ziel, DatenordnerInhalt.Datenbankdatei));
        return DatenbestandTabellen.Laden(verbindung);
    }

    /// <summary>Alle Dateien eines Ordners mit relativem Pfad und Inhalt – zum Vergleich vorher/nachher.</summary>
    private static Dictionary<string, byte[]> Abbild(string ordner) =>
        Directory.GetFiles(ordner, "*", SearchOption.AllDirectories)
            .ToDictionary(d => Path.GetRelativePath(ordner, d), File.ReadAllBytes);

    [Fact]
    public void Jede_lesbare_Aufzeichnung_wird_einmal_zur_Tour()
    {
        using var t = new Testordner();

        Aufzeichnungsergebnis ergebnis = Uebernehmen(t.Quelle, t.Ziel);

        Assert.Equal(4, ergebnis.Gefunden);
        Assert.Equal(2, ergebnis.Uebernommen);
        Assert.Equal(2, LadeZiel(t.Ziel).Trainingseinheiten.Count);
    }

    [Fact]
    public void Touren_haben_dieselben_Werte_wie_beim_Import_vom_Edge()
    {
        using var t = new Testordner();
        string datei = Path.Combine(t.Quelle, "2024", "Juni", "b.fit");
        Aufzeichnung aufzeichnung = LeseFitAufzeichnung.Ausfuehren(datei).Aufzeichnung!;

        Uebernehmen(t.Quelle, t.Ziel);

        Datenbestand bestand = LadeZiel(t.Ziel);
        Trainingseinheit erwartet = ErstelleImportierteEinheit.Ausfuehren(bestand, aufzeichnung, null, "2024/b.fit");
        Trainingseinheit einheit = bestand.Trainingseinheiten.Single(e => e.Datum == aufzeichnung.Datum);
        Assert.Equal(erwartet.Startzeit, einheit.Startzeit);
        Assert.Equal(erwartet.Fahrzeit.TotalSeconds, einheit.Fahrzeit.TotalSeconds, 0);
        Assert.Equal(erwartet.DistanzKm, einheit.DistanzKm, 2);
        Assert.Equal(erwartet.Hoehenmeter, einheit.Hoehenmeter);
        Assert.Equal(erwartet.MaxHoehe, einheit.MaxHoehe);
        Assert.Equal(erwartet.StartHoehe, einheit.StartHoehe);
        Assert.Equal(erwartet.MaxGeschwindigkeit, einheit.MaxGeschwindigkeit, 2);
        Assert.Equal(erwartet.HfDurchschnitt, einheit.HfDurchschnitt);
        Assert.Equal(erwartet.HfMax, einheit.HfMax);
        Assert.Equal(erwartet.HfMin, einheit.HfMin);
        Assert.Equal(erwartet.Trittfrequenz, einheit.Trittfrequenz);
        Assert.Equal(erwartet.Temperatur, einheit.Temperatur);
        Assert.Equal(erwartet.LeistungWatt, einheit.LeistungWatt, 1);
        Assert.Equal(erwartet.Anhaenge.Select(a => (a.Typ, a.RelativerPfad)), einheit.Anhaenge.Select(a => (a.Typ, a.RelativerPfad)));
    }

    [Fact]
    public void Aufzeichnungen_liegen_im_Jahresordner_des_Datenordners()
    {
        using var t = new Testordner();

        Uebernehmen(t.Quelle, t.Ziel);

        Assert.All(LadeZiel(t.Ziel).Trainingseinheiten.SelectMany(e => e.Anhaenge),
            a => Assert.True(File.Exists(Path.Combine(t.Ziel, a.RelativerPfad)), a.RelativerPfad));
        Assert.True(File.Exists(Path.Combine(t.Ziel, "2024", "a.fit")));
        Assert.True(File.Exists(Path.Combine(t.Ziel, "2024", "b.fit")));
    }

    [Fact]
    public void Touren_haben_kein_Rad_und_die_Hauptaktivitaet_Radfahren()
    {
        using var t = new Testordner();

        Uebernehmen(t.Quelle, t.Ziel);

        Datenbestand bestand = LadeZiel(t.Ziel);
        Aktivitaet aktivitaet = Assert.Single(bestand.Aktivitaeten);
        Assert.Equal("Radfahren", aktivitaet.Name);
        Assert.True(aktivitaet.IstHaupt);
        Assert.Empty(bestand.Equipments);
        Assert.All(bestand.Trainingseinheiten, e =>
        {
            Assert.Null(e.EquipmentId);
            Assert.Equal(aktivitaet.Id, e.AktivitaetId);
        });
    }

    [Fact]
    public void Nicht_lesbare_und_doppelte_Dateien_werden_uebersprungen_und_im_Bericht_genannt()
    {
        using var t = new Testordner();

        Aufzeichnungsergebnis ergebnis = Uebernehmen(t.Quelle, t.Ziel);

        Assert.Equal(["kaputt.fit", "Zweitgeraet" + Path.DirectorySeparatorChar + "doppelt.fit"], ergebnis.Uebersprungen.Select(u => u.Datei).Order());
        Assert.Equal(ErstelleAufzeichnungsordner.GrundDoppelt, ergebnis.Uebersprungen.Single(u => u.Datei.EndsWith("doppelt.fit", StringComparison.Ordinal)).Grund);
        string bericht = File.ReadAllText(Path.Combine(t.Ziel, ErstelleAufzeichnungsbericht.Berichtsdatei));
        Assert.Contains("kaputt.fit: ", bericht);
        Assert.Contains("doppelt.fit: ", bericht);
        Assert.Contains("Übernommene Touren:       2", bericht);
    }

    [Fact]
    public void Ordner_mit_den_Aufzeichnungen_bleibt_unveraendert()
    {
        using var t = new Testordner();
        Dictionary<string, byte[]> vorher = Abbild(t.Quelle);

        Uebernehmen(t.Quelle, t.Ziel);

        Dictionary<string, byte[]> nachher = Abbild(t.Quelle);
        Assert.Equal(vorher.Keys.Order(), nachher.Keys.Order());
        Assert.All(vorher, paar => Assert.Equal(paar.Value, nachher[paar.Key]));
    }

    [Fact]
    public void Fehler_beim_Aufbau_laesst_den_Zielordner_unveraendert()
    {
        using var t = new Testordner();
        Directory.CreateDirectory(t.Ziel);
        // Eine Datei mit dem Namen des Zwischenordners verhindert, dass er angelegt werden kann
        string blockade = Path.Combine(t.Pfad, ".AndyTours.umwandlung");
        File.WriteAllText(blockade, "blockiert");
        Altdatenfund fund = ErkenneAltdaten.Ausfuehren(t.Quelle);

        Assert.ThrowsAny<IOException>(() => ErstelleAufzeichnungsordner.Ausfuehren(fund.Aufzeichnungen, t.Quelle, t.Ziel, DateTime.Now));

        Assert.Empty(Directory.GetFileSystemEntries(t.Ziel));
        Assert.Equal("blockiert", File.ReadAllText(blockade));
    }

    /// <summary>Startet das Umwandlungsprogramm wie das Installationsprogramm und liefert Rückgabecode und Ausgabe.</summary>
    private static (int Code, string Ausgabe) Starte(params string[] argumente)
    {
        var start = new System.Diagnostics.ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "AndyTours.Altdatenuebernahme.exe"))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            UseShellExecute = false,
        };
        foreach (string argument in argumente)
        {
            start.ArgumentList.Add(argument);
        }

        using var prozess = System.Diagnostics.Process.Start(start)!;
        Task<string> fehler = prozess.StandardError.ReadToEndAsync();
        string ausgabe = prozess.StandardOutput.ReadToEnd();
        prozess.WaitForExit();
        return (prozess.ExitCode, ausgabe + fehler.Result);
    }

    [Fact]
    public void Programm_erkennt_Aufzeichnungen_und_nennt_ihre_Anzahl()
    {
        using var t = new Testordner();

        (int code, string ausgabe) = Starte("--erkennen", t.Quelle);

        Assert.Equal(Rueckgabecode.NurAufzeichnungen, code);
        Assert.Equal("4", ausgabe.Trim());
    }

    [Fact]
    public void Programm_meldet_einen_Ordner_ohne_bisherige_Daten()
    {
        using var temp = new TempOrdner();

        (int code, string ausgabe) = Starte("--erkennen", temp.Pfad);

        Assert.Equal(Rueckgabecode.Nichts, code);
        Assert.Equal("0", ausgabe.Trim());
    }

    [Fact]
    public void Programm_uebernimmt_Aufzeichnungen_und_gibt_die_Zusammenfassung_aus()
    {
        using var t = new Testordner();

        (int code, string ausgabe) = Starte("--aufzeichnungen", t.Quelle, t.Ziel, "--ohne-rueckfrage");

        Assert.Equal(Rueckgabecode.Erfolg, code);
        Assert.Contains("Übernommene Touren:    2", ausgabe);
        Assert.Contains("Übersprungene Dateien: 2", ausgabe);
        Assert.Contains(Path.Combine(t.Ziel, ErstelleAufzeichnungsbericht.Berichtsdatei), ausgabe);
        Assert.Equal(2, LadeZiel(t.Ziel).Trainingseinheiten.Count);
    }

    [Fact]
    public void Ohne_Rueckfrage_bleibt_ein_Zielordner_mit_Daten_unveraendert()
    {
        using var t = new Testordner();
        Directory.CreateDirectory(t.Ziel);
        File.WriteAllText(Path.Combine(t.Ziel, DatenordnerInhalt.Datenbankdatei), "alter Bestand");

        (int code, _) = Starte("--aufzeichnungen", t.Quelle, t.Ziel, "--ohne-rueckfrage");

        Assert.Equal(Rueckgabecode.ZielHatDaten, code);
        Assert.Equal("alter Bestand", File.ReadAllText(Path.Combine(t.Ziel, DatenordnerInhalt.Datenbankdatei)));
        Assert.Single(Directory.GetFileSystemEntries(t.Ziel));
    }
}

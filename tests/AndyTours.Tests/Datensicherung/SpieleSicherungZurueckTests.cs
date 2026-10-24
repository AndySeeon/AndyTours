using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="SpieleSicherungZurueck"/>.
/// </summary>
/// <remarks>
/// Jeder Test legt einen Datenordner mit 5 Einheiten „Aktuell“ an; die Sicherungen enthalten
/// 2 Einheiten „Alt“. So ist nach dem Test eindeutig erkennbar, welcher Stand gilt.
/// </remarks>
public class SpieleSicherungZurueckTests
{
    /// <summary>Zeitpunkt des Wiederherstellens (Name der zusätzlichen Sicherung).</summary>
    private static readonly DateTime Jetzt = new(2026, 10, 24, 10, 0, 0);

    /// <summary>Legt den Datenordner mit dem aktuellen Stand an.</summary>
    private static void LegeAktuellenStandAn(string datenordner) =>
        TestDatenbestand.SpeichereIn(datenordner, TestDatenbestand.Erzeuge(5, "Aktuell"));

    /// <summary>Pfad einer Sicherung vom 1. Oktober 2026 um 8 Uhr plus <paramref name="tage"/> Tage im Ordner.</summary>
    private static string Sicherungspfad(string ordner, int tage)
    {
        Directory.CreateDirectory(ordner);
        return Path.Combine(ordner, "andytours_"
            + new DateTime(2026, 10, 1, 8, 0, 0).AddDays(tage).ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) + ".db");
    }

    /// <summary>Standard-Sicherungsordner des Datenordners.</summary>
    private static string Standardordner(string datenordner) => Path.Combine(datenordner, "Sicherungen");

    /// <summary>Namen der Einheiten im Datenordner (zeigt, welcher Stand gilt).</summary>
    private static List<string> Einheiten(string datenordner) =>
        TestDatenbestand.Lade(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei)).Trainingseinheiten.Select(e => e.Name).ToList();

    /// <summary>Speichert Einstellungen in der Datenbank des Datenordners.</summary>
    private static void Stelle(string datenordner, Sicherungseinstellungen einstellungen)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei));
        SpeichereSicherungseinstellungen.Ausfuehren(verbindung, einstellungen);
    }

    /// <summary>Liest die Einstellungen aus der Datenbank des Datenordners.</summary>
    private static Sicherungseinstellungen Lies(string datenordner)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei));
        return LadeSicherungseinstellungen.Ausfuehren(verbindung);
    }

    [Fact]
    public void Nach_dem_Wiederherstellen_gilt_der_Stand_der_Sicherung()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        TestSicherungen.Gueltig(sicherung, 2, "Alt");

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(Wiederherstellungsstatus.Wiederhergestellt, ergebnis.Status);
        Datenbestand bestand = TestDatenbestand.Lade(Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei));
        Assert.Equal(["Alt", "Alt"], bestand.Trainingseinheiten.Select(e => e.Name));
        Assert.Equal("Genius", Assert.Single(bestand.Equipments).Name);
        Assert.Equal("Kette gewechselt", Assert.Single(bestand.Wartungen).Taetigkeit);
        Assert.False(File.Exists(Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei + SpieleSicherungZurueck.Zwischenendung)));
    }

    [Fact]
    public void Vorher_wird_der_aktuelle_Stand_zusaetzlich_gesichert()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        TestSicherungen.Gueltig(sicherung, 2, "Alt");

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(Path.Combine(Standardordner(temp.Pfad), "andytours_2026-10-24_10-00-00.db"), ergebnis.ZusaetzlicheSicherung);
        Assert.Equal(Enumerable.Repeat("Aktuell", 5), TestDatenbestand.Lade(ergebnis.ZusaetzlicheSicherung!).Trainingseinheiten.Select(e => e.Name));
    }

    [Fact]
    public void Die_zusaetzliche_Sicherung_zaehlt_zur_Hoechstzahl()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        Stelle(temp.Pfad, new Sicherungseinstellungen { Hoechstzahl = 2 });
        string aelteste = Sicherungspfad(Standardordner(temp.Pfad), 0);
        string neuere = Sicherungspfad(Standardordner(temp.Pfad), 1);
        TestSicherungen.Gueltig(aelteste, 2, "Alt");
        TestSicherungen.Gueltig(neuere, 2, "Alt");

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, neuere, Jetzt);

        Assert.Equal(Wiederherstellungsstatus.Wiederhergestellt, ergebnis.Status);
        Assert.Equal([neuere, ergebnis.ZusaetzlicheSicherung], Directory.GetFiles(Standardordner(temp.Pfad)).Order());
    }

    [Fact]
    public void Einstellungen_der_Datensicherung_bleiben_erhalten()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string eigenerOrdner = Path.Combine(temp.Pfad, "Eigener Ordner");
        string sicherung = Sicherungspfad(eigenerOrdner, 0);
        TestSicherungen.Gueltig(sicherung, 2, "Alt"); // enthält die Standardwerte 3 / 10 und keinen Ordner
        Stelle(temp.Pfad, new Sicherungseinstellungen { Sicherungsordner = eigenerOrdner, Intervall = 5, Hoechstzahl = 7, StartsSeitSicherung = 4 });

        SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(new Sicherungseinstellungen { Sicherungsordner = eigenerOrdner, Intervall = 5, Hoechstzahl = 7, StartsSeitSicherung = 0 },
            Lies(temp.Pfad));
        Assert.Equal(["Alt", "Alt"], Einheiten(temp.Pfad));
    }

    [Fact]
    public void Scheitert_die_zusaetzliche_Sicherung_wird_nichts_wiederhergestellt()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string ordner = Standardordner(temp.Pfad);
        string sicherung = Sicherungspfad(ordner, 0);
        TestSicherungen.Gueltig(sicherung, 2, "Alt");

        // Dem aktuellen Benutzer das Anlegen von Dateien im Sicherungsordner verweigern
        var info = new DirectoryInfo(ordner);
        var verbot = new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!,
            FileSystemRights.CreateFiles | FileSystemRights.WriteData | FileSystemRights.AppendData,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
        DirectorySecurity rechte = info.GetAccessControl();
        rechte.AddAccessRule(verbot);
        info.SetAccessControl(rechte);

        try
        {
            Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

            Assert.Equal(Wiederherstellungsstatus.SicherungFehlgeschlagen, ergebnis.Status);
            Assert.Contains("nicht schreiben", ergebnis.Fehlergrund);
            Assert.Equal(Enumerable.Repeat("Aktuell", 5), Einheiten(temp.Pfad));
        }
        finally
        {
            rechte.RemoveAccessRule(verbot);
            info.SetAccessControl(rechte);
        }
    }

    [Fact]
    public void Unbrauchbare_Sicherung_aendert_nichts()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        File.WriteAllText(sicherung, "keine Datenbank");

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(Wiederherstellungsstatus.Unbrauchbar, ergebnis.Status);
        Assert.Equal(Enumerable.Repeat("Aktuell", 5), Einheiten(temp.Pfad));
        Assert.Equal([sicherung], Directory.GetFiles(Standardordner(temp.Pfad))); // keine zusätzliche Sicherung
    }

    [Fact]
    public void Sicherung_einer_neueren_Version_wird_abgelehnt()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        TestSicherungen.MitVersion(sicherung, ErstelleSchema.Schemaversion + 1);

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(Wiederherstellungsstatus.NeuereVersion, ergebnis.Status);
        Assert.Equal(Enumerable.Repeat("Aktuell", 5), Einheiten(temp.Pfad));
        Assert.Equal([sicherung], Directory.GetFiles(Standardordner(temp.Pfad)));
    }

    [Fact]
    public void Scheitert_der_Austausch_bleibt_der_bisherige_Bestand_erhalten()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        TestSicherungen.Gueltig(sicherung, 2, "Alt");
        string datenbankdatei = Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei);

        Wiederherstellungsergebnis ergebnis;
        // Ein „anderes Programm“ hält die Datenbank offen: Lesen und Schreiben ist erlaubt, Ersetzen nicht
        using (new FileStream(datenbankdatei, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);
        }

        Assert.Equal(Wiederherstellungsstatus.Fehlgeschlagen, ergebnis.Status);
        Assert.NotNull(ergebnis.ZusaetzlicheSicherung); // Prüfen und Sichern sind gelungen, erst der Austausch scheitert
        Assert.Contains("anderen Programm", ergebnis.Fehlergrund);
        Assert.Equal(Enumerable.Repeat("Aktuell", 5), Einheiten(temp.Pfad));
        Assert.False(File.Exists(datenbankdatei + SpieleSicherungZurueck.Zwischenendung));
    }

    [Fact]
    public void Sicherung_einer_aelteren_Version_wird_uebernommen_und_ist_nutzbar()
    {
        using var temp = new TempOrdner();
        LegeAktuellenStandAn(temp.Pfad);
        string sicherung = Sicherungspfad(Standardordner(temp.Pfad), 0);
        TestSicherungen.Version1(sicherung, 2, "Alt");

        Wiederherstellungsergebnis ergebnis = SpieleSicherungZurueck.Ausfuehren(temp.Pfad, sicherung, Jetzt);

        Assert.Equal(Wiederherstellungsstatus.Wiederhergestellt, ergebnis.Status);
        Assert.Equal(["Alt", "Alt"], Einheiten(temp.Pfad));
        Assert.Equal(Sicherungspruefung.Brauchbar, PruefeSicherung.Ausfuehren(Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei)));

        // Einstellungen lassen sich speichern – die Tabelle aus Schemaversion 2 ist also vorhanden
        Stelle(temp.Pfad, Lies(temp.Pfad) with { Intervall = 9 });
        Assert.Equal(9, Lies(temp.Pfad).Intervall);
        using var verbindung = new SqliteConnection($"Data Source={Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei)};Pooling=False");
        verbindung.Open();
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "PRAGMA user_version;";
        Assert.Equal((long)ErstelleSchema.Schemaversion, befehl.ExecuteScalar());
    }
}

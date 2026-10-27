using System.Text;
using Microsoft.Data.Sqlite;
using AndyTours.Altdatenuebernahme;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Altdaten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests des Gesamtablaufs der Umwandlung mit den künstlichen <see cref="TestAltdaten"/>:
/// lesen → Zwischenordner aufbauen → Zielordner austauschen (wie in <c>Program.cs</c>).
/// </summary>
public class UmwandlungTests
{
    /// <summary>Führt die Umwandlung so aus wie das Programm (ohne Konsole).</summary>
    private static Migrationsergebnis Umwandeln(string quelle, string ziel)
    {
        Migrationsergebnis ergebnis = UebernehmeAltdaten.Ausfuehren(quelle);
        string zwischenordner = ErstelleZwischenordner.Ausfuehren(ergebnis, quelle, ziel);
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
    public void Alle_Daten_landen_im_Zielordner()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");
        TestAltdaten.Erzeuge(quelle);

        Umwandeln(quelle, ziel);
        Datenbestand bestand = LadeZiel(ziel);

        // Zwei gültige Einheiten (auch die ohne Aufzeichnung), die kaputte Zeile fehlt
        Assert.Equal(["Ohne Aufzeichnung", "Stoisser Bäckeralm"], bestand.Trainingseinheiten.Select(e => e.Name).Order());
        Assert.Single(bestand.Wartungen);
        Assert.Contains(bestand.Equipments, e => e.Name == "Namur");
        Assert.Contains(bestand.Aktivitaeten, a => a.Name == "Bike");
        Assert.Contains(bestand.Kategorien, k => k.Name == "Training");
        Assert.Single(bestand.Schwellwerte);
        Assert.Equal("Testfahrer", Assert.Single(bestand.Fahrer).Vorname);
    }

    [Fact]
    public void Vorhandene_Aufzeichnungen_liegen_im_Ziel_unter_ihrem_Anhangpfad()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");
        TestAltdaten.Erzeuge(quelle);

        Umwandeln(quelle, ziel);

        Trainingseinheit einheit = LadeZiel(ziel).Trainingseinheiten.Single(e => e.Name == "Stoisser Bäckeralm");
        Assert.All(einheit.Anhaenge, a => Assert.True(File.Exists(Path.Combine(ziel, a.RelativerPfad)), a.RelativerPfad));
        Assert.Equal([1, 2, 3, 4, 5], File.ReadAllBytes(Path.Combine(ziel, TestAltdaten.VorhandeneAufzeichnung)));
    }

    [Fact]
    public void Bericht_nennt_Jahressummen_fehlende_Aufzeichnung_und_fehlerhafte_Zeile()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");
        TestAltdaten.Erzeuge(quelle);

        Umwandeln(quelle, ziel);
        string bericht = File.ReadAllText(Path.Combine(ziel, ErstelleZwischenordner.Berichtsdatei));

        Assert.Contains("Übernommen:", bericht);
        Assert.Contains("Jahressummen", bericht);
        Assert.Contains("Hinweise", bericht);
        Assert.Contains("Nicht reparierte Umlaute", bericht);
        Assert.Contains(TestAltdaten.FehlendeAufzeichnung, bericht);
        Assert.Contains("Zeile 4", bericht);
    }

    [Fact]
    public void Zusammenfassung_nennt_Mengen_Warnungen_und_Bericht()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");
        TestAltdaten.Erzeuge(quelle);

        Migrationsergebnis ergebnis = Umwandeln(quelle, ziel);
        string text = ErstelleZusammenfassung.Ausfuehren(ergebnis, ziel);

        Assert.Contains("Trainingseinheiten: 2", text);
        Assert.Contains($"Warnungen: {ergebnis.Bericht.Warnungen.Count}", text);
        Assert.Contains(Path.Combine(ziel, ErstelleZwischenordner.Berichtsdatei), text);
    }

    [Fact]
    public void Zugangsdaten_stehen_in_keiner_Datei_des_Zielordners()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");
        TestAltdaten.Erzeuge(quelle);

        Umwandeln(quelle, ziel);

        foreach (string datei in Directory.GetFiles(ziel, "*", SearchOption.AllDirectories))
        {
            // SQLite speichert Texte unverschlüsselt als UTF-8 – ein übernommenes Passwort wäre hier lesbar
            string inhalt = Encoding.UTF8.GetString(File.ReadAllBytes(datei));
            Assert.DoesNotContain(TestAltdaten.Passwort, inhalt);
            Assert.DoesNotContain(TestAltdaten.DropboxSchluessel, inhalt);
        }
    }

    [Fact]
    public void BikeXperience_Ordner_bleibt_unveraendert()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        TestAltdaten.Erzeuge(quelle);
        Dictionary<string, byte[]> vorher = Abbild(quelle);

        Umwandeln(quelle, Path.Combine(temp.Pfad, "AndyTours"));

        Dictionary<string, byte[]> nachher = Abbild(quelle);
        Assert.Equal(vorher.Keys.Order(), nachher.Keys.Order());
        Assert.All(vorher, paar => Assert.Equal(paar.Value, nachher[paar.Key]));
    }

    [Fact]
    public void Ordner_ohne_BikeXperience_Daten_wird_gemeldet_und_nichts_geschrieben()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "Leer")).FullName;
        string ziel = Path.Combine(temp.Pfad, "AndyTours");

        Assert.Throws<FileNotFoundException>(() => Umwandeln(quelle, ziel));

        Assert.False(Directory.Exists(ziel));
        Assert.Equal([quelle], Directory.GetDirectories(temp.Pfad));
    }

    [Fact]
    public void Fehler_beim_Aufbau_laesst_den_vorhandenen_Zielordner_unveraendert()
    {
        using var temp = new TempOrdner();
        string quelle = Directory.CreateDirectory(Path.Combine(temp.Pfad, "BikeXperience")).FullName;
        string ziel = Directory.CreateDirectory(Path.Combine(temp.Pfad, "AndyTours")).FullName;
        File.WriteAllText(Path.Combine(ziel, DatenordnerInhalt.Datenbankdatei), "alter Bestand");
        TestAltdaten.Erzeuge(quelle);

        // Gesperrte Aufzeichnung: Das Kopieren in den Zwischenordner schlägt fehl
        using (File.Open(Path.Combine(quelle, TestAltdaten.VorhandeneAufzeichnung), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.ThrowsAny<IOException>(() => Umwandeln(quelle, ziel));
        }

        Assert.Equal("alter Bestand", File.ReadAllText(Path.Combine(ziel, DatenordnerInhalt.Datenbankdatei)));
        // Kein Zwischenordner bleibt zurück: nur Quelle und Ziel liegen nebeneinander
        Assert.Equal(new[] { quelle, ziel }.Order(), Directory.GetDirectories(temp.Pfad).Order());
    }
}

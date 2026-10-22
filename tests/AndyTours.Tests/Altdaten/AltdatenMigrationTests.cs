using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Import.Altdaten;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Abnahmetest der Phase 1: Übernahme der echten Altdaten aus dem Ordner <c>Daten/</c>.
/// </summary>
/// <remarks>
/// Der Ordner <c>Daten/</c> enthält persönliche Daten und ist nicht im Git-Repository. Fehlt er
/// (z. B. auf einem anderen Rechner), enden diese Tests ohne Prüfung. Die Vergleichswerte werden
/// bewusst unabhängig vom Importcode direkt aus der Datei ermittelt, damit ein Fehler im
/// Import nicht durch denselben Fehler im Test verdeckt wird.
/// </remarks>
public class AltdatenMigrationTests
{
    /// <summary>
    /// Sucht den Ordner <c>Daten/</c>, indem vom Ausgabeordner der Tests aufwärts gegangen wird
    /// (tests/AndyTours.Tests/bin/Debug/net10.0 → Projektordner).
    /// </summary>
    private static string? FindeDatenordner()
    {
        for (var ordner = new DirectoryInfo(AppContext.BaseDirectory); ordner is not null; ordner = ordner.Parent)
        {
            string kandidat = Path.Combine(ordner.FullName, "Daten");
            if (File.Exists(Path.Combine(kandidat, "workouts.csv")))
            {
                return kandidat;
            }
        }

        return null;
    }

    /// <summary>
    /// Einfache, vom Import unabhängige Auswertung der workouts.csv: Anzahl, km und hm je Jahr.
    /// </summary>
    private static Dictionary<int, (int Anzahl, double Km, int Hm)> ZaehleDirektAusCsv(string datenordner)
    {
        var summen = new Dictionary<int, (int Anzahl, double Km, int Hm)>();
        foreach (string zeile in File.ReadLines(Path.Combine(datenordner, "workouts.csv")).Skip(1))
        {
            string[] f = zeile.Split(';');
            int jahr = int.Parse(f[0][6..10], CultureInfo.InvariantCulture);   // TT.MM.JJJJ → JJJJ
            summen.TryGetValue(jahr, out var bisher);
            summen[jahr] = (bisher.Anzahl + 1,
                            bisher.Km + double.Parse(f[3], CultureInfo.InvariantCulture),
                            bisher.Hm + int.Parse(f[4], CultureInfo.InvariantCulture));
        }

        return summen;
    }

    [Fact]
    public void Alle_Altdaten_werden_vollstaendig_uebernommen()
    {
        string? datenordner = FindeDatenordner();
        if (datenordner is null)
        {
            return; // keine Altdaten auf diesem Rechner
        }

        Migrationsergebnis ergebnis = UebernehmeAltdaten.Ausfuehren(datenordner);
        Datenbestand bestand = ergebnis.Bestand;

        // Erwartete Mengen laut Analyse: 932 Einheiten, 18 Equipments in der INI
        int erwarteteEinheiten = int.Parse(File.ReadLines(Path.Combine(datenordner, "workouts.csv")).First(), CultureInfo.InvariantCulture);
        int erwarteteWartungen = File.ReadLines(Path.Combine(datenordner, "maintanence.dat")).Count(z => z.Trim().Length > 0);

        Assert.Equal(erwarteteEinheiten, bestand.Trainingseinheiten.Count);
        Assert.Equal(erwarteteWartungen, bestand.Wartungen.Count);
        Assert.Equal(18, LeseBikexperienceIni.Ausfuehren(Path.Combine(datenordner, "bikexperience.ini")).Equipments.Count);

        // Keine übersprungenen Zeilen
        Assert.DoesNotContain(ergebnis.Bericht.Warnungen, w => w.Contains("übersprungen"));

        // Alle beschädigten Umlaute sind repariert (zuletzt „Bäckeralm" und „ÜPD", Spezifikation 002)
        Assert.Empty(ergebnis.Bericht.NichtReparierteTexte);
        Assert.Contains(bestand.Trainingseinheiten, e => e.Name == "Stoisser Bäckeralm");
        Assert.Contains(bestand.Trainingseinheiten, e => e.Name.Contains("(ÜPD)"));

        // Jede Einheit und jede Wartung (außer „-") hat ihre Zuordnung behalten
        Assert.All(bestand.Trainingseinheiten, e => Assert.NotNull(e.EquipmentId));
        Assert.All(bestand.Wartungen, w => Assert.NotNull(w.EquipmentId));
    }

    [Fact]
    public void Jahressummen_stimmen_nach_Speichern_und_Laden_mit_dem_Original_ueberein()
    {
        string? datenordner = FindeDatenordner();
        if (datenordner is null)
        {
            return;
        }

        Migrationsergebnis ergebnis = UebernehmeAltdaten.Ausfuehren(datenordner);

        // Über eine echte Datenbankdatei im Temp-Ordner speichern und wieder laden
        string dbPfad = Path.Combine(Path.GetTempPath(), $"andytours-test-{Guid.NewGuid():N}.db");
        try
        {
            Datenbestand geladen;
            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(dbPfad))
            {
                SpeichereDatenbestand.Ausfuehren(verbindung, ergebnis.Bestand);
                geladen = LadeDatenbestand.Ausfuehren(verbindung);
            }

            var original = ZaehleDirektAusCsv(datenordner);
            List<Jahressumme> jahressummen = BerechneJahressummen.Ausfuehren(geladen.Trainingseinheiten);

            Assert.Equal(original.Keys.OrderBy(j => j), jahressummen.Select(s => s.Jahr));
            foreach (Jahressumme summe in jahressummen)
            {
                Assert.Equal(original[summe.Jahr].Anzahl, summe.Anzahl);
                Assert.Equal(original[summe.Jahr].Km, summe.DistanzKm, precision: 2);
                Assert.Equal(original[summe.Jahr].Hm, summe.Hoehenmeter);
            }
        }
        finally
        {
            File.Delete(dbPfad);
        }
    }
}

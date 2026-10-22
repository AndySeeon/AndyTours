using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using AndyTours.Daten;
using AndyTours.Import.Altdaten;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Übernimmt die BikeXperience-Altdaten eines Ordners in eine neue AndyTours-Datenbank.
/// </summary>
public static class FuehreAltdatenuebernahmeDurch
{
    /// <summary>Dateiname der AndyTours-Datenbank im Datenordner.</summary>
    public const string Datenbankdatei = "andytours.db";

    /// <summary>Dateiname des Migrationsberichts im Datenordner.</summary>
    public const string Berichtsdatei = "migrationsbericht.txt";

    /// <summary>
    /// Liest die Altdaten, legt die Datenbank neu an, speichert alles und schreibt den Bericht.
    /// </summary>
    /// <remarks>
    /// Eine vorhandene <c>andytours.db</c> wird ersetzt – der Aufrufer muss das vorher bestätigen lassen.
    /// Die Altdateien selbst bleiben unverändert. Die Datenbank liegt im selben Ordner wie die
    /// FIT-Dateien, weil die Anhangpfade relativ zu diesem Ordner gespeichert sind.
    /// </remarks>
    /// <param name="datenordner">Ordner mit den BikeXperience-Dateien.</param>
    /// <returns>Das Ergebnis der Übernahme (für die Zusammenfassung in der Oberfläche).</returns>
    public static Migrationsergebnis Ausfuehren(string datenordner)
    {
        // Erst vollständig lesen; schlägt das fehl, bleibt eine vorhandene Datenbank unangetastet.
        Migrationsergebnis ergebnis = UebernehmeAltdaten.Ausfuehren(datenordner);

        string dbPfad = Path.Combine(datenordner, Datenbankdatei);
        File.Delete(dbPfad); // löscht nur, wenn vorhanden

        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(dbPfad))
        {
            SpeichereDatenbestand.Ausfuehren(verbindung, ergebnis.Bestand);
        }

        // Bericht mit Byte-Order-Mark speichern, damit auch der Windows-Editor die Umlaute richtig anzeigt.
        File.WriteAllText(Path.Combine(datenordner, Berichtsdatei), ErstelleBerichtstext.Ausfuehren(ergebnis), Encoding.UTF8);

        return ergebnis;
    }
}

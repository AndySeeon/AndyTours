using System.Text;
using Microsoft.Data.Sqlite;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Altdaten;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Baut den umgewandelten Datenbestand vollständig in einem Zwischenordner neben dem Ziel auf.
/// </summary>
public static class ErstelleZwischenordner
{
    /// <summary>Dateiname des Berichts im Datenordner.</summary>
    public const string Berichtsdatei = "migrationsbericht.txt";

    /// <summary>
    /// Legt den Zwischenordner an und schreibt Datenbank, Bericht und Aufzeichnungen hinein.
    /// </summary>
    /// <remarks>
    /// Warum ein Zwischenordner? Der Zielordner soll erst angetastet werden, wenn wirklich alles
    /// fertig ist (siehe <see cref="TauscheZielordnerAus"/>). Bricht etwas ab, bleibt das Ziel
    /// unverändert. Der Zwischenordner liegt direkt neben dem Ziel – also auf demselben Laufwerk –,
    /// damit der spätere Austausch ein schnelles Umbenennen ist.
    /// <para>
    /// Name: <c>.&lt;Zielname&gt;.umwandlung</c> (der Punkt am Anfang kennzeichnet ihn als Hilfsordner).
    /// Ein Rest eines früheren, abgebrochenen Laufs wird vorher entfernt. Bei einem Fehler wird der
    /// Zwischenordner wieder gelöscht und der Fehler weitergegeben.
    /// </para>
    /// </remarks>
    /// <param name="ergebnis">Die bereits gelesenen und umgewandelten Altdaten.</param>
    /// <param name="quellordner">BikeXperience-Ordner (für die Aufzeichnungen).</param>
    /// <param name="zielordner">Späterer Zielordner; bestimmt Ort und Namen des Zwischenordners.</param>
    /// <returns>Vollständiger Pfad des fertigen Zwischenordners.</returns>
    public static string Ausfuehren(Migrationsergebnis ergebnis, string quellordner, string zielordner)
    {
        string zwischenordner = LegeLeerAn(zielordner);
        try
        {
            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(zwischenordner, DatenordnerInhalt.Datenbankdatei)))
            {
                DatenbestandTabellen.Speichern(verbindung, ergebnis.Bestand);
            }

            // Bericht mit Byte-Order-Mark, damit auch der Windows-Editor die Umlaute richtig anzeigt
            File.WriteAllText(Path.Combine(zwischenordner, Berichtsdatei), ErstelleBerichtstext.Ausfuehren(ergebnis), Encoding.UTF8);

            KopiereAufzeichnungen.Ausfuehren(quellordner, zwischenordner);
        }
        catch
        {
            // Halbfertigen Zwischenordner nicht liegen lassen
            try
            {
                Directory.Delete(zwischenordner, recursive: true);
            }
            catch (Exception aufraeumen) when (aufraeumen is IOException or UnauthorizedAccessException)
            {
                // Die ursprüngliche Fehlermeldung ist wichtiger; der Rest wird beim nächsten Lauf entfernt.
            }

            throw;
        }

        return zwischenordner;
    }

    /// <summary>
    /// Legt den leeren Zwischenordner neben dem Zielordner an; ein Rest eines früheren Laufs wird vorher entfernt.
    /// </summary>
    /// <remarks>
    /// Wird auch von <see cref="ErstelleAufzeichnungsordner"/> verwendet, damit beide Wege denselben
    /// Zwischenordner nutzen und ihn auf dieselbe Weise gegen das Ziel austauschen.
    /// </remarks>
    /// <param name="zielordner">Späterer Zielordner; bestimmt Ort und Namen des Zwischenordners.</param>
    /// <returns>Vollständiger Pfad des leeren Zwischenordners <c>.&lt;Zielname&gt;.umwandlung</c>.</returns>
    /// <exception cref="IOException">Der Zielordner ist ein Laufwerk.</exception>
    public static string LegeLeerAn(string zielordner)
    {
        string ziel = Path.TrimEndingDirectorySeparator(zielordner);
        string uebergeordnet = Path.GetDirectoryName(ziel)
            ?? throw new IOException($"Ein Laufwerk ({zielordner}) kann nicht als Zielordner dienen; bitte einen Unterordner angeben.");
        string zwischenordner = Path.Combine(uebergeordnet, $".{Path.GetFileName(ziel)}.umwandlung");

        if (Directory.Exists(zwischenordner))
        {
            Directory.Delete(zwischenordner, recursive: true);
        }

        Directory.CreateDirectory(zwischenordner);
        return zwischenordner;
    }
}

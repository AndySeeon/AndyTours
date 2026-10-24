using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten.Datenordner;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Setzt den Datenbestand eines Datenordners auf den Stand einer Sicherung zurück.
/// </summary>
public static class SpieleSicherungZurueck
{
    /// <summary>
    /// Zusätzliche Endung der Zwischendatei im Datenordner, in der die neue Datenbank vorbereitet wird.
    /// </summary>
    public const string Zwischenendung = ".wiederherstellen.tmp";

    /// <summary>SQLite-Fehlercode SQLITE_CANTOPEN: Die Datei lässt sich nicht öffnen (z. B. nicht erreichbar).</summary>
    private const int SqliteNichtZuOeffnen = 14;

    /// <summary>
    /// Spielt die Sicherung <paramref name="sicherung"/> in den Datenordner zurück.
    /// </summary>
    /// <remarks>
    /// Ablauf – bis einschließlich Schritt 3 bleibt die Datenbank im Datenordner unberührt:
    /// <list type="number">
    /// <item><b>Prüfen</b> (<see cref="PruefeSicherung"/>): beschädigte, fremde oder neuere Dateien werden
    /// abgelehnt, bevor irgendetwas geschieht.</item>
    /// <item><b>Aktuellen Stand sichern</b> (<see cref="ErstelleSicherung"/>) in den Sicherungsordner. So lässt
    /// sich auch das Wiederherstellen rückgängig machen. Schlägt das fehl, wird abgebrochen.</item>
    /// <item><b>Vorbereiten:</b> Die Sicherung wird per Online-Sicherung von SQLite in eine Zwischendatei im
    /// Datenordner kopiert. Beim Öffnen mit <see cref="OeffneDatenbank"/> erhält eine ältere Sicherung
    /// fehlende Tabellen und die aktuelle Schemaversion. Danach werden die vorher gelesenen
    /// Sicherungseinstellungen hineingeschrieben – sie gelten unverändert weiter; nur der Startzähler
    /// steht auf 0, weil gerade gesichert wurde.</item>
    /// <item><b>Austauschen:</b> Die Zwischendatei ersetzt die Datenbank in einem Schritt
    /// (<see cref="File.Move(string, string, bool)"/>). Geht dabei oder vorher etwas schief, wird die
    /// Zwischendatei entfernt und der bisherige Bestand bleibt vollständig erhalten.</item>
    /// <item><b>Aufräumen</b> (<see cref="EntferneAlteSicherungen"/>) erst jetzt, damit die gewählte Sicherung
    /// nicht vor dem Zurückspielen gelöscht wird. Die zusätzliche Sicherung zählt dabei wie jede andere zur
    /// Höchstzahl. Fehler beim Aufräumen werden ignoriert – die nächste Sicherung räumt erneut auf.</item>
    /// </list>
    /// Erwartete Fehler (Datei-, Rechte-, Datenbankfehler) werden nicht geworfen, sondern im Ergebnis gemeldet.
    /// Die Methode läuft synchron; die App ruft sie im Hintergrund auf.
    /// </remarks>
    /// <param name="datenordner">Der aktive Datenordner.</param>
    /// <param name="sicherung">Vollständiger Pfad der zurückzuspielenden Sicherung.</param>
    /// <param name="zeitpunkt">Zeitpunkt für den Namen der zusätzlichen Sicherung (in der App: jetzt).</param>
    /// <returns>Das Ergebnis mit Status, zusätzlicher Sicherung und ggf. Fehlergrund.</returns>
    public static Wiederherstellungsergebnis Ausfuehren(string datenordner, string sicherung, DateTime zeitpunkt)
    {
        string datenbankdatei = Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei);
        string zwischendatei = datenbankdatei + Zwischenendung;
        string? zusaetzlicheSicherung = null;

        try
        {
            // 1. Prüfen
            switch (PruefeSicherung.Ausfuehren(sicherung))
            {
                case Sicherungspruefung.Unbrauchbar:
                    return new Wiederherstellungsergebnis(Wiederherstellungsstatus.Unbrauchbar);
                case Sicherungspruefung.NeuereVersion:
                    return new Wiederherstellungsergebnis(Wiederherstellungsstatus.NeuereVersion);
            }

            // Die aktuellen Einstellungen werden für Schritt 2 (Ordner) und Schritt 3 (Übernahme) gebraucht
            Sicherungseinstellungen einstellungen;
            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datenbankdatei))
            {
                einstellungen = SicherungEinstellungen.Laden(verbindung);
            }

            string sicherungsordner = ErmittleSicherungsordner.Ausfuehren(datenordner, einstellungen);

            // 2. Aktuellen Stand sichern
            try
            {
                zusaetzlicheSicherung = ErstelleSicherung.Ausfuehren(datenbankdatei, sicherungsordner, zeitpunkt);
            }
            catch (Exception fehler) when (IstErwartet(fehler))
            {
                return new Wiederherstellungsergebnis(Wiederherstellungsstatus.SicherungFehlgeschlagen,
                    Fehlergrund: BeschreibeSicherungsfehler.Ausfuehren(fehler, FuehreStartsicherungDurch.StandardZeitlimit));
            }

            // 3. Vorbereiten
            LoescheZwischendatei(zwischendatei); // Rest eines früher abgebrochenen Versuchs
            using (var quelle = new SqliteConnection(Verbindungstext(sicherung, SqliteOpenMode.ReadOnly)))
            using (var ziel = new SqliteConnection(Verbindungstext(zwischendatei, SqliteOpenMode.ReadWriteCreate)))
            {
                quelle.Open();
                ziel.Open();
                quelle.BackupDatabase(ziel);
            }

            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(zwischendatei))
            {
                SicherungEinstellungen.Speichern(verbindung, einstellungen with { StartsSeitSicherung = 0 });
            }

            // 4. Austauschen
            File.Move(zwischendatei, datenbankdatei, overwrite: true);

            // 5. Aufräumen
            try
            {
                EntferneAlteSicherungen.Ausfuehren(sicherungsordner, einstellungen.Hoechstzahl);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                // Nicht kritisch: Die nächste Sicherung beim Programmstart räumt erneut auf
            }

            return new Wiederherstellungsergebnis(Wiederherstellungsstatus.Wiederhergestellt, zusaetzlicheSicherung);
        }
        catch (Exception fehler) when (IstErwartet(fehler))
        {
            LoescheZwischendatei(zwischendatei);
            return new Wiederherstellungsergebnis(Wiederherstellungsstatus.Fehlgeschlagen, zusaetzlicheSicherung,
                BeschreibeFehler(fehler));
        }
    }

    /// <summary><c>true</c> für die Fehlerarten, mit denen beim Zugriff auf Dateien und Datenbanken zu rechnen ist.</summary>
    private static bool IstErwartet(Exception fehler) =>
        fehler is IOException or UnauthorizedAccessException or SqliteException;

    /// <summary>Verständliche Beschreibung eines Fehlers beim Lesen der Sicherung oder beim Austausch der Datenbank.</summary>
    private static string BeschreibeFehler(Exception fehler) => fehler switch
    {
        IOException io when io.HResult is BeschreibeSicherungsfehler.DatentraegerVoll or BeschreibeSicherungsfehler.DatentraegerVollHandle =>
            "Auf dem Datenträger des Datenordners ist nicht genug Platz.",
        SqliteException { SqliteErrorCode: BeschreibeSicherungsfehler.SqliteVoll } =>
            "Auf dem Datenträger des Datenordners ist nicht genug Platz.",
        // Windows meldet „Zugriff verweigert“ auch dann, wenn ein anderes Programm die Datei geöffnet hat
        UnauthorizedAccessException =>
            "Die Datenbank im Datenordner konnte nicht ersetzt werden: Sie wird gerade von einem anderen Programm verwendet, oder AndyTours fehlen die Rechte dazu.",
        DirectoryNotFoundException or DriveNotFoundException or FileNotFoundException or SqliteException { SqliteErrorCode: SqliteNichtZuOeffnen } =>
            "Die Sicherung oder der Datenordner ist nicht erreichbar.",
        IOException =>
            $"Die Datenbank im Datenordner konnte nicht ersetzt werden (wird sie gerade von einem anderen Programm verwendet?).\n\n{fehler.Message}",
        _ => $"Unerwarteter Fehler: {fehler.Message}",
    };

    /// <summary>Verbindungstext für eine Datei mit der angegebenen Zugriffsart, ohne Verbindungs-Wiederverwendung.</summary>
    private static string Verbindungstext(string datei, SqliteOpenMode modus) =>
        new SqliteConnectionStringBuilder { DataSource = datei, Mode = modus, Pooling = false }.ToString();

    /// <summary>Entfernt die Zwischendatei, falls vorhanden; Fehler dabei werden ignoriert.</summary>
    private static void LoescheZwischendatei(string zwischendatei)
    {
        try
        {
            File.Delete(zwischendatei);
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            // Der eigentliche Fehler ist wichtiger; die Zwischendatei wird beim nächsten Versuch ersetzt
        }
    }
}

using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten.Datenordner;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Automatische Datensicherung beim Programmstart.
/// </summary>
public static class FuehreStartsicherungDurch
{
    /// <summary>
    /// Zeitlimit für Sichern und Aufräumen; ein ausgeschaltetes NAS soll den Start nicht länger aufhalten.
    /// </summary>
    public static readonly TimeSpan StandardZeitlimit = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Zählt den Programmstart und sichert die Datenbank des Datenordners, wenn die Sicherung fällig ist.
    /// </summary>
    /// <remarks>
    /// Ablauf:
    /// <list type="number">
    /// <item>Einstellungen aus der Datenbank des Datenordners lesen, Startzähler um 1 erhöhen und
    /// sofort speichern – der Start zählt auch dann, wenn danach etwas schiefgeht.</item>
    /// <item>Prüfen, ob die Sicherung fällig ist (<see cref="IstSicherungFaellig"/>): Intervall erreicht
    /// und mindestens eine Trainingseinheit vorhanden.</item>
    /// <item>Fällig: Sicherung anlegen (<see cref="ErstelleSicherung"/>) und überzählige alte Sicherungen
    /// löschen (<see cref="EntferneAlteSicherungen"/>). Beides läuft im Hintergrund mit Zeitlimit.</item>
    /// <item>Erfolg: Startzähler auf 0 setzen. Fehler: Zähler bleibt stehen, sodass die Sicherung beim
    /// nächsten Start sofort wieder fällig ist; das Ergebnis enthält einen verständlichen Grund.</item>
    /// </list>
    /// Weil Einstellungen und Zähler in der Datenbank des übergebenen Datenordners liegen, wird nach einem
    /// Wechsel des Datenordners automatisch dessen Datenbank gesichert. Die Methode wirft keine Ausnahmen
    /// für die erwarteten Fehler (Datei-, Rechte-, Datenbankfehler, Zeitüberschreitung), damit der
    /// Programmstart in jedem Fall weitergehen kann.
    /// </remarks>
    /// <param name="datenordner">Der geprüfte, aktive Datenordner.</param>
    /// <param name="zeitpunkt">Zeitpunkt für den Namen der Sicherung (in der App: jetzt).</param>
    /// <param name="zeitlimit">Höchstdauer für Sichern und Aufräumen; Standard <see cref="StandardZeitlimit"/>.</param>
    /// <returns>Das Ergebnis mit Status, Sicherungsordner und ggf. Datei oder Fehlergrund.</returns>
    public static async Task<Startsicherungsergebnis> Ausfuehren(string datenordner, DateTime zeitpunkt, TimeSpan? zeitlimit = null)
    {
        TimeSpan limit = zeitlimit ?? StandardZeitlimit;
        string datenbankdatei = Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei);
        string? sicherungsordner = null;

        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datenbankdatei);

            // 1. Start zählen
            Sicherungseinstellungen einstellungen = SicherungEinstellungen.Laden(verbindung);
            einstellungen = einstellungen with { StartsSeitSicherung = einstellungen.StartsSeitSicherung + 1 };
            SicherungEinstellungen.Speichern(verbindung, einstellungen);
            sicherungsordner = ErmittleSicherungsordner.Ausfuehren(datenordner, einstellungen);

            // 2. Fällig?
            if (!IstSicherungFaellig.Ausfuehren(einstellungen.StartsSeitSicherung, einstellungen.Intervall, ZaehleEinheiten(verbindung)))
            {
                return new Startsicherungsergebnis(Startsicherungsstatus.NichtFaellig, sicherungsordner);
            }

            // 3. Sichern und aufräumen – im Hintergrund, damit ein hängendes NAS nach dem Zeitlimit aufgegeben wird
            string ordner = sicherungsordner;
            string sicherung = await Task.Run(() =>
            {
                string datei = ErstelleSicherung.Ausfuehren(datenbankdatei, ordner, zeitpunkt);
                EntferneAlteSicherungen.Ausfuehren(ordner, einstellungen.Hoechstzahl);
                return datei;
            }).WaitAsync(limit);

            // 4. Erfolg: Zählung beginnt von vorn
            SicherungEinstellungen.Speichern(verbindung, einstellungen with { StartsSeitSicherung = 0 });
            return new Startsicherungsergebnis(Startsicherungsstatus.Gesichert, sicherungsordner, sicherung);
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or SqliteException or TimeoutException)
        {
            return new Startsicherungsergebnis(Startsicherungsstatus.Fehlgeschlagen, sicherungsordner,
                Fehlergrund: BeschreibeSicherungsfehler.Ausfuehren(fehler, limit));
        }
    }

    /// <summary>Anzahl der Trainingseinheiten in der Datenbank.</summary>
    private static int ZaehleEinheiten(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT COUNT(*) FROM Trainingseinheit;";
        return Convert.ToInt32(befehl.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}

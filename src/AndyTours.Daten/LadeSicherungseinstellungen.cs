using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt die Einstellungen der Datensicherung aus der Tabelle <c>Einstellung</c>.
/// </summary>
public static class LadeSicherungseinstellungen
{
    /// <summary>
    /// Liest Sicherungsordner, Intervall, Höchstzahl und Startzähler.
    /// </summary>
    /// <remarks>
    /// Fehlende oder unbrauchbare Einträge (z. B. von Hand in der Datenbank verändert) werden
    /// durch die Standardwerte aus <see cref="Sicherungseinstellungen"/> ersetzt – eine neue oder
    /// ältere Datenbank ohne Einträge liefert also genau die Standardeinstellungen.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Einstellungen.</returns>
    public static Sicherungseinstellungen Ausfuehren(SqliteConnection verbindung)
    {
        // Alle Einträge auf einmal lesen; es sind nur wenige
        var werte = new Dictionary<string, string>(StringComparer.Ordinal);
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "SELECT Schluessel, Wert FROM Einstellung;";
            using SqliteDataReader leser = befehl.ExecuteReader();
            while (leser.Read())
            {
                werte[leser.GetString(0)] = leser.GetString(1);
            }
        }

        var standard = new Sicherungseinstellungen();
        string? ordner = werte.GetValueOrDefault(Einstellungsschluessel.Sicherungsordner);
        int starts = int.TryParse(werte.GetValueOrDefault(Einstellungsschluessel.StartsSeitSicherung),
            NumberStyles.None, CultureInfo.InvariantCulture, out int s) ? s : 0;

        return new Sicherungseinstellungen
        {
            Sicherungsordner = string.IsNullOrWhiteSpace(ordner) ? null : ordner,
            // Intervall und Höchstzahl unterliegen denselben Grenzen wie die Eingabe in den Einstellungen
            Intervall = PruefeSicherungswert.Ausfuehren(werte.GetValueOrDefault(Einstellungsschluessel.Sicherungsintervall)) ?? standard.Intervall,
            Hoechstzahl = PruefeSicherungswert.Ausfuehren(werte.GetValueOrDefault(Einstellungsschluessel.Sicherungshoechstzahl)) ?? standard.Hoechstzahl,
            StartsSeitSicherung = starts,
        };
    }
}

using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Speichert die zuletzt gewählte Karteikarte des Hauptfensters.
/// </summary>
public static class SpeichereKarteikarte
{
    /// <summary>
    /// Schreibt die Nummer (Index ab 0) der gewählten Karteikarte in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="nummer">Index der Karteikarte, ab 0.</param>
    public static void Ausfuehren(SqliteConnection verbindung, int nummer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nummer);

        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Karteikarte);
        befehl.Parameters.AddWithValue("$wert", nummer.ToString(CultureInfo.InvariantCulture));
        befehl.ExecuteNonQuery();
    }
}

using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die zuletzt gewählte Karteikarte des Hauptfensters.
/// </summary>
public static class KarteikarteEinstellung
{
    /// <summary>
    /// Liest die Nummer der zuletzt gewählten Karteikarte aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Gespeichert ist die Nummer (Index ab 0) der Karteikarte. Fehlt der Eintrag oder ist er keine
    /// Zahl ab 0, wird <c>null</c> geliefert – dann zeigt das Hauptfenster die erste Karteikarte.
    /// Ob es die Karteikarte noch gibt, prüft das Hauptfenster.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Nummer der Karteikarte oder <c>null</c>.</returns>
    public static int? Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Karteikarte);
        if (befehl.ExecuteScalar() is not string text
            || !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int nummer))
        {
            return null;
        }

        return nummer;
    }

    /// <summary>
    /// Schreibt die Nummer (Index ab 0) der gewählten Karteikarte in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="nummer">Index der Karteikarte, ab 0.</param>
    public static void Speichern(SqliteConnection verbindung, int nummer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nummer);

        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Karteikarte);
        befehl.Parameters.AddWithValue("$wert", nummer.ToString(CultureInfo.InvariantCulture));
        befehl.ExecuteNonQuery();
    }
}

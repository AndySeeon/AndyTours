using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Lädt die zuletzt gewählte Karteikarte des Hauptfensters.
/// </summary>
public static class LadeKarteikarte
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
    public static int? Ausfuehren(SqliteConnection verbindung)
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
}

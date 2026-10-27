using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt den Ordner, der zuletzt für „Als GPX exportieren…“ verwendet wurde.
/// </summary>
public static class GpxOrdnerEinstellung
{
    /// <summary>
    /// Liest den zuletzt verwendeten Ordner aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Ob es den Ordner noch gibt, prüft der Aufrufer; sonst schlägt er „Dokumente“ vor.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Der Ordner oder <c>null</c>, wenn noch keiner gespeichert ist.</returns>
    public static string? Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.GpxOrdner);
        return befehl.ExecuteScalar() is string ordner && ordner.Length > 0 ? ordner : null;
    }

    /// <summary>
    /// Schreibt den verwendeten Ordner in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="ordner">Der Ordner, in den zuletzt exportiert wurde.</param>
    public static void Speichern(SqliteConnection verbindung, string ordner)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.GpxOrdner);
        befehl.Parameters.AddWithValue("$wert", ordner);
        befehl.ExecuteNonQuery();
    }
}

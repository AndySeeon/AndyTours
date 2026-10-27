using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt eine in den Einstellungen gepflegte Auswahlliste (z. B. die Trainingsbereiche).
/// </summary>
/// <remarks>
/// Die Liste steht in der Tabelle <c>Einstellung</c> unter <see cref="Einstellungsschluessel.Auswahlliste"/>:
/// die Einträge in der Reihenfolge des Benutzers, einen je Zeile. Ein fehlender Schlüssel bedeutet „noch nie
/// angelegt“, ein leerer Wert „alle Einträge gelöscht“ – beides muss unterscheidbar bleiben, damit eine vom
/// Benutzer geleerte Liste nicht wieder aus den Einheiten aufgefüllt wird.
/// </remarks>
public static class AuswahllisteEinstellung
{
    /// <summary>Trennzeichen zwischen den Einträgen.</summary>
    private const char Trenner = '\n';

    /// <summary>
    /// Liest die Einträge einer Liste.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="liste">Die Liste.</param>
    /// <returns>Die Einträge in gespeicherter Reihenfolge; <c>null</c>, wenn die Liste noch nie angelegt wurde.</returns>
    public static List<string>? Laden(SqliteConnection verbindung, Auswahlliste liste)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Auswahlliste(liste));
        if (befehl.ExecuteScalar() is not string text)
        {
            return null;
        }

        return text.Split(Trenner, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    /// <summary>
    /// Schreibt die Einträge einer Liste in der angegebenen Reihenfolge; eine leere Liste wird als leerer
    /// Wert gespeichert (nicht gelöscht).
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion (z. B. beim Umbenennen samt Einheiten) oder <c>null</c>.</param>
    /// <param name="liste">Die Liste.</param>
    /// <param name="eintraege">Die Einträge.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction? transaktion, Auswahlliste liste, IEnumerable<string> eintraege)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Auswahlliste(liste));
        befehl.Parameters.AddWithValue("$wert", string.Join(Trenner, eintraege.Select(e => e.Trim())));
        befehl.ExecuteNonQuery();
    }
}

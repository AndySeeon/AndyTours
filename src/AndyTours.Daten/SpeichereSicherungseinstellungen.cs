using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Speichert die Einstellungen der Datensicherung in der Tabelle <c>Einstellung</c>.
/// </summary>
public static class SpeichereSicherungseinstellungen
{
    /// <summary>
    /// Schreibt alle Werte; vorhandene Einträge werden ersetzt.
    /// </summary>
    /// <remarks>
    /// Ist kein eigener Sicherungsordner gewählt (<c>null</c>), wird der Eintrag gelöscht – dann gilt
    /// wieder der Standardordner. Alle Änderungen laufen in einer Transaktion, damit nie nur ein Teil
    /// der Einstellungen gespeichert ist.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einstellungen">Die zu speichernden Einstellungen.</param>
    public static void Ausfuehren(SqliteConnection verbindung, Sicherungseinstellungen einstellungen)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        Schreibe(verbindung, transaktion, Einstellungsschluessel.Sicherungsordner, einstellungen.Sicherungsordner);
        Schreibe(verbindung, transaktion, Einstellungsschluessel.Sicherungsintervall, AlsText(einstellungen.Intervall));
        Schreibe(verbindung, transaktion, Einstellungsschluessel.Sicherungshoechstzahl, AlsText(einstellungen.Hoechstzahl));
        Schreibe(verbindung, transaktion, Einstellungsschluessel.StartsSeitSicherung, AlsText(einstellungen.StartsSeitSicherung));

        transaktion.Commit();
    }

    /// <summary>Ganze Zahl als Text ohne Tausenderpunkte (unabhängig von der Spracheinstellung).</summary>
    private static string AlsText(int zahl) => zahl.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Ersetzt einen Eintrag oder löscht ihn, wenn <paramref name="wert"/> <c>null</c> ist.
    /// </summary>
    private static void Schreibe(SqliteConnection verbindung, SqliteTransaction transaktion, string schluessel, string? wert)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = wert is null
            ? "DELETE FROM Einstellung WHERE Schluessel = $schluessel;"
            : "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", schluessel);
        if (wert is not null)
        {
            befehl.Parameters.AddWithValue("$wert", wert);
        }

        befehl.ExecuteNonQuery();
    }
}

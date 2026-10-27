using System.Globalization;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Einstellungen der automatischen Datensicherung.
/// </summary>
public static class SicherungEinstellungen
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
    public static Sicherungseinstellungen Laden(SqliteConnection verbindung)
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
    public static void Speichern(SqliteConnection verbindung, Sicherungseinstellungen einstellungen)
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

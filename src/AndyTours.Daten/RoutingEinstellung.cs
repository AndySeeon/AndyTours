using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Einstellungen „Routing (Tourenplanung)“: Dienst, BRouter-Adresse und Zugangsschlüssel für
/// openrouteservice (siehe <c>docs/adr/0004-routingdienst.md</c>).
/// </summary>
/// <remarks>
/// Der Zugangsschlüssel liegt wie alle Einstellungen in der Datenbank im Datenordner. Er wird hier weder protokolliert
/// noch in Fehlermeldungen genannt.
/// </remarks>
public static class RoutingEinstellung
{
    /// <summary>
    /// Liest die Routingeinstellungen aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Ohne Einträge gilt <see cref="Routingeinstellungen.Standard"/>: BRouter mit dem öffentlichen Server.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Einstellungen.</returns>
    public static Routingeinstellungen Laden(SqliteConnection verbindung)
    {
        var werte = new Dictionary<string, string>(StringComparer.Ordinal);
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "SELECT Schluessel, Wert FROM Einstellung WHERE Schluessel LIKE 'Routing.%';";
            using SqliteDataReader leser = befehl.ExecuteReader();
            while (leser.Read())
            {
                werte[leser.GetString(0)] = leser.GetString(1);
            }
        }

        Routingeinstellungen standard = Routingeinstellungen.Standard;
        Routingdienst dienst = Enum.TryParse(werte.GetValueOrDefault(Einstellungsschluessel.RoutingDienst), out Routingdienst d) && Enum.IsDefined(d)
            ? d
            : standard.Dienst;
        string? adresse = werte.GetValueOrDefault(Einstellungsschluessel.RoutingBRouterAdresse);
        string? schluessel = werte.GetValueOrDefault(Einstellungsschluessel.RoutingOpenrouteserviceSchluessel);
        return new Routingeinstellungen(
            dienst,
            string.IsNullOrWhiteSpace(adresse) ? standard.BRouterAdresse : adresse,
            string.IsNullOrWhiteSpace(schluessel) ? null : schluessel);
    }

    /// <summary>
    /// Schreibt die Routingeinstellungen in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Eine leere BRouter-Adresse oder ein leerer Schlüssel löscht den Eintrag; beim nächsten Laden gilt dann der
    /// öffentliche Server bzw. kein Schlüssel.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einstellungen">Die Einstellungen.</param>
    public static void Speichern(SqliteConnection verbindung, Routingeinstellungen einstellungen)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        Schreibe(verbindung, transaktion, Einstellungsschluessel.RoutingDienst, einstellungen.Dienst.ToString());
        Schreibe(verbindung, transaktion, Einstellungsschluessel.RoutingBRouterAdresse, Bereinigt(einstellungen.BRouterAdresse));
        Schreibe(verbindung, transaktion, Einstellungsschluessel.RoutingOpenrouteserviceSchluessel, Bereinigt(einstellungen.OpenrouteserviceSchluessel));
        transaktion.Commit();
    }

    /// <summary>Text ohne Leerzeichen am Rand; leer → <c>null</c>.</summary>
    private static string? Bereinigt(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Schreibt einen Eintrag; <c>null</c> löscht ihn.</summary>
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

using System.Globalization;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt, was sich das Fenster „Tour planen“ merkt: Fahrprofil, Kartenausschnitt und Ordner.
/// </summary>
public static class PlanungseinstellungenEinstellung
{
    /// <summary>
    /// Liest die Planungseinstellungen aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Fehlt ein Eintrag oder ist er unlesbar, gilt der Wert aus <see cref="Planungseinstellungen.Standard"/>
    /// (Fahrprofil „Trekking/Tourenrad“, kein Ausschnitt, kein Ordner). Ob es den Ordner noch gibt, prüft der Aufrufer.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Einstellungen.</returns>
    public static Planungseinstellungen Laden(SqliteConnection verbindung)
    {
        Dictionary<string, string> werte = LiesWerte(verbindung);
        Planungseinstellungen standard = Planungseinstellungen.Standard;

        Fahrprofil profil = Enum.TryParse(werte.GetValueOrDefault(Einstellungsschluessel.PlanungProfil), out Fahrprofil p) && Enum.IsDefined(p)
            ? p
            : standard.Profil;
        (Geoposition? suedWest, Geoposition? nordOst) = LiesAusschnitt(werte.GetValueOrDefault(Einstellungsschluessel.PlanungAusschnitt));
        string? ordner = werte.GetValueOrDefault(Einstellungsschluessel.PlanungOrdner);
        return new Planungseinstellungen(profil, suedWest, nordOst, string.IsNullOrWhiteSpace(ordner) ? null : ordner);
    }

    /// <summary>
    /// Schreibt die Planungseinstellungen in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Ein fehlender Ausschnitt oder Ordner löscht den jeweiligen Eintrag.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einstellungen">Die zu merkenden Einstellungen.</param>
    public static void Speichern(SqliteConnection verbindung, Planungseinstellungen einstellungen)
    {
        string? ausschnitt = einstellungen.SuedWest is Geoposition sw && einstellungen.NordOst is Geoposition no
            ? string.Join(';', new[] { sw.Breite, sw.Laenge, no.Breite, no.Laenge }.Select(z => z.ToString("R", CultureInfo.InvariantCulture)))
            : null;

        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        Schreibe(verbindung, transaktion, Einstellungsschluessel.PlanungProfil, einstellungen.Profil.ToString());
        Schreibe(verbindung, transaktion, Einstellungsschluessel.PlanungAusschnitt, ausschnitt);
        Schreibe(verbindung, transaktion, Einstellungsschluessel.PlanungOrdner, einstellungen.Ordner);
        transaktion.Commit();
    }

    /// <summary>Liest den Ausschnitt „Südbreite;Westlänge;Nordbreite;Ostlänge“; unlesbar → kein Ausschnitt.</summary>
    private static (Geoposition? SuedWest, Geoposition? NordOst) LiesAusschnitt(string? text)
    {
        string[] teile = text?.Split(';') ?? [];
        double[] zahlen = [.. teile
            .Select(t => double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double z) ? z : double.NaN)
            .Where(double.IsFinite)];
        if (teile.Length != 4 || zahlen.Length != 4 || zahlen[0] >= zahlen[2] || zahlen[1] >= zahlen[3])
        {
            return (null, null);
        }

        return (new Geoposition(zahlen[0], zahlen[1]), new Geoposition(zahlen[2], zahlen[3]));
    }

    /// <summary>Liest alle Einträge der Tourenplanung.</summary>
    private static Dictionary<string, string> LiesWerte(SqliteConnection verbindung)
    {
        var werte = new Dictionary<string, string>(StringComparer.Ordinal);
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Schluessel, Wert FROM Einstellung WHERE Schluessel LIKE 'Tourenplanung.%';";
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            werte[leser.GetString(0)] = leser.GetString(1);
        }

        return werte;
    }

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

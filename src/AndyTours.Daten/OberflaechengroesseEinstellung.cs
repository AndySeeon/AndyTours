using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Größe der Oberfläche (Einstellungen, Gruppe „Darstellung“, Spezifikation 035).
/// </summary>
/// <remarks>
/// Die Größe gehört zum Datenbestand: Sie wandert mit dem Datenordner und mit jeder Sicherung mit
/// (siehe docs/adr/0007-skalierung-der-oberflaeche.md).
/// </remarks>
public static class OberflaechengroesseEinstellung
{
    /// <summary>Die wählbaren Stufen in Prozent, in der Reihenfolge der Auswahl.</summary>
    public static IReadOnlyList<int> Stufen { get; } = [100, 110, 125, 150, 175, 200];

    /// <summary>Die voreingestellte Größe: 100 %, also die Oberfläche wie bisher.</summary>
    public const int Standard = 100;

    /// <summary>
    /// Liest die Größe der Oberfläche aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Nur die festen <see cref="Stufen"/> gelten. Ein fehlender, unlesbarer oder von Hand auf eine andere
    /// Zahl geänderter Wert ergibt <see cref="Standard"/>, damit die Oberfläche nie unbrauchbar groß oder klein wird.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Größe in Prozent, z. B. 125.</returns>
    public static int Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.DarstellungGroesse);
        return befehl.ExecuteScalar() is string wert
            && int.TryParse(wert, NumberStyles.Integer, CultureInfo.InvariantCulture, out int prozent)
            && Stufen.Contains(prozent)
            ? prozent
            : Standard;
    }

    /// <summary>
    /// Schreibt die Größe der Oberfläche in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="prozent">Die Größe in Prozent; muss eine der <see cref="Stufen"/> sein.</param>
    /// <exception cref="ArgumentOutOfRangeException">Die Größe ist keine der Stufen.</exception>
    public static void Speichern(SqliteConnection verbindung, int prozent)
    {
        if (!Stufen.Contains(prozent))
        {
            throw new ArgumentOutOfRangeException(nameof(prozent), prozent, "Die Größe muss eine der festen Stufen sein.");
        }

        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.DarstellungGroesse);
        befehl.Parameters.AddWithValue("$wert", prozent.ToString(CultureInfo.InvariantCulture));
        befehl.ExecuteNonQuery();
    }
}

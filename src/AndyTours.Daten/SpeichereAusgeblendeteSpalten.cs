using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Speichert, welche Spalten der Tabelle „Trainingseinheiten“ ausgeblendet sind.
/// </summary>
public static class SpeichereAusgeblendeteSpalten
{
    /// <summary>
    /// Schreibt die ausgeblendeten Spalten in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Gespeichert werden die ausgeblendeten (nicht die sichtbaren) Spalten, damit Spalten, die in
    /// einer späteren Version hinzukommen, automatisch sichtbar sind. Die Namen stehen in
    /// Anzeigereihenfolge, durch Komma getrennt. Eine leere Menge löscht den Eintrag (= alle sichtbar).
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="ausgeblendet">Die ausgeblendeten Spalten.</param>
    public static void Ausfuehren(SqliteConnection verbindung, IReadOnlySet<Einheitenspalte> ausgeblendet)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.AusgeblendeteSpalten);
        if (ausgeblendet.Count == 0)
        {
            befehl.CommandText = "DELETE FROM Einstellung WHERE Schluessel = $schluessel;";
        }
        else
        {
            befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
            befehl.Parameters.AddWithValue("$wert", string.Join(",", ausgeblendet.Order().Select(s => s.ToString())));
        }

        befehl.ExecuteNonQuery();
    }
}

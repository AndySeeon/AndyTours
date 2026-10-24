using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Speichert Reihenfolge und Breiten der Spalten der Tabelle „Trainingseinheiten“.
/// </summary>
public static class SpeichereSpaltenanordnung
{
    /// <summary>
    /// Schreibt die Anordnung als Text, z. B. <c>Datum:96,Startzeit:,Name:250</c>.
    /// </summary>
    /// <remarks>
    /// Die Breite wird auf eine Nachkommastelle gerundet und mit Punkt geschrieben (unabhängig von der
    /// Spracheinstellung); eine automatische Breite (<c>null</c>) bleibt leer.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="anordnung">Die Spalten in Anzeigereihenfolge.</param>
    public static void Ausfuehren(SqliteConnection verbindung, IEnumerable<Spalteneinstellung> anordnung)
    {
        string text = string.Join(",", anordnung.Select(a =>
            a.Spalte + ":" + (a.Breite is double breite ? Math.Round(breite, 1).ToString(CultureInfo.InvariantCulture) : string.Empty)));

        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Spaltenanordnung);
        befehl.Parameters.AddWithValue("$wert", text);
        befehl.ExecuteNonQuery();
    }
}

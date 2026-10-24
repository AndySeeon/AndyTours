using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Speichert Größe, Position und Zustand des Hauptfensters.
/// </summary>
public static class SpeichereFensterposition
{
    /// <summary>
    /// Schreibt die Fensterposition als <c>Links;Oben;Breite;Höhe;Maximiert</c> in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="position">Die zu merkende Position.</param>
    public static void Ausfuehren(SqliteConnection verbindung, Fensterposition position)
    {
        string text = string.Join(";",
            Zahl(position.Links), Zahl(position.Oben), Zahl(position.Breite), Zahl(position.Hoehe), position.Maximiert ? "1" : "0");

        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($schluessel, $wert);";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Fensterposition);
        befehl.Parameters.AddWithValue("$wert", text);
        befehl.ExecuteNonQuery();
    }

    /// <summary>Zahl mit einer Nachkommastelle und Punkt (unabhängig von der Spracheinstellung).</summary>
    private static string Zahl(double wert) => Math.Round(wert, 1).ToString(CultureInfo.InvariantCulture);
}

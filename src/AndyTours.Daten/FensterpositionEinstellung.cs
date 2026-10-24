using System.Globalization;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt Größe, Position und Zustand des Hauptfensters.
/// </summary>
public static class FensterpositionEinstellung
{
    /// <summary>
    /// Liest die Fensterposition aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Gespeichert ist <c>Links;Oben;Breite;Höhe;Maximiert</c> mit Punkt als Dezimaltrennzeichen und
    /// 0/1 für „maximiert“. Fehlt der Eintrag oder ist er unlesbar (oder Breite/Höhe nicht positiv),
    /// wird <c>null</c> geliefert – dann öffnet das Fenster in seiner Standardgröße.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die gemerkte Position oder <c>null</c>.</returns>
    public static Fensterposition? Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Fensterposition);
        if (befehl.ExecuteScalar() is not string text)
        {
            return null;
        }

        string[] teile = text.Split(';');
        if (teile.Length != 5)
        {
            return null;
        }

        var zahlen = new double[4];
        for (int i = 0; i < 4; i++)
        {
            if (!double.TryParse(teile[i], NumberStyles.Float, CultureInfo.InvariantCulture, out zahlen[i]) || !double.IsFinite(zahlen[i]))
            {
                return null;
            }
        }

        if (zahlen[2] <= 0 || zahlen[3] <= 0 || teile[4] is not ("0" or "1"))
        {
            return null;
        }

        return new Fensterposition(zahlen[0], zahlen[1], zahlen[2], zahlen[3], teile[4] == "1");
    }

    /// <summary>
    /// Schreibt die Fensterposition als <c>Links;Oben;Breite;Höhe;Maximiert</c> in die Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="position">Die zu merkende Position.</param>
    public static void Speichern(SqliteConnection verbindung, Fensterposition position)
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

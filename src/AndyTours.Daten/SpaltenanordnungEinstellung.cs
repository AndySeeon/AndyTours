using System.Globalization;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt Reihenfolge und Breiten der Spalten der Tabelle „Trainingseinheiten“.
/// </summary>
public static class SpaltenanordnungEinstellung
{
    /// <summary>
    /// Liest die gespeicherte Anordnung und vervollständigt sie.
    /// </summary>
    /// <remarks>
    /// Gespeichert ist ein Text wie <c>Datum:96,Startzeit:,Name:250</c>: die Spaltennamen in
    /// Anzeigereihenfolge, jeweils mit Breite (Punkt als Dezimaltrennzeichen) oder leer für
    /// „automatisch“. Unbekannte Spaltennamen und unlesbare Breiten werden übergangen;
    /// <see cref="VervollstaendigeSpaltenanordnung"/> ergänzt fehlende Spalten. Ohne Eintrag ergibt
    /// sich die Standardanordnung.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Alle Spalten genau einmal, in Anzeigereihenfolge.</returns>
    public static List<Spalteneinstellung> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.Spaltenanordnung);
        string text = befehl.ExecuteScalar() as string ?? string.Empty;

        var gespeichert = new List<Spalteneinstellung>();
        foreach (string teil in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] nameUndBreite = teil.Split(':');
            string name = nameUndBreite[0];
            if (int.TryParse(name, out _) || !Enum.TryParse(name, ignoreCase: false, out Einheitenspalte spalte))
            {
                continue; // unbekannte Spalte
            }

            double? breite = nameUndBreite.Length > 1
                && double.TryParse(nameUndBreite[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double b) ? b : null;
            gespeichert.Add(new Spalteneinstellung(spalte, breite));
        }

        return VervollstaendigeSpaltenanordnung.Ausfuehren(gespeichert);
    }

    /// <summary>
    /// Schreibt die Anordnung als Text, z. B. <c>Datum:96,Startzeit:,Name:250</c>.
    /// </summary>
    /// <remarks>
    /// Die Breite wird auf eine Nachkommastelle gerundet und mit Punkt geschrieben (unabhängig von der
    /// Spracheinstellung); eine automatische Breite (<c>null</c>) bleibt leer.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="anordnung">Die Spalten in Anzeigereihenfolge.</param>
    public static void Speichern(SqliteConnection verbindung, IEnumerable<Spalteneinstellung> anordnung)
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

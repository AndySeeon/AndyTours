using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt Reihenfolge und Breiten der Spalten der Tabelle „Trainingseinheiten“.
/// </summary>
public static class LadeSpaltenanordnung
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
    public static List<Spalteneinstellung> Ausfuehren(SqliteConnection verbindung)
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
}

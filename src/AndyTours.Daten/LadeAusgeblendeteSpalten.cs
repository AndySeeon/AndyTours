using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt, welche Spalten der Tabelle „Trainingseinheiten“ ausgeblendet sind.
/// </summary>
public static class LadeAusgeblendeteSpalten
{
    /// <summary>
    /// Liest die ausgeblendeten Spalten aus der Tabelle <c>Einstellung</c>.
    /// </summary>
    /// <remarks>
    /// Gespeichert sind die Namen der <see cref="Einheitenspalte"/>-Werte, durch Komma getrennt. Fehlt
    /// der Eintrag, sind alle Spalten sichtbar. Unbekannte Namen (z. B. von Hand verändert oder eine
    /// inzwischen entfallene Spalte) werden übergangen. Sind dadurch alle Spalten ausgeblendet, gilt
    /// die Wahl als unbrauchbar und alle Spalten sind wieder sichtbar.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die ausgeblendeten Spalten (leer = alle sichtbar).</returns>
    public static HashSet<Einheitenspalte> Ausfuehren(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Wert FROM Einstellung WHERE Schluessel = $schluessel;";
        befehl.Parameters.AddWithValue("$schluessel", Einstellungsschluessel.AusgeblendeteSpalten);
        string text = befehl.ExecuteScalar() as string ?? string.Empty;

        var ausgeblendet = new HashSet<Einheitenspalte>();
        foreach (string name in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // ignoreCase: false und Prüfung auf definierte Werte – Zahlen wie „3“ würden sonst auch akzeptiert
            if (Enum.TryParse(name, ignoreCase: false, out Einheitenspalte spalte) && Enum.IsDefined(spalte)
                && !int.TryParse(name, out _))
            {
                ausgeblendet.Add(spalte);
            }
        }

        return ausgeblendet.Count < Enum.GetValues<Einheitenspalte>().Length ? ausgeblendet : [];
    }
}

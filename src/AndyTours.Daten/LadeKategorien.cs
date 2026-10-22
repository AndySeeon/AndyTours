using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Kategorien aus der Tabelle <c>Kategorie</c>.
/// </summary>
public static class LadeKategorien
{
    /// <summary>
    /// Liest alle Kategorien, sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Kategorien.</returns>
    public static List<Kategorie> Ausfuehren(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Id, Name, IstStandard FROM Kategorie ORDER BY Id;";

        var kategorien = new List<Kategorie>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            kategorien.Add(new Kategorie
            {
                Id = leser.GetInt64(0),
                Name = leser.GetString(1),
                IstStandard = leser.GetBoolean(2),
            });
        }

        return kategorien;
    }
}

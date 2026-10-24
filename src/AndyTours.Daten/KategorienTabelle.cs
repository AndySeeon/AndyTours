using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Kategorie</c>.
/// </summary>
public static class KategorienTabelle
{
    /// <summary>
    /// Liest alle Kategorien, sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Kategorien.</returns>
    public static List<Kategorie> Laden(SqliteConnection verbindung)
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

    /// <summary>
    /// Fügt alle übergebenen Kategorien mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="kategorien">Die Kategorien.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Kategorie> kategorien)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "INSERT INTO Kategorie (Id, Name, IstStandard) VALUES ($id, $name, $istStandard);";

        foreach (Kategorie k in kategorien)
        {
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", k.Id);
            befehl.Parameters.AddWithValue("$name", k.Name);
            befehl.Parameters.AddWithValue("$istStandard", k.IstStandard);
            befehl.ExecuteNonQuery();
        }
    }
}

using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Kategorien in die Tabelle <c>Kategorie</c> ein.
/// </summary>
public static class SpeichereKategorien
{
    /// <summary>
    /// Fügt alle übergebenen Kategorien mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="kategorien">Die Kategorien.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Kategorie> kategorien)
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

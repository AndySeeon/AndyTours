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

    /// <summary>
    /// Fügt eine neue Kategorie ein; die Datenbank vergibt die Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="kategorie">Die Kategorie (ihre <see cref="Kategorie.Id"/> wird nicht verwendet).</param>
    /// <returns>Die vergebene Nummer.</returns>
    public static long Einfuegen(SqliteConnection verbindung, SqliteTransaction transaktion, Kategorie kategorie)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "INSERT INTO Kategorie (Name, IstStandard) VALUES ($name, $istStandard) RETURNING Id;";
        befehl.Parameters.AddWithValue("$name", kategorie.Name);
        befehl.Parameters.AddWithValue("$istStandard", kategorie.IstStandard);
        return (long)befehl.ExecuteScalar()!;
    }

    /// <summary>
    /// Schreibt Name und Standard-Kennzeichen einer vorhandenen Kategorie.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="kategorie">Die Kategorie mit ihrer Nummer.</param>
    public static void Aendern(SqliteConnection verbindung, SqliteTransaction transaktion, Kategorie kategorie)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "UPDATE Kategorie SET Name = $name, IstStandard = $istStandard WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", kategorie.Id);
        befehl.Parameters.AddWithValue("$name", kategorie.Name);
        befehl.Parameters.AddWithValue("$istStandard", kategorie.IstStandard);
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Löscht eine Kategorie. Ist sie noch Einheiten zugeordnet, scheitert das am Fremdschlüssel – die
    /// Oberfläche prüft das vorher mit <c>ErmittleLoeschsperre</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="id">Nummer der Kategorie.</param>
    public static void Loeschen(SqliteConnection verbindung, SqliteTransaction transaktion, long id)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "DELETE FROM Kategorie WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Führt zwei Kategorien zusammen: Jede Einheit der Ursprungskategorie erhält die Zielkategorie (eine
    /// Einheit, die beide hatte, behält sie nur einmal), ein Standard-Kennzeichen geht auf das Ziel über,
    /// die Ursprungskategorie wird gelöscht.
    /// </summary>
    /// <remarks>
    /// <c>INSERT OR IGNORE</c> übergeht Einheiten, die das Ziel schon haben – sonst verletzte die zweite
    /// Zuordnung den Primärschlüssel von <c>EinheitKategorie</c>. Die Standardkategorie der Equipments
    /// (als Name gespeichert) zieht <see cref="EquipmentsTabelle.ErsetzeStandardKategorie"/> nach.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion – alle Schritte gelten nur gemeinsam.</param>
    /// <param name="ursprungId">Nummer der Kategorie, die entfernt wird.</param>
    /// <param name="zielId">Nummer der Kategorie, die bleibt.</param>
    public static void FuehreZusammen(SqliteConnection verbindung, SqliteTransaction transaktion, long ursprungId, long zielId)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT OR IGNORE INTO EinheitKategorie (EinheitId, KategorieId)
                SELECT EinheitId, $ziel FROM EinheitKategorie WHERE KategorieId = $ursprung;
            DELETE FROM EinheitKategorie WHERE KategorieId = $ursprung;
            UPDATE Kategorie SET IstStandard = 1
                WHERE Id = $ziel AND EXISTS (SELECT 1 FROM Kategorie WHERE Id = $ursprung AND IstStandard <> 0);
            DELETE FROM Kategorie WHERE Id = $ursprung;
            """;
        befehl.Parameters.AddWithValue("$ursprung", ursprungId);
        befehl.Parameters.AddWithValue("$ziel", zielId);
        befehl.ExecuteNonQuery();
    }
}

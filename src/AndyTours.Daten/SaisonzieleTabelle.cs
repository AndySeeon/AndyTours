using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Saisonziel</c> (eigene Saisonziele je Aktivität und „ab Jahr“).
/// </summary>
public static class SaisonzieleTabelle
{
    /// <summary>
    /// Liest alle eigenen Saisonziele, sortiert nach Aktivität und Jahr.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Saisonziele.</returns>
    public static List<Saisonziel> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT AktivitaetId, AbJahr, ZielKm, ZielHm, ZielAnzahl
            FROM Saisonziel ORDER BY AktivitaetId, AbJahr;
            """;

        var ziele = new List<Saisonziel>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            ziele.Add(new Saisonziel
            {
                AktivitaetId = leser.GetInt64(0),
                AbJahr = leser.GetInt32(1),
                ZielKm = leser.GetInt32(2),
                ZielHm = leser.GetInt32(3),
                ZielAnzahl = leser.GetInt32(4),
            });
        }

        return ziele;
    }

    /// <summary>
    /// Fügt alle übergebenen Saisonziele ein (für das Speichern des ganzen Datenbestands).
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="ziele">Die Saisonziele.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Saisonziel> ziele)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Saisonziel (AktivitaetId, AbJahr, ZielKm, ZielHm, ZielAnzahl)
            VALUES ($aktivitaet, $jahr, $km, $hm, $anzahl);
            """;

        foreach (Saisonziel z in ziele)
        {
            // Parameter je Zeile neu setzen (Clear), damit keine Werte der vorigen Zeile stehen bleiben.
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$aktivitaet", z.AktivitaetId);
            befehl.Parameters.AddWithValue("$jahr", z.AbJahr);
            befehl.Parameters.AddWithValue("$km", z.ZielKm);
            befehl.Parameters.AddWithValue("$hm", z.ZielHm);
            befehl.Parameters.AddWithValue("$anzahl", z.ZielAnzahl);
            befehl.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Ersetzt die eigenen Saisonziele der angegebenen Jahre; andere Jahre bleiben unverändert.
    /// </summary>
    /// <remarks>
    /// Alles läuft in einer Transaktion: Schlägt ein Schritt fehl, bleibt die Datenbank wie vorher.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="jahre">Die gepflegten Jahre (im Fenster: laufendes und folgendes Jahr).</param>
    /// <param name="ziele">Die neuen eigenen Ziele dieser Jahre.</param>
    public static void SpeichereJahre(SqliteConnection verbindung, IReadOnlyCollection<int> jahre, IEnumerable<Saisonziel> ziele)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        using (SqliteCommand loeschen = verbindung.CreateCommand())
        {
            loeschen.Transaction = transaktion;
            loeschen.CommandText = "DELETE FROM Saisonziel WHERE AbJahr = $jahr;";
            SqliteParameter jahrParameter = loeschen.Parameters.Add("$jahr", SqliteType.Integer);
            foreach (int jahr in jahre)
            {
                jahrParameter.Value = jahr;
                loeschen.ExecuteNonQuery();
            }
        }

        Speichern(verbindung, transaktion, ziele.Where(z => jahre.Contains(z.AbJahr)));
        transaktion.Commit();
    }
}

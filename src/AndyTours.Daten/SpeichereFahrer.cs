using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Sportler in die Tabelle <c>Fahrer</c> ein.
/// </summary>
public static class SpeichereFahrer
{
    /// <summary>
    /// Fügt alle übergebenen Sportler mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="fahrer">Die Sportler.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Fahrer> fahrer)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Fahrer (Id, Vorname, Nachname, Geburtstag, GroesseCm, GewichtKg, Ruhepuls, Maxpuls,
                                TrainingspulsVon, TrainingspulsBis, Heimatort, HeimatBreitengrad, HeimatLaengengrad)
            VALUES ($id, $vorname, $nachname, $geburtstag, $groesse, $gewicht, $ruhepuls, $maxpuls,
                    $tpVon, $tpBis, $heimatort, $breite, $laenge);
            """;

        foreach (Fahrer f in fahrer)
        {
            // Parameter je Zeile neu setzen (Clear), damit keine Werte der vorigen Zeile stehen bleiben.
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", f.Id);
            befehl.Parameters.AddWithValue("$vorname", f.Vorname);
            befehl.Parameters.AddWithValue("$nachname", f.Nachname);
            befehl.Parameters.AddWithValue("$geburtstag", DatumAlsText.Ausfuehren(f.Geburtstag));
            befehl.Parameters.AddWithValue("$groesse", f.GroesseCm);
            befehl.Parameters.AddWithValue("$gewicht", f.GewichtKg);
            befehl.Parameters.AddWithValue("$ruhepuls", f.Ruhepuls);
            befehl.Parameters.AddWithValue("$maxpuls", f.Maxpuls);
            befehl.Parameters.AddWithValue("$tpVon", f.TrainingspulsVon);
            befehl.Parameters.AddWithValue("$tpBis", f.TrainingspulsBis);
            befehl.Parameters.AddWithValue("$heimatort", f.Heimatort);
            befehl.Parameters.AddWithValue("$breite", (object?)f.HeimatBreitengrad ?? DBNull.Value);
            befehl.Parameters.AddWithValue("$laenge", (object?)f.HeimatLaengengrad ?? DBNull.Value);
            befehl.ExecuteNonQuery();
        }
    }
}

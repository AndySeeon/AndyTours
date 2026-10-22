using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Sportler aus der Tabelle <c>Fahrer</c>.
/// </summary>
public static class LadeFahrer
{
    /// <summary>
    /// Liest alle Sportler, sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Sportler.</returns>
    public static List<Fahrer> Ausfuehren(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Vorname, Nachname, Geburtstag, GroesseCm, GewichtKg, Ruhepuls, Maxpuls,
                   TrainingspulsVon, TrainingspulsBis, Heimatort, HeimatBreitengrad, HeimatLaengengrad
            FROM Fahrer ORDER BY Id;
            """;

        var fahrer = new List<Fahrer>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            // Die Spaltennummern entsprechen der Reihenfolge im SELECT oben.
            fahrer.Add(new Fahrer
            {
                Id = leser.GetInt64(0),
                Vorname = leser.GetString(1),
                Nachname = leser.GetString(2),
                Geburtstag = TextAlsDatum.Ausfuehren(leser, 3),
                GroesseCm = leser.GetInt32(4),
                GewichtKg = leser.GetDouble(5),
                Ruhepuls = leser.GetInt32(6),
                Maxpuls = leser.GetInt32(7),
                TrainingspulsVon = leser.GetInt32(8),
                TrainingspulsBis = leser.GetInt32(9),
                Heimatort = leser.GetString(10),
                HeimatBreitengrad = leser.IsDBNull(11) ? null : leser.GetDouble(11),
                HeimatLaengengrad = leser.IsDBNull(12) ? null : leser.GetDouble(12),
            });
        }

        return fahrer;
    }
}

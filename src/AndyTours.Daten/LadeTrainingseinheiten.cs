using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Trainingseinheiten samt Kategorie-Zuordnungen und Anhängen.
/// </summary>
public static class LadeTrainingseinheiten
{
    /// <summary>
    /// Liest alle Einheiten, sortiert nach Datum und Startzeit.
    /// </summary>
    /// <remarks>
    /// Statt für jede Einheit einzeln die Kategorien und Anhänge abzufragen (≈ 2000 Abfragen),
    /// werden drei Abfragen über die ganzen Tabellen gestellt und die Ergebnisse über die
    /// Einheiten-Nummer im Arbeitsspeicher zugeordnet. Das ist deutlich schneller.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Trainingseinheiten.</returns>
    public static List<Trainingseinheit> Ausfuehren(SqliteConnection verbindung)
    {
        var einheiten = new List<Trainingseinheit>();

        // Nachschlagetabelle Nummer → Einheit für die Zuordnung der Kategorien und Anhänge
        var nachNummer = new Dictionary<long, Trainingseinheit>();

        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = """
                SELECT Id, FahrerId, Datum, Startzeit, Name, FahrzeitSekunden, DistanzKm, Hoehenmeter,
                       MaxHoehe, StartHoehe, MaxGeschwindigkeit, HfDurchschnitt, HfMax, HfMin,
                       Trittfrequenz, Temperatur, LeistungWatt, KoerpergewichtKg, Erholungspuls,
                       Trainingsbereich, Kommentar, AktivitaetId, EquipmentId
                FROM Trainingseinheit ORDER BY Datum, Startzeit, Id;
                """;

            using SqliteDataReader leser = befehl.ExecuteReader();
            while (leser.Read())
            {
                var einheit = new Trainingseinheit
                {
                    Id = leser.GetInt64(0),
                    FahrerId = leser.GetInt64(1),
                    // Datum ist Pflichtfeld (NOT NULL), deshalb ist .Value hier sicher
                    Datum = TextAlsDatum.Ausfuehren(leser, 2)!.Value,
                    Startzeit = leser.IsDBNull(3)
                        ? null
                        : TimeOnly.ParseExact(leser.GetString(3), "HH:mm", CultureInfo.InvariantCulture),
                    Name = leser.GetString(4),
                    Fahrzeit = TimeSpan.FromSeconds(leser.GetInt64(5)),
                    DistanzKm = leser.GetDouble(6),
                    Hoehenmeter = leser.GetInt32(7),
                    MaxHoehe = leser.GetInt32(8),
                    StartHoehe = leser.GetInt32(9),
                    MaxGeschwindigkeit = leser.GetDouble(10),
                    HfDurchschnitt = leser.GetInt32(11),
                    HfMax = leser.GetInt32(12),
                    HfMin = leser.GetInt32(13),
                    Trittfrequenz = leser.GetInt32(14),
                    Temperatur = leser.GetInt32(15),
                    LeistungWatt = leser.GetDouble(16),
                    KoerpergewichtKg = leser.GetDouble(17),
                    Erholungspuls = leser.GetInt32(18),
                    Trainingsbereich = leser.GetString(19),
                    Kommentar = leser.GetString(20),
                    AktivitaetId = leser.GetInt64(21),
                    EquipmentId = leser.IsDBNull(22) ? null : leser.GetInt64(22),
                };
                einheiten.Add(einheit);
                nachNummer[einheit.Id] = einheit;
            }
        }

        // Kategorie-Zuordnungen
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "SELECT EinheitId, KategorieId FROM EinheitKategorie ORDER BY EinheitId, KategorieId;";
            using SqliteDataReader leser = befehl.ExecuteReader();
            while (leser.Read())
            {
                nachNummer[leser.GetInt64(0)].KategorieIds.Add(leser.GetInt64(1));
            }
        }

        // Anhänge
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "SELECT Id, EinheitId, Typ, RelativerPfad FROM Anhang ORDER BY Id;";
            using SqliteDataReader leser = befehl.ExecuteReader();
            while (leser.Read())
            {
                nachNummer[leser.GetInt64(1)].Anhaenge.Add(new Anhang
                {
                    Id = leser.GetInt64(0),
                    Typ = Enum.Parse<AnhangTyp>(leser.GetString(2)),
                    RelativerPfad = leser.GetString(3),
                });
            }
        }

        return einheiten;
    }
}

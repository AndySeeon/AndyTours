using System.Globalization;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Trainingseinheiten samt Kategorien und Anhängen.
/// </summary>
public static class TrainingseinheitenTabelle
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
    public static List<Trainingseinheit> Laden(SqliteConnection verbindung)
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
                    Datum = DatumSpalte.Lesen(leser, 2)!.Value,
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

    /// <summary>
    /// Fügt alle übergebenen Einheiten mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <remarks>
    /// Eine Einheit verteilt sich auf drei Tabellen: <c>Trainingseinheit</c> (Kennwerte),
    /// <c>EinheitKategorie</c> (je zugeordneter Kategorie eine Zeile) und <c>Anhang</c>.
    /// Die drei Befehle werden einmal vorbereitet und für jede Einheit wiederverwendet.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="einheiten">Die Trainingseinheiten.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Trainingseinheit> einheiten)
    {
        using SqliteCommand einheitBefehl = verbindung.CreateCommand();
        einheitBefehl.Transaction = transaktion;
        einheitBefehl.CommandText = """
            INSERT INTO Trainingseinheit (Id, FahrerId, Datum, Startzeit, Name, FahrzeitSekunden, DistanzKm, Hoehenmeter,
                                          MaxHoehe, StartHoehe, MaxGeschwindigkeit, HfDurchschnitt, HfMax, HfMin,
                                          Trittfrequenz, Temperatur, LeistungWatt, KoerpergewichtKg, Erholungspuls,
                                          Trainingsbereich, Kommentar, AktivitaetId, EquipmentId)
            VALUES ($id, $fahrerId, $datum, $startzeit, $name, $fahrzeit, $distanz, $hm,
                    $maxHoehe, $startHoehe, $maxGeschw, $hfDurchschnitt, $hfMax, $hfMin,
                    $tf, $temperatur, $leistung, $gewicht, $erholungspuls,
                    $bereich, $kommentar, $aktivitaetId, $equipmentId);
            """;

        using SqliteCommand kategorieBefehl = verbindung.CreateCommand();
        kategorieBefehl.Transaction = transaktion;
        kategorieBefehl.CommandText = "INSERT INTO EinheitKategorie (EinheitId, KategorieId) VALUES ($einheitId, $kategorieId);";

        using SqliteCommand anhangBefehl = verbindung.CreateCommand();
        anhangBefehl.Transaction = transaktion;
        anhangBefehl.CommandText = "INSERT INTO Anhang (Id, EinheitId, Typ, RelativerPfad) VALUES ($id, $einheitId, $typ, $pfad);";

        foreach (Trainingseinheit e in einheiten)
        {
            einheitBefehl.Parameters.Clear();
            einheitBefehl.Parameters.AddWithValue("$id", e.Id);
            einheitBefehl.Parameters.AddWithValue("$fahrerId", e.FahrerId);
            einheitBefehl.Parameters.AddWithValue("$datum", DatumSpalte.AlsText(e.Datum));
            // Uhrzeit als „hh:mm"; Sekunden werden im Original nicht erfasst
            einheitBefehl.Parameters.AddWithValue("$startzeit",
                e.Startzeit?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
            einheitBefehl.Parameters.AddWithValue("$name", e.Name);
            // Dauer als ganze Sekunden; Bruchteile kommen in den Daten nicht vor
            einheitBefehl.Parameters.AddWithValue("$fahrzeit", (long)e.Fahrzeit.TotalSeconds);
            einheitBefehl.Parameters.AddWithValue("$distanz", e.DistanzKm);
            einheitBefehl.Parameters.AddWithValue("$hm", e.Hoehenmeter);
            einheitBefehl.Parameters.AddWithValue("$maxHoehe", e.MaxHoehe);
            einheitBefehl.Parameters.AddWithValue("$startHoehe", e.StartHoehe);
            einheitBefehl.Parameters.AddWithValue("$maxGeschw", e.MaxGeschwindigkeit);
            einheitBefehl.Parameters.AddWithValue("$hfDurchschnitt", e.HfDurchschnitt);
            einheitBefehl.Parameters.AddWithValue("$hfMax", e.HfMax);
            einheitBefehl.Parameters.AddWithValue("$hfMin", e.HfMin);
            einheitBefehl.Parameters.AddWithValue("$tf", e.Trittfrequenz);
            einheitBefehl.Parameters.AddWithValue("$temperatur", e.Temperatur);
            einheitBefehl.Parameters.AddWithValue("$leistung", e.LeistungWatt);
            einheitBefehl.Parameters.AddWithValue("$gewicht", e.KoerpergewichtKg);
            einheitBefehl.Parameters.AddWithValue("$erholungspuls", e.Erholungspuls);
            einheitBefehl.Parameters.AddWithValue("$bereich", e.Trainingsbereich);
            einheitBefehl.Parameters.AddWithValue("$kommentar", e.Kommentar);
            einheitBefehl.Parameters.AddWithValue("$aktivitaetId", e.AktivitaetId);
            einheitBefehl.Parameters.AddWithValue("$equipmentId", (object?)e.EquipmentId ?? DBNull.Value);
            einheitBefehl.ExecuteNonQuery();

            foreach (long kategorieId in e.KategorieIds)
            {
                kategorieBefehl.Parameters.Clear();
                kategorieBefehl.Parameters.AddWithValue("$einheitId", e.Id);
                kategorieBefehl.Parameters.AddWithValue("$kategorieId", kategorieId);
                kategorieBefehl.ExecuteNonQuery();
            }

            foreach (Anhang anhang in e.Anhaenge)
            {
                anhangBefehl.Parameters.Clear();
                anhangBefehl.Parameters.AddWithValue("$id", anhang.Id);
                anhangBefehl.Parameters.AddWithValue("$einheitId", e.Id);
                anhangBefehl.Parameters.AddWithValue("$typ", anhang.Typ.ToString());
                anhangBefehl.Parameters.AddWithValue("$pfad", anhang.RelativerPfad);
                anhangBefehl.ExecuteNonQuery();
            }
        }
    }
}

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
            SetzeWerte(einheitBefehl, e);
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

    /// <summary>
    /// Speichert eine neue Einheit (Befehle „Neue Einheit“, „Kopieren“ und Garmin-Import) und liefert ihre Nummer.
    /// </summary>
    /// <remarks>
    /// Die Nummer vergibt die Datenbank. Hat die Einheit noch keinen Sportler (Nummer 0, leerer Datenordner),
    /// wird der Standardsportler verwendet bzw. angelegt (<see cref="FahrerTabelle.Standardfahrer"/>).
    /// Einheit, Kategorien und Anhänge (beim Import die Aufzeichnung) werden in einer Transaktion geschrieben:
    /// Entweder alles oder nichts. Die Anhänge erhalten ihre Nummer ebenfalls von der Datenbank; sie wird in
    /// die übergebenen Anhänge eingetragen.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einheit">Die neue Einheit (ihre Nummer wird nicht verwendet).</param>
    /// <returns>Die vergebene Nummer und die Nummer des Sportlers.</returns>
    public static (long Id, long FahrerId) Einfuegen(SqliteConnection verbindung, Trainingseinheit einheit)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        long fahrerId = einheit.FahrerId > 0 ? einheit.FahrerId : FahrerTabelle.Standardfahrer(verbindung, transaktion);

        long id;
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.Transaction = transaktion;
            befehl.CommandText = """
                INSERT INTO Trainingseinheit (FahrerId, Datum, Startzeit, Name, FahrzeitSekunden, DistanzKm, Hoehenmeter,
                                              MaxHoehe, StartHoehe, MaxGeschwindigkeit, HfDurchschnitt, HfMax, HfMin,
                                              Trittfrequenz, Temperatur, LeistungWatt, KoerpergewichtKg, Erholungspuls,
                                              Trainingsbereich, Kommentar, AktivitaetId, EquipmentId)
                VALUES ($fahrerId, $datum, $startzeit, $name, $fahrzeit, $distanz, $hm,
                        $maxHoehe, $startHoehe, $maxGeschw, $hfDurchschnitt, $hfMax, $hfMin,
                        $tf, $temperatur, $leistung, $gewicht, $erholungspuls,
                        $bereich, $kommentar, $aktivitaetId, $equipmentId);
                SELECT last_insert_rowid();
                """;
            SetzeWerte(befehl, einheit);
            befehl.Parameters["$fahrerId"].Value = fahrerId;
            id = (long)befehl.ExecuteScalar()!;
        }

        SchreibeKategorien(verbindung, transaktion, id, einheit.KategorieIds);
        SchreibeNeueAnhaenge(verbindung, transaktion, id, einheit.Anhaenge);
        transaktion.Commit();
        return (id, fahrerId);
    }

    /// <summary>
    /// Speichert die geänderten Werte einer vorhandenen Einheit.
    /// </summary>
    /// <remarks>
    /// Die Kategorie-Zuordnungen werden ersetzt. Die Anhänge bleiben unberührt – das Bearbeiten ändert sie nicht.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einheit">Die Einheit mit ihrer Nummer.</param>
    /// <exception cref="InvalidOperationException">Die Einheit gibt es in der Datenbank nicht (mehr).</exception>
    public static void Aendern(SqliteConnection verbindung, Trainingseinheit einheit) => AendereMehrere(verbindung, [einheit]);

    /// <summary>
    /// Speichert die geänderten Werte mehrerer vorhandener Einheiten in einer Transaktion: entweder alle
    /// oder keine (z. B. nach „Kategorien zuordnen…“ für eine Mehrfachauswahl).
    /// </summary>
    /// <remarks>
    /// Je Einheit wie <see cref="Aendern"/>: Kategorie-Zuordnungen werden ersetzt, Anhänge bleiben unberührt.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="einheiten">Die Einheiten mit ihren Nummern.</param>
    /// <exception cref="InvalidOperationException">
    /// Eine der Einheiten gibt es in der Datenbank nicht (mehr); dann bleibt keine geändert.
    /// </exception>
    public static void AendereMehrere(SqliteConnection verbindung, IEnumerable<Trainingseinheit> einheiten)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        foreach (Trainingseinheit einheit in einheiten)
        {
            AendereInTransaktion(verbindung, transaktion, einheit);
        }

        transaktion.Commit();
    }

    /// <summary>Ändert eine Einheit samt Kategorien innerhalb einer laufenden Transaktion.</summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="einheit">Die Einheit mit ihrer Nummer.</param>
    /// <exception cref="InvalidOperationException">Die Einheit gibt es in der Datenbank nicht (mehr).</exception>
    private static void AendereInTransaktion(SqliteConnection verbindung, SqliteTransaction transaktion, Trainingseinheit einheit)
    {
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.Transaction = transaktion;
            befehl.CommandText = """
                UPDATE Trainingseinheit SET
                    FahrerId = $fahrerId, Datum = $datum, Startzeit = $startzeit, Name = $name,
                    FahrzeitSekunden = $fahrzeit, DistanzKm = $distanz, Hoehenmeter = $hm, MaxHoehe = $maxHoehe,
                    StartHoehe = $startHoehe, MaxGeschwindigkeit = $maxGeschw, HfDurchschnitt = $hfDurchschnitt,
                    HfMax = $hfMax, HfMin = $hfMin, Trittfrequenz = $tf, Temperatur = $temperatur,
                    LeistungWatt = $leistung, KoerpergewichtKg = $gewicht, Erholungspuls = $erholungspuls,
                    Trainingsbereich = $bereich, Kommentar = $kommentar, AktivitaetId = $aktivitaetId,
                    EquipmentId = $equipmentId
                WHERE Id = $id;
                """;
            SetzeWerte(befehl, einheit);
            befehl.Parameters.AddWithValue("$id", einheit.Id);
            if (befehl.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException($"Die Trainingseinheit Nr. {einheit.Id} ist nicht (mehr) vorhanden.");
            }
        }

        // Kategorien: alte Zuordnungen löschen, neue schreiben
        using (SqliteCommand loeschen = verbindung.CreateCommand())
        {
            loeschen.Transaction = transaktion;
            loeschen.CommandText = "DELETE FROM EinheitKategorie WHERE EinheitId = $id;";
            loeschen.Parameters.AddWithValue("$id", einheit.Id);
            loeschen.ExecuteNonQuery();
        }

        SchreibeKategorien(verbindung, transaktion, einheit.Id, einheit.KategorieIds);
    }

    /// <summary>
    /// Löscht die Einheiten mit den angegebenen Nummern.
    /// </summary>
    /// <remarks>
    /// Kategorie-Zuordnungen und Anhang-Einträge verschwinden über <c>ON DELETE CASCADE</c> mit. Die Dateien
    /// der Anhänge im Datenordner bleiben bewusst liegen: Ein versehentliches Löschen kostet so keine
    /// Aufzeichnung. Alle Einheiten werden in einer Transaktion gelöscht: Entweder alle oder keine.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="ids">Die Nummern der zu löschenden Einheiten.</param>
    public static void Loeschen(SqliteConnection verbindung, IEnumerable<long> ids)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "DELETE FROM Trainingseinheit WHERE Id = $id;";
        SqliteParameter id = befehl.Parameters.Add("$id", SqliteType.Integer);
        foreach (long nummer in ids)
        {
            id.Value = nummer;
            befehl.ExecuteNonQuery();
        }

        transaktion.Commit();
    }

    /// <summary>
    /// Ersetzt einen Trainingsbereich in allen Einheiten durch einen neuen (Umbenennen in den Auswahllisten).
    /// </summary>
    /// <remarks>
    /// Verglichen wird ohne Leerzeichen am Rand und ohne Unterschied zwischen Groß- und Kleinschreibung
    /// (SQLite <c>NOCASE</c>, d. h. für die Buchstaben A–Z – die Kürzel bestehen daraus und aus Ziffern).
    /// Läuft in der übergebenen Transaktion, damit Liste und Einheiten gemeinsam gespeichert werden.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="alt">Bisheriges Kürzel.</param>
    /// <param name="neu">Neues Kürzel.</param>
    /// <returns>Die Anzahl der geänderten Einheiten.</returns>
    public static int ErsetzeTrainingsbereich(SqliteConnection verbindung, SqliteTransaction transaktion, string alt, string neu)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "UPDATE Trainingseinheit SET Trainingsbereich = $neu WHERE TRIM(Trainingsbereich) = $alt COLLATE NOCASE;";
        befehl.Parameters.AddWithValue("$alt", alt.Trim());
        befehl.Parameters.AddWithValue("$neu", neu.Trim());
        return befehl.ExecuteNonQuery();
    }

    /// <summary>Schreibt die Kategorie-Zuordnungen einer Einheit.</summary>
    private static void SchreibeKategorien(SqliteConnection verbindung, SqliteTransaction transaktion, long einheitId, IEnumerable<long> kategorieIds)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "INSERT INTO EinheitKategorie (EinheitId, KategorieId) VALUES ($einheitId, $kategorieId);";
        befehl.Parameters.AddWithValue("$einheitId", einheitId);
        SqliteParameter kategorie = befehl.Parameters.Add("$kategorieId", SqliteType.Integer);
        foreach (long kategorieId in kategorieIds.Distinct())
        {
            kategorie.Value = kategorieId;
            befehl.ExecuteNonQuery();
        }
    }

    /// <summary>Schreibt die Anhänge einer neuen Einheit und trägt die vergebenen Nummern ein.</summary>
    private static void SchreibeNeueAnhaenge(SqliteConnection verbindung, SqliteTransaction transaktion, long einheitId, IEnumerable<Anhang> anhaenge)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Anhang (EinheitId, Typ, RelativerPfad) VALUES ($einheitId, $typ, $pfad);
            SELECT last_insert_rowid();
            """;
        befehl.Parameters.AddWithValue("$einheitId", einheitId);
        SqliteParameter typ = befehl.Parameters.Add("$typ", SqliteType.Text);
        SqliteParameter pfad = befehl.Parameters.Add("$pfad", SqliteType.Text);
        foreach (Anhang anhang in anhaenge)
        {
            typ.Value = anhang.Typ.ToString();
            pfad.Value = anhang.RelativerPfad;
            anhang.Id = (long)befehl.ExecuteScalar()!;
        }
    }

    /// <summary>
    /// Setzt alle Werte einer Einheit als Parameter (ohne <c>$id</c>); gemeinsam genutzt von Speichern,
    /// Einfügen und Ändern, damit alle drei die Werte gleich schreiben.
    /// </summary>
    private static void SetzeWerte(SqliteCommand einheitBefehl, Trainingseinheit e)
    {
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
    }
}

using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Trainingseinheiten samt Kategorie-Zuordnungen und Anhängen ein.
/// </summary>
public static class SpeichereTrainingseinheiten
{
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
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Trainingseinheit> einheiten)
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
            einheitBefehl.Parameters.AddWithValue("$datum", DatumAlsText.Ausfuehren(e.Datum));
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

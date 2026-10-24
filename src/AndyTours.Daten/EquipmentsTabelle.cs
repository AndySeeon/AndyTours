using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Equipment</c> samt Wartungsintervallen.
/// </summary>
public static class EquipmentsTabelle
{
    /// <summary>
    /// Liest alle Equipments (auch ausgemusterte), sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Equipments.</returns>
    public static List<Equipment> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Name, AktivitaetId, GewichtKg, RadumfangMm, CrCoeff, CmCoeff, CwCoeff, CwaCoeff,
                   SinCoeff, Kettenblaetter, Ritzel, Geraetezuordnung, StandardKategorie, IstStandard,
                   Ausgemustert, Anschaffungsdatum, Bildpfad, AnfangsdistanzKm
            FROM Equipment ORDER BY Id;
            """;

        // Wandelt den gespeicherten Text „39,53" zurück in eine Zahlenliste.
        static List<int> Zahlenliste(string text) =>
            text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        var equipments = new List<Equipment>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            equipments.Add(new Equipment
            {
                Id = leser.GetInt64(0),
                Name = leser.GetString(1),
                AktivitaetId = leser.IsDBNull(2) ? null : leser.GetInt64(2),
                GewichtKg = leser.GetDouble(3),
                RadumfangMm = leser.GetInt32(4),
                CrCoeff = leser.GetDouble(5),
                CmCoeff = leser.GetDouble(6),
                CwCoeff = leser.GetDouble(7),
                CwaCoeff = leser.GetDouble(8),
                SinCoeff = leser.GetDouble(9),
                Kettenblaetter = Zahlenliste(leser.GetString(10)),
                Ritzel = Zahlenliste(leser.GetString(11)),
                Geraetezuordnung = leser.GetString(12),
                StandardKategorie = leser.GetString(13),
                IstStandard = leser.GetBoolean(14),
                Ausgemustert = leser.GetBoolean(15),
                Anschaffungsdatum = DatumSpalte.Lesen(leser, 16),
                Bildpfad = leser.GetString(17),
                AnfangsdistanzKm = leser.GetDouble(18),
            });
        }

        return equipments;
    }

    /// <summary>
    /// Fügt alle übergebenen Equipments mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="equipments">Die Equipments.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Equipment> equipments)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Equipment (Id, Name, AktivitaetId, GewichtKg, RadumfangMm, CrCoeff, CmCoeff, CwCoeff, CwaCoeff,
                                   SinCoeff, Kettenblaetter, Ritzel, Geraetezuordnung, StandardKategorie, IstStandard,
                                   Ausgemustert, Anschaffungsdatum, Bildpfad, AnfangsdistanzKm)
            VALUES ($id, $name, $aktivitaetId, $gewicht, $radumfang, $cr, $cm, $cw, $cwa,
                    $sin, $kettenblaetter, $ritzel, $zuordnung, $standardKategorie, $istStandard,
                    $ausgemustert, $anschaffung, $bild, $anfangsdistanz);
            """;

        foreach (Equipment e in equipments)
        {
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", e.Id);
            befehl.Parameters.AddWithValue("$name", e.Name);
            befehl.Parameters.AddWithValue("$aktivitaetId", (object?)e.AktivitaetId ?? DBNull.Value);
            befehl.Parameters.AddWithValue("$gewicht", e.GewichtKg);
            befehl.Parameters.AddWithValue("$radumfang", e.RadumfangMm);
            befehl.Parameters.AddWithValue("$cr", e.CrCoeff);
            befehl.Parameters.AddWithValue("$cm", e.CmCoeff);
            befehl.Parameters.AddWithValue("$cw", e.CwCoeff);
            befehl.Parameters.AddWithValue("$cwa", e.CwaCoeff);
            befehl.Parameters.AddWithValue("$sin", e.SinCoeff);
            // Zähnezahlen als kommagetrennter Text, z. B. „39,53"
            befehl.Parameters.AddWithValue("$kettenblaetter", string.Join(',', e.Kettenblaetter));
            befehl.Parameters.AddWithValue("$ritzel", string.Join(',', e.Ritzel));
            befehl.Parameters.AddWithValue("$zuordnung", e.Geraetezuordnung);
            befehl.Parameters.AddWithValue("$standardKategorie", e.StandardKategorie);
            befehl.Parameters.AddWithValue("$istStandard", e.IstStandard);
            befehl.Parameters.AddWithValue("$ausgemustert", e.Ausgemustert);
            befehl.Parameters.AddWithValue("$anschaffung", DatumSpalte.AlsText(e.Anschaffungsdatum));
            befehl.Parameters.AddWithValue("$bild", e.Bildpfad);
            befehl.Parameters.AddWithValue("$anfangsdistanz", e.AnfangsdistanzKm);
            befehl.ExecuteNonQuery();
        }
    }
}

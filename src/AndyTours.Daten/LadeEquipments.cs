using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Equipments aus der Tabelle <c>Equipment</c>.
/// </summary>
public static class LadeEquipments
{
    /// <summary>
    /// Liest alle Equipments (auch ausgemusterte), sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Equipments.</returns>
    public static List<Equipment> Ausfuehren(SqliteConnection verbindung)
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
                Anschaffungsdatum = TextAlsDatum.Ausfuehren(leser, 16),
                Bildpfad = leser.GetString(17),
                AnfangsdistanzKm = leser.GetDouble(18),
            });
        }

        return equipments;
    }
}

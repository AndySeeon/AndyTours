using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Equipments in die Tabelle <c>Equipment</c> ein.
/// </summary>
public static class SpeichereEquipments
{
    /// <summary>
    /// Fügt alle übergebenen Equipments mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="equipments">Die Equipments.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Equipment> equipments)
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
            befehl.Parameters.AddWithValue("$anschaffung", DatumAlsText.Ausfuehren(e.Anschaffungsdatum));
            befehl.Parameters.AddWithValue("$bild", e.Bildpfad);
            befehl.Parameters.AddWithValue("$anfangsdistanz", e.AnfangsdistanzKm);
            befehl.ExecuteNonQuery();
        }
    }
}

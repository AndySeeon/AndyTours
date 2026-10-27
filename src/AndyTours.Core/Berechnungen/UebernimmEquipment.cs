using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Übernimmt ein neues oder geändertes Equipment in den Datenbestand und hält dabei die Regeln für
/// „Standard“ ein.
/// </summary>
/// <remarks>
/// Zwei Schritte, weil zuerst die Datenbank geschrieben und erst nach Erfolg der Speicher geändert wird:
/// <see cref="ErmittleAenderungen"/> liefert alle zu schreibenden Equipments, ohne den Bestand anzufassen;
/// <see cref="Einsetzen"/> setzt sie danach in den Bestand ein.
/// </remarks>
public static class UebernimmEquipment
{
    /// <summary>
    /// Ermittelt die Equipments, die für dieses Equipment gespeichert werden müssen.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Ein ausgemustertes Equipment ist nie Standard; die Kennzeichnung fällt weg.</item>
    /// <item>Ist das Equipment Standard, verlieren alle anderen Equipments derselben Aktivität die
    /// Kennzeichnung – auch ausgemusterte aus dem Altbestand, damit je Aktivität genau eines übrig bleibt.</item>
    /// </list>
    /// Die gelieferten Equipments sind Kopien; der Bestand bleibt unverändert.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipment">Das neue (Nummer 0) oder geänderte Equipment.</param>
    /// <returns>Das Equipment selbst an erster Stelle, danach die Equipments, deren Standard wegfällt.</returns>
    public static List<Equipment> ErmittleAenderungen(Datenbestand bestand, Equipment equipment)
    {
        Equipment eigenes = Kopie(equipment);
        if (eigenes.Ausgemustert)
        {
            eigenes.IstStandard = false;
        }

        List<Equipment> aenderungen = [eigenes];
        if (eigenes.IstStandard)
        {
            foreach (Equipment anderes in bestand.Equipments.Where(e =>
                         e.Id != eigenes.Id && e.IstStandard && e.AktivitaetId == eigenes.AktivitaetId))
            {
                Equipment ohneStandard = Kopie(anderes);
                ohneStandard.IstStandard = false;
                aenderungen.Add(ohneStandard);
            }
        }

        return aenderungen;
    }

    /// <summary>
    /// Setzt gespeicherte Equipments in den Bestand ein: vorhandene werden ersetzt, neue angehängt.
    /// </summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipments">Die gespeicherten Equipments mit ihrer endgültigen Nummer.</param>
    public static void Einsetzen(Datenbestand bestand, IEnumerable<Equipment> equipments)
    {
        foreach (Equipment equipment in equipments)
        {
            int stelle = bestand.Equipments.FindIndex(e => e.Id == equipment.Id);
            if (stelle >= 0)
            {
                bestand.Equipments[stelle] = equipment;
            }
            else
            {
                bestand.Equipments.Add(equipment);
            }
        }
    }

    /// <summary>Erzeugt eine unabhängige Kopie mit eigenen Listen.</summary>
    /// <param name="e">Das zu kopierende Equipment.</param>
    /// <returns>Die Kopie.</returns>
    public static Equipment Kopie(Equipment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        AktivitaetId = e.AktivitaetId,
        GewichtKg = e.GewichtKg,
        RadumfangMm = e.RadumfangMm,
        CrCoeff = e.CrCoeff,
        CmCoeff = e.CmCoeff,
        CwCoeff = e.CwCoeff,
        CwaCoeff = e.CwaCoeff,
        SinCoeff = e.SinCoeff,
        Kettenblaetter = [.. e.Kettenblaetter],
        Ritzel = [.. e.Ritzel],
        Geraetezuordnung = e.Geraetezuordnung,
        StandardKategorie = e.StandardKategorie,
        IstStandard = e.IstStandard,
        Ausgemustert = e.Ausgemustert,
        Anschaffungsdatum = e.Anschaffungsdatum,
        Bildpfad = e.Bildpfad,
        AnfangsdistanzKm = e.AnfangsdistanzKm,
    };
}

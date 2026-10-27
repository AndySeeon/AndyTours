using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bereitet eine neue Trainingseinheit für den Befehl „Neue Einheit“ vor.
/// </summary>
public static class ErstelleNeueEinheit
{
    /// <summary>
    /// Liefert eine neue, noch nicht gespeicherte Einheit mit den Vorbelegungen.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Datum: heute.</item>
    /// <item>Aktivität: die in der Werkzeugleiste gewählte; bei „alle Aktivitäten“ (<c>null</c>) die
    /// Hauptaktivität. Gibt es keine, bleibt die Aktivität offen (Nummer 0) und muss im Fenster gewählt werden.</item>
    /// <item>Kategorien: alle als Standard markierten.</item>
    /// <item>Equipment: das Standard-Equipment der Aktivität (siehe <see cref="ErmittleStandardEquipment"/>);
    /// hat es eine Standardkategorie, wird diese zusätzlich angehakt. Ohne Standard-Equipment bleibt es leer.</item>
    /// <item>Fahrer: der erste vorhandene (AndyTours hat nur einen); fehlt er, wird er beim Speichern angelegt.</item>
    /// <item>Alles andere bleibt leer bzw. 0 („nicht erfasst“).</item>
    /// </list>
    /// Die Nummer ist 0; die Datenbank vergibt sie beim Speichern.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Aktivität der Werkzeugleiste oder <c>null</c> für „alle Aktivitäten“.</param>
    /// <param name="heute">Das heutige Datum.</param>
    /// <returns>Die vorbereitete Einheit.</returns>
    public static Trainingseinheit Ausfuehren(Datenbestand bestand, long? aktivitaetId, DateOnly heute)
    {
        long aktivitaet = aktivitaetId ?? bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id ?? 0;
        long? equipmentId = ErmittleStandardEquipment.Ausfuehren(bestand, aktivitaet)?.Id;
        List<long> kategorien = bestand.Kategorien.Where(k => k.IstStandard).Select(k => k.Id).ToList();
        if (FindeStandardkategorie.Ausfuehren(bestand, equipmentId) is long kategorieId && !kategorien.Contains(kategorieId))
        {
            kategorien.Add(kategorieId);
        }

        return new Trainingseinheit
        {
            Datum = heute,
            AktivitaetId = aktivitaet,
            KategorieIds = kategorien,
            EquipmentId = equipmentId,
            FahrerId = bestand.Fahrer.FirstOrDefault()?.Id ?? 0,
        };
    }
}

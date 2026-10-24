using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Führt zwei Aktivitäten zusammen: Die Ursprungsaktivität geht in der Zielaktivität auf.
/// </summary>
/// <remarks>
/// Alle Einheiten und Equipments der Ursprungsaktivität gehören danach zur Zielaktivität; war die
/// Ursprungsaktivität die Hauptaktivität, wird es die Zielaktivität. Die Saisonziele der
/// Ursprungsaktivität entfallen; es gelten die der Zielaktivität. Zum Schluss wird die
/// Ursprungsaktivität aus der Liste entfernt.
/// </remarks>
public static class FuehreAktivitaetenZusammen
{
    /// <summary>
    /// Führt die Zusammenführung im Datenbestand aus.
    /// </summary>
    /// <param name="bestand">Der Datenbestand; er wird geändert.</param>
    /// <param name="ursprungId">Id der Aktivität, die entfernt wird.</param>
    /// <param name="zielId">Id der Aktivität, die bleibt.</param>
    /// <returns>Anzahl der umgehängten Einheiten.</returns>
    public static int Ausfuehren(Datenbestand bestand, long ursprungId, long zielId)
    {
        Aktivitaet? ursprung = bestand.Aktivitaeten.FirstOrDefault(a => a.Id == ursprungId);
        Aktivitaet? ziel = bestand.Aktivitaeten.FirstOrDefault(a => a.Id == zielId);
        if (ursprung is null || ziel is null || ursprungId == zielId)
        {
            return 0;
        }

        int anzahl = 0;
        foreach (Trainingseinheit einheit in bestand.Trainingseinheiten.Where(e => e.AktivitaetId == ursprungId))
        {
            einheit.AktivitaetId = zielId;
            anzahl++;
        }

        foreach (Equipment equipment in bestand.Equipments.Where(e => e.AktivitaetId == ursprungId))
        {
            equipment.AktivitaetId = zielId;
        }

        if (ursprung.IstHaupt)
        {
            ziel.IstHaupt = true;
        }

        // In der Datenbank entfallen die Ziele mit der Aktivität (ON DELETE CASCADE), hier ebenso
        bestand.Saisonziele.RemoveAll(z => z.AktivitaetId == ursprungId);
        bestand.Aktivitaeten.Remove(ursprung);
        return anzahl;
    }
}

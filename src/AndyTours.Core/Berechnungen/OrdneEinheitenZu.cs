using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ordnet mehreren Einheiten auf einmal Kategorien, eine Aktivität oder ein Equipment zu.
/// </summary>
/// <remarks>
/// Jede Methode liefert geänderte Kopien und lässt die übergebenen Einheiten unverändert: Zuerst wird die
/// Datenbank geschrieben, erst nach Erfolg ersetzt das Hauptfenster die Einheiten im Bestand. Nummer und
/// Anhänge bleiben erhalten, alle übrigen Werte auch.
/// </remarks>
public static class OrdneEinheitenZu
{
    /// <summary>
    /// Setzt bzw. entfernt Kategorien bei allen Einheiten.
    /// </summary>
    /// <remarks>
    /// Kategorien, die weder gesetzt noch entfernt werden („teilweise“ belassen), bleiben bei jeder Einheit,
    /// wie sie sind. Eine Kategorie kommt nie doppelt vor.
    /// </remarks>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <param name="setzen">Kategorien, die danach alle Einheiten haben.</param>
    /// <param name="entfernen">Kategorien, die danach keine Einheit mehr hat.</param>
    /// <returns>Die geänderten Kopien in derselben Reihenfolge.</returns>
    public static List<Trainingseinheit> Kategorien(IEnumerable<Trainingseinheit> einheiten, IReadOnlyCollection<long> setzen, IReadOnlyCollection<long> entfernen) =>
        einheiten.Select(e =>
        {
            Trainingseinheit kopie = Kopie(e);
            kopie.KategorieIds = [.. kopie.KategorieIds.Where(id => !entfernen.Contains(id)).Concat(setzen).Distinct()];
            return kopie;
        }).ToList();

    /// <summary>
    /// Stellt alle Einheiten auf eine Aktivität um; das Equipment bleibt, auch wenn es nicht dazu passt.
    /// </summary>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <param name="aktivitaetId">Die neue Aktivität.</param>
    /// <returns>Die geänderten Kopien in derselben Reihenfolge.</returns>
    public static List<Trainingseinheit> Aktivitaet(IEnumerable<Trainingseinheit> einheiten, long aktivitaetId) =>
        einheiten.Select(e =>
        {
            Trainingseinheit kopie = Kopie(e);
            kopie.AktivitaetId = aktivitaetId;
            return kopie;
        }).ToList();

    /// <summary>
    /// Ordnet allen Einheiten ein Equipment zu (oder keines); die Kategorien bleiben unverändert.
    /// </summary>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <param name="equipmentId">Das neue Equipment oder <c>null</c> für „(kein)“.</param>
    /// <returns>Die geänderten Kopien in derselben Reihenfolge.</returns>
    public static List<Trainingseinheit> Equipment(IEnumerable<Trainingseinheit> einheiten, long? equipmentId) =>
        einheiten.Select(e =>
        {
            Trainingseinheit kopie = Kopie(e);
            kopie.EquipmentId = equipmentId;
            return kopie;
        }).ToList();

    /// <summary>Unabhängige Kopie mit derselben Nummer, demselben Datum und denselben Anhängen.</summary>
    private static Trainingseinheit Kopie(Trainingseinheit e)
    {
        Trainingseinheit kopie = KopiereEinheit.Ausfuehren(e, e.Datum);
        kopie.Id = e.Id;
        kopie.Anhaenge = [.. e.Anhaenge];
        return kopie;
    }
}

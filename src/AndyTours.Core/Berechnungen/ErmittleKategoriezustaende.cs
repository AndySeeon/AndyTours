using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt für das Fenster „Kategorien zuordnen“, welche Kategorien die markierten Einheiten haben.
/// </summary>
public static class ErmittleKategoriezustaende
{
    /// <summary>
    /// Liefert je Kategorie, ob alle, keine oder nur manche der Einheiten sie haben.
    /// </summary>
    /// <remarks>
    /// Die Kategorien kommen alphabetisch, wie im Bearbeitungsfenster einer Einheit. Ohne Einheiten gilt
    /// jede Kategorie als „keine“.
    /// </remarks>
    /// <param name="kategorien">Alle Kategorien des Datenbestands.</param>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <returns>
    /// Je Kategorie: <c>true</c>, wenn alle Einheiten sie haben, <c>false</c>, wenn keine sie hat, und
    /// <c>null</c>, wenn nur manche sie haben.
    /// </returns>
    public static List<(Kategorie Kategorie, bool? Zustand)> Ausfuehren(IEnumerable<Kategorie> kategorien, IReadOnlyCollection<Trainingseinheit> einheiten) =>
        kategorien
            .OrderBy(k => k.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(k =>
            {
                int anzahl = einheiten.Count(e => e.KategorieIds.Contains(k.Id));
                bool? zustand = anzahl == 0 ? false : anzahl == einheiten.Count ? true : null;
                return (k, zustand);
            })
            .ToList();
}

using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Blendet eine Spalte der Tabelle „Trainingseinheiten“ ein oder aus.
/// </summary>
public static class SchalteSpalteUm
{
    /// <summary>
    /// Liefert die neue Menge der ausgeblendeten Spalten, nachdem <paramref name="spalte"/> umgeschaltet wurde.
    /// </summary>
    /// <remarks>
    /// Eine ausgeblendete Spalte wird wieder sichtbar, eine sichtbare wird ausgeblendet – außer sie ist
    /// die letzte sichtbare: Dann bliebe eine Tabelle ohne Spalten übrig, deren Überschrift sich nicht
    /// mehr anklicken ließe. In diesem Fall bleibt die Menge unverändert.
    /// Die übergebene Menge wird nicht verändert.
    /// </remarks>
    /// <param name="ausgeblendet">Die bisher ausgeblendeten Spalten.</param>
    /// <param name="spalte">Die umzuschaltende Spalte.</param>
    /// <returns>Die neue Menge der ausgeblendeten Spalten.</returns>
    public static HashSet<Einheitenspalte> Ausfuehren(IReadOnlySet<Einheitenspalte> ausgeblendet, Einheitenspalte spalte)
    {
        var neu = new HashSet<Einheitenspalte>(ausgeblendet);
        if (neu.Remove(spalte))
        {
            return neu; // war ausgeblendet → jetzt sichtbar
        }

        int sichtbar = Enum.GetValues<Einheitenspalte>().Count(s => !neu.Contains(s));
        if (sichtbar > 1)
        {
            neu.Add(spalte);
        }

        return neu;
    }
}

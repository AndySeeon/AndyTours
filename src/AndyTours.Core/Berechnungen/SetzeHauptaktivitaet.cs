using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Macht eine Aktivität zur Hauptaktivität; alle anderen verlieren das Kennzeichen.
/// </summary>
/// <remarks>
/// Die Hauptaktivität ist beim Start in der Werkzeugleiste vorgewählt und wird bei „Neue Einheit“
/// unter „alle Aktivitäten“ vorbelegt. Es gibt deshalb genau eine.
/// </remarks>
public static class SetzeHauptaktivitaet
{
    /// <summary>
    /// Setzt das Kennzeichen <see cref="Aktivitaet.IstHaupt"/> genau bei der Aktivität mit dieser Id.
    /// </summary>
    /// <param name="aktivitaeten">Alle Aktivitäten; sie werden geändert.</param>
    /// <param name="aktivitaetId">Id der neuen Hauptaktivität.</param>
    public static void Ausfuehren(IEnumerable<Aktivitaet> aktivitaeten, long aktivitaetId)
    {
        foreach (Aktivitaet aktivitaet in aktivitaeten)
        {
            aktivitaet.IstHaupt = aktivitaet.Id == aktivitaetId;
        }
    }
}

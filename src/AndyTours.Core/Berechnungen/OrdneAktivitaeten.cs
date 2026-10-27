using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bringt die Aktivitäten in die vom Benutzer festgelegte Reihenfolge.
/// </summary>
/// <remarks>
/// Die Reihenfolge ist als Liste der Ids gespeichert, damit sie ein Umbenennen übersteht. Aktivitäten,
/// deren Id dort fehlt (z. B. weil die Reihenfolge noch nie gespeichert wurde), kommen hinten in der
/// Reihenfolge ihrer Id dazu; Ids ohne passende Aktivität werden übergangen.
/// </remarks>
public static class OrdneAktivitaeten
{
    /// <summary>
    /// Liefert die Aktivitäten in der festgelegten Reihenfolge; die übergebene Liste bleibt unverändert.
    /// </summary>
    /// <param name="aktivitaeten">Alle Aktivitäten (in beliebiger Reihenfolge).</param>
    /// <param name="reihenfolge">Die gespeicherten Ids in der Reihenfolge des Benutzers; leer = Id-Reihenfolge.</param>
    /// <returns>Die geordnete Liste.</returns>
    public static List<Aktivitaet> Ausfuehren(IEnumerable<Aktivitaet> aktivitaeten, IReadOnlyList<long> reihenfolge)
    {
        // Stelle jeder Id in der gespeicherten Liste; doppelte Ids zählen an ihrer ersten Stelle
        var stelle = new Dictionary<long, int>();
        for (int i = 0; i < reihenfolge.Count; i++)
        {
            stelle.TryAdd(reihenfolge[i], i);
        }

        return aktivitaeten
            .OrderBy(a => stelle.TryGetValue(a.Id, out int s) ? s : int.MaxValue)
            .ThenBy(a => a.Id)
            .ToList();
    }
}

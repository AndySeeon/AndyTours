using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Sammelt den gesamten Inhalt eines Overlay-Ordners: alle Unterordner und alle Overlays, auf allen Ebenen.
/// </summary>
/// <remarks>
/// Gebraucht beim Ein- und Ausschalten eines Ordners (wirkt auf alle Overlays darin) und für die Rückfrage beim
/// Löschen eines Ordners, die nennt, wie viel mitgelöscht wird.
/// </remarks>
public static class SammleOrdnerinhalt
{
    /// <summary>
    /// Liefert die Unterordner (ohne den Ordner selbst) und die Overlays des Ordners und seiner Unterordner.
    /// </summary>
    /// <param name="ordnerId">Der Ordner.</param>
    /// <param name="ordner">Alle vorhandenen Ordner.</param>
    /// <param name="overlays">Alle vorhandenen Overlays.</param>
    /// <returns>Die Nummern der Unterordner und die enthaltenen Overlays in der Reihenfolge der Eingabe.</returns>
    public static (IReadOnlySet<long> Unterordner, IReadOnlyList<Overlay> Overlays) Ausfuehren(
        long ordnerId, IReadOnlyList<Overlayordner> ordner, IReadOnlyList<Overlay> overlays)
    {
        var unterordner = new HashSet<long>();
        var offen = new Queue<long>([ordnerId]);
        while (offen.TryDequeue(out long aktuell))
        {
            // Ein Kreis in den Daten (oder der Ordner selbst als Kind) wird so nur einmal besucht
            foreach (Overlayordner kind in ordner.Where(o => o.ElternId == aktuell && o.Id != ordnerId))
            {
                if (unterordner.Add(kind.Id))
                {
                    offen.Enqueue(kind.Id);
                }
            }
        }

        List<Overlay> enthalten = [.. overlays.Where(o => o.OrdnerId is long id && (id == ordnerId || unterordner.Contains(id)))];
        return (unterordner, enthalten);
    }
}

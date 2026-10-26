using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft, ob ein Overlay-Ordner in einen anderen Ordner verschoben werden darf.
/// </summary>
public static class PruefeVerschiebeziel
{
    /// <summary>
    /// Liefert <c>false</c>, wenn das Ziel der Ordner selbst oder einer seiner Unterordner (auf beliebiger Ebene) ist –
    /// der Ordner hinge dann an sich selbst und verschwände aus dem Baum.
    /// </summary>
    /// <param name="ordnerId">Der zu verschiebende Ordner.</param>
    /// <param name="zielId">Der Zielordner; <c>null</c> = oberste Ebene (immer erlaubt).</param>
    /// <param name="ordner">Alle vorhandenen Ordner.</param>
    /// <returns><c>true</c>, wenn das Verschieben erlaubt ist.</returns>
    public static bool Ausfuehren(long ordnerId, long? zielId, IReadOnlyList<Overlayordner> ordner)
    {
        Dictionary<long, long?> eltern = ordner.ToDictionary(o => o.Id, o => o.ElternId);
        var besucht = new HashSet<long>();

        // Vom Ziel aus die übergeordneten Ordner hinaufgehen; trifft der Weg den Ordner, ist das Ziel sein Unterordner.
        // „besucht“ schützt vor einer Endlosschleife, falls die Daten doch einmal einen Kreis enthalten.
        for (long? aktuell = zielId; aktuell is long id && besucht.Add(id); aktuell = eltern.GetValueOrDefault(id))
        {
            if (id == ordnerId)
            {
                return false;
            }
        }

        return true;
    }
}

using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet die Auswahlliste der Zielordner für „Verschieben nach…“ in der Overlay-Verwaltung.
/// </summary>
public static class BildeOrdnerpfade
{
    /// <summary>Trennzeichen zwischen den Ordnernamen eines Pfads.</summary>
    public const string Trenner = " › ";

    /// <summary>Angezeigter Name des Eintrags für die oberste Ebene.</summary>
    public const string ObersteEbene = "(oberste Ebene)";

    /// <summary>
    /// Liefert „(oberste Ebene)“ und danach jeden Ordner mit seinem vollen Pfad, z. B. „Alpen › Tirol“, alphabetisch
    /// nach dem Pfad sortiert.
    /// </summary>
    /// <remarks>
    /// Wird ein Ordner verschoben, fehlen er selbst und seine Unterordner in der Liste
    /// (<see cref="PruefeVerschiebeziel"/>), damit kein unzulässiges Ziel gewählt werden kann.
    /// </remarks>
    /// <param name="ordner">Alle vorhandenen Ordner.</param>
    /// <param name="verschobenerOrdnerId">Der zu verschiebende Ordner; <c>null</c>, wenn Overlays verschoben werden.</param>
    /// <returns>Die Einträge; <see cref="Auswahleintrag.Id"/> ist <c>null</c> für die oberste Ebene.</returns>
    public static IReadOnlyList<Auswahleintrag> Ausfuehren(IReadOnlyList<Overlayordner> ordner, long? verschobenerOrdnerId = null)
    {
        Dictionary<long, Overlayordner> nachId = ordner.ToDictionary(o => o.Id);
        StringComparer vergleich = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true);

        IEnumerable<Auswahleintrag> pfade = ordner
            .Where(o => verschobenerOrdnerId is not long id || PruefeVerschiebeziel.Ausfuehren(id, o.Id, ordner))
            .Select(o => new Auswahleintrag(o.Id, Pfad(o, nachId)))
            .OrderBy(e => e.Name, vergleich);
        return [new Auswahleintrag(null, ObersteEbene), .. pfade];
    }

    /// <summary>Der Pfad eines Ordners von der obersten Ebene bis zu ihm selbst.</summary>
    private static string Pfad(Overlayordner ordner, Dictionary<long, Overlayordner> nachId)
    {
        var namen = new List<string>();
        var besucht = new HashSet<long>();
        for (Overlayordner? aktuell = ordner; aktuell is not null && besucht.Add(aktuell.Id);
             aktuell = aktuell.ElternId is long eltern ? nachId.GetValueOrDefault(eltern) : null)
        {
            namen.Add(aktuell.Name);
        }

        namen.Reverse();
        return string.Join(Trenner, namen);
    }
}

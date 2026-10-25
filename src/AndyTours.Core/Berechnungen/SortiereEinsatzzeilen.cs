using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Sortiert die Zeilen der Auskunft „Equipment-Einsatz“ nach einer Spalte; die Zeile „Summe“ bleibt unten.
/// </summary>
public static class SortiereEinsatzzeilen
{
    /// <summary>
    /// Liefert die Zeilen nach der Spalte sortiert, die Zeile „Summe“ immer am Ende.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Zahlenspalten werden nach dem Zahlenwert sortiert; eine leere Zelle zählt als 0.</item>
    /// <item>„Equipment“ wird alphabetisch ohne Unterschied der Groß- und Kleinschreibung sortiert.</item>
    /// <item>Bei gleichem Wert bleibt die bisherige Reihenfolge erhalten.</item>
    /// </list>
    /// </remarks>
    /// <param name="zeilen">Die Zeilen der Auskunft.</param>
    /// <param name="spalte">Die Spalte, nach der sortiert wird.</param>
    /// <param name="aufsteigend"><c>true</c> = aufsteigend, <c>false</c> = absteigend.</param>
    /// <returns>Die sortierten Zeilen.</returns>
    public static List<Einsatzzeile> Ausfuehren(IEnumerable<Einsatzzeile> zeilen, Einsatzspalte spalte, bool aufsteigend)
    {
        List<Einsatzzeile> liste = zeilen.ToList();
        List<Einsatzzeile> ohneSumme = liste.Where(z => !z.IstSumme).ToList();

        IEnumerable<Einsatzzeile> sortiert = spalte == Einsatzspalte.Equipment
            ? Sortiere(ohneSumme, z => z.Name, StringComparer.CurrentCultureIgnoreCase, aufsteigend)
            : Sortiere(ohneSumme, z => Zahlenwert(z, spalte), Comparer<double>.Default, aufsteigend);

        // Die Summe hängt immer unten an
        return sortiert.Concat(liste.Where(z => z.IstSumme)).ToList();
    }

    /// <summary>Sortiert stabil auf- oder absteigend nach einem Schlüssel.</summary>
    private static IEnumerable<Einsatzzeile> Sortiere<T>(List<Einsatzzeile> zeilen, Func<Einsatzzeile, T> schluessel, IComparer<T> vergleich, bool aufsteigend) =>
        aufsteigend ? zeilen.OrderBy(schluessel, vergleich) : zeilen.OrderByDescending(schluessel, vergleich);

    /// <summary>Liefert den Wert einer Zahlenspalte; eine leere Zelle zählt als 0.</summary>
    private static double Zahlenwert(Einsatzzeile zeile, Einsatzspalte spalte) => spalte switch
    {
        Einsatzspalte.VorherKm => zeile.VorherKm ?? 0,
        Einsatzspalte.GesamtKm => zeile.GesamtKm ?? 0,
        Einsatzspalte.GesamtHm => zeile.GesamtHm ?? 0,
        _ => throw new ArgumentOutOfRangeException(nameof(spalte), spalte, "Keine Zahlenspalte."),
    };
}

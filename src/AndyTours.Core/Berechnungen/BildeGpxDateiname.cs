using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Schlägt den Dateinamen für „Als GPX exportieren…“ vor.
/// </summary>
public static class BildeGpxDateiname
{
    /// <summary>Zeichen, die Windows in Dateinamen nicht erlaubt.</summary>
    private const string Unzulaessig = "\\/:*?\"<>|";

    /// <summary>
    /// Bildet den Dateinamen aus Datum und Name, z. B. „2026-05-01 Bründlingalm.gpx“.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Bei mehreren Etappen stehen das erste und das letzte Datum davor, z. B.
    /// „2026-05-01 bis 2026-05-04 Alpencross.gpx“; der Name stammt von der ersten Etappe. Es zählen alle Etappen,
    /// auch solche ohne GPS-Daten, damit der Name zur Kopfzeile des Tourdetails passt.</item>
    /// <item>Liegen alle Etappen am selben Tag, steht nur dieses eine Datum davor.</item>
    /// <item>Zeichen, die in Dateinamen nicht erlaubt sind (z. B. „/“), sowie Steuerzeichen werden durch „-“
    /// ersetzt. Ohne Namen besteht der Dateiname nur aus dem Datum.</item>
    /// </list>
    /// </remarks>
    /// <param name="etappen">Die Etappen des Tourdetails in ihrer Reihenfolge; mindestens eine.</param>
    /// <returns>Der vorgeschlagene Dateiname mit der Endung „.gpx“.</returns>
    public static string Ausfuehren(IReadOnlyList<Etappe> etappen)
    {
        DateOnly erstes = etappen.Min(e => e.Einheit.Datum);
        DateOnly letztes = etappen.Max(e => e.Einheit.Datum);
        string datum = erstes == letztes ? Datum(erstes) : $"{Datum(erstes)} bis {Datum(letztes)}";

        string name = new(etappen[0].Einheit.Name.Trim()
            .Select(zeichen => Unzulaessig.Contains(zeichen) || char.IsControl(zeichen) ? '-' : zeichen)
            .ToArray());
        return (name.Length == 0 ? datum : $"{datum} {name}") + ".gpx";
    }

    /// <summary>Datum als „2026-05-01“.</summary>
    private static string Datum(DateOnly datum) => datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

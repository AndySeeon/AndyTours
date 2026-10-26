using System.Globalization;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet den Dateinamen, den „Speichern…“ im Fenster „Tour planen“ für eine neue Planung vorschlägt.
/// </summary>
public static class BildePlanungsdateiname
{
    /// <summary>
    /// Liefert den Vorschlag „Planung JJJJ-MM-TT.gpx“.
    /// </summary>
    /// <param name="datum">Das Datum, meist heute.</param>
    /// <returns>Der Dateiname mit Endung.</returns>
    public static string Ausfuehren(DateOnly datum) =>
        $"Planung {datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.gpx";
}

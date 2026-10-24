using System.Globalization;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Formatiert eine Stundenzahl als Stunden und Minuten mit Einheit, z. B. „123:45 h“.
/// </summary>
/// <remarks>
/// Für große Summen wie die Fahrzeit eines Jahres; Sekunden wären dort nur störend (anders als bei
/// <see cref="FormatiereDauer"/> für einzelne Einheiten).
/// </remarks>
public static class FormatiereStunden
{
    /// <summary>Deutsche Kultur für den Tausenderpunkt.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Liefert die Stunden im Format „h:mm h“, auf ganze Minuten gerundet; die Stunden mit Tausenderpunkt.
    /// </summary>
    /// <param name="stunden">Die Stunden (nicht negativ), z. B. 123,75.</param>
    /// <returns>Der Text, z. B. „123:45 h“ oder „1.234:30 h“.</returns>
    public static string Ausfuehren(double stunden)
    {
        long minuten = (long)Math.Round(stunden * 60, MidpointRounding.AwayFromZero);
        return string.Format(Deutsch, "{0:N0}:{1:00} h", minuten / 60, minuten % 60);
    }
}

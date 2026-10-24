using System.Globalization;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Formatiert eine Dauer als Stunden, Minuten und Sekunden, z. B. „3:45:12“.
/// </summary>
public static class FormatiereDauer
{
    /// <summary>
    /// Liefert die Dauer im Format „h:mm:ss“; die Stunden laufen auch über 24 hinaus („512:13:45“).
    /// </summary>
    /// <remarks>
    /// Das Standardformat von <see cref="TimeSpan"/> würde ab 24 Stunden Tage voranstellen
    /// („21.08:13:45“) – für Summen über viele Touren ist eine durchgehende Stundenzahl lesbarer.
    /// Bruchteile von Sekunden werden auf ganze Sekunden gerundet.
    /// </remarks>
    /// <param name="dauer">Die Dauer (nicht negativ).</param>
    /// <returns>Der Text, z. B. „0:05:07“ oder „30:05:07“.</returns>
    public static string Ausfuehren(TimeSpan dauer)
    {
        long sekunden = (long)Math.Round(dauer.TotalSeconds, MidpointRounding.AwayFromZero);
        long stunden = sekunden / 3600;
        long minuten = sekunden / 60 % 60;
        long rest = sekunden % 60;
        return string.Create(CultureInfo.InvariantCulture, $"{stunden}:{minuten:00}:{rest:00}");
    }
}

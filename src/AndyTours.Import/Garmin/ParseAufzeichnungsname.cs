using System.Globalization;

namespace AndyTours.Import.Garmin;

/// <summary>
/// Liest Datum und Startzeit aus dem Dateinamen einer Garmin-Aufzeichnung.
/// </summary>
/// <remarks>
/// Die Edge-Geräte benennen ihre Aufzeichnungen nach der Startzeit in Ortszeit, z. B. „2026-09-26-08-02-49.fit“.
/// Damit lassen sich schon vorhandene Touren überspringen, ohne die Datei zu kopieren und zu lesen – das spart
/// beim Edge 1040 viel Zeit. Geprüft wird nach dem Lesen trotzdem noch einmal mit den Werten aus der Datei.
/// </remarks>
public static class ParseAufzeichnungsname
{
    /// <summary>
    /// Liefert Datum und Startzeit aus dem Dateinamen.
    /// </summary>
    /// <param name="dateiname">Dateiname mit oder ohne Endung „.fit“.</param>
    /// <returns>Datum und Startzeit oder <c>null</c>, wenn der Name nicht diesem Muster folgt.</returns>
    public static (DateOnly Datum, TimeOnly Startzeit)? Ausfuehren(string dateiname)
    {
        string name = Path.GetFileNameWithoutExtension(dateiname);
        return System.DateTime.TryParseExact(name, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out System.DateTime zeitpunkt)
            ? (DateOnly.FromDateTime(zeitpunkt), TimeOnly.FromDateTime(zeitpunkt))
            : null;
    }
}

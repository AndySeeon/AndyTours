using System.Globalization;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt ein Datum aus den Altdaten (deutsches Format) um.
/// </summary>
public static class ParseAltesDatum
{
    /// <summary>
    /// Mögliche Schreibweisen: mit führender Null („05.03.2016", in <c>workouts.csv</c>)
    /// und ohne („13.4.2016", in den Equipment-Abschnitten der INI).
    /// Das Format „d.M.yyyy" akzeptiert beim Lesen beide Varianten.
    /// </summary>
    private const string Format = "d.M.yyyy";

    /// <summary>
    /// Liest das Datum.
    /// </summary>
    /// <param name="text">Datumstext, z. B. „05.03.2016".</param>
    /// <returns>
    /// Das Datum oder <c>null</c>, wenn der Text leer ist oder den Platzhalter „01.01.1900"
    /// enthält, mit dem das Original „kein Datum" kennzeichnet.
    /// </returns>
    /// <exception cref="FormatException">Der Text ist kein gültiges Datum.</exception>
    public static DateOnly? Ausfuehren(string text)
    {
        string bereinigt = text.Trim();
        if (bereinigt.Length == 0)
        {
            return null;
        }

        if (!DateOnly.TryParseExact(bereinigt, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly datum))
        {
            throw new FormatException($"„{text}“ ist kein gültiges Datum (erwartet TT.MM.JJJJ).");
        }

        // 01.01.1900 setzt das Original als Vorbelegung ein, wenn kein Datum eingegeben wurde.
        return datum == new DateOnly(1900, 1, 1) ? null : datum;
    }
}

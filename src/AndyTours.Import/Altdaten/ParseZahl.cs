using System.Globalization;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt einen Zahlentext aus den Altdaten in eine Kommazahl um.
/// </summary>
public static class ParseZahl
{
    /// <summary>
    /// Liest eine Zahl, bei der Punkt oder Komma als Dezimaltrennzeichen stehen kann.
    /// </summary>
    /// <remarks>
    /// <c>workouts.csv</c> verwendet den Dezimalpunkt („77.83"), in <c>maintanence.dat</c> und in
    /// handschriftlich gepflegten INI-Werten kann auch ein Komma stehen. Tausendertrennzeichen
    /// kommen in den Altdaten nicht vor.
    /// </remarks>
    /// <param name="text">Der Zahlentext, z. B. „77.83", „0,75" oder „-3".</param>
    /// <returns>Die Zahl; ein leerer Text ergibt 0.</returns>
    /// <exception cref="FormatException">Der Text ist keine Zahl.</exception>
    public static double Ausfuehren(string text)
    {
        string bereinigt = text.Trim();

        // Leere Felder bedeuten im Original „kein Wert" und werden wie 0 behandelt.
        if (bereinigt.Length == 0)
        {
            return 0;
        }

        // Komma vereinheitlichen und kulturunabhängig lesen, damit das Ergebnis nicht von den
        // Windows-Ländereinstellungen abhängt (in Deutschland wäre sonst „77.83" = 7783).
        bereinigt = bereinigt.Replace(',', '.');
        if (double.TryParse(bereinigt, NumberStyles.Float, CultureInfo.InvariantCulture, out double zahl))
        {
            return zahl;
        }

        throw new FormatException($"„{text}“ ist keine gültige Zahl.");
    }
}

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt einen Zahlentext aus den Altdaten in eine ganze Zahl um.
/// </summary>
public static class ParseGanzzahl
{
    /// <summary>
    /// Liest eine ganze Zahl; Nachkommastellen werden kaufmännisch gerundet.
    /// </summary>
    /// <remarks>
    /// Das Original schreibt manche ganzzahligen Werte mit Nachkommastelle (z. B. Gewicht „95.0"),
    /// deshalb wird zunächst über <see cref="ParseZahl"/> gelesen und dann gerundet.
    /// </remarks>
    /// <param name="text">Der Zahlentext.</param>
    /// <returns>Die gerundete Zahl; ein leerer Text ergibt 0.</returns>
    /// <exception cref="FormatException">Der Text ist keine Zahl.</exception>
    public static int Ausfuehren(string text)
    {
        // MidpointRounding.AwayFromZero: 2,5 → 3 (wie man es erwartet), nicht 2 („Banker's Rounding").
        return (int)Math.Round(ParseZahl.Ausfuehren(text), MidpointRounding.AwayFromZero);
    }
}

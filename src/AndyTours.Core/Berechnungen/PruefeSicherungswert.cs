using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft eine Eingabe für das Sicherungsintervall oder die Höchstzahl der Sicherungen.
/// </summary>
public static class PruefeSicherungswert
{
    /// <summary>
    /// Wandelt den eingegebenen Text in eine ganze Zahl von 1 bis 99 um.
    /// </summary>
    /// <remarks>
    /// Leerzeichen am Anfang und Ende werden ignoriert. Vorzeichen, Tausenderpunkte und
    /// Nachkommastellen sind nicht erlaubt; „2,5“ oder „-1“ sind also ungültig.
    /// </remarks>
    /// <param name="eingabe">Der Text aus dem Eingabefeld.</param>
    /// <returns>Die Zahl oder <c>null</c>, wenn die Eingabe keine ganze Zahl im erlaubten Bereich ist.</returns>
    public static int? Ausfuehren(string? eingabe)
    {
        // NumberStyles.None: nur Ziffern; Leerzeichen entfernen wir vorher selbst
        if (!int.TryParse((eingabe ?? string.Empty).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int wert))
        {
            return null;
        }

        return wert is >= Sicherungseinstellungen.Minimalwert and <= Sicherungseinstellungen.Maximalwert ? wert : null;
    }
}

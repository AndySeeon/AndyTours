using System.Globalization;
using System.Text;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt den Hinweis auf fällige Wartungsalarme, der beim Programmstart erscheint.
/// </summary>
public static class BeschreibeFaelligeAlarme
{
    /// <summary>
    /// Liefert den Text des Hinweises mit einer Zeile je fälligem Alarm, in der Reihenfolge der Alarmliste
    /// (die größte Überschreitung zuerst).
    /// </summary>
    /// <remarks>
    /// Beispiel:
    /// <code>
    /// Folgende Wartungen sind fällig:
    ///
    /// • Genius – Kettenwechsel: 120 km überschritten
    /// • Stevens – Bremsbeläge: Intervall genau erreicht
    /// </code>
    /// Ausgeschaltete Alarme und die ausgemusterter Räder erscheinen nie, bald fällige ebenfalls nicht.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <returns>Der Text oder <c>null</c>, wenn kein Alarm fällig ist.</returns>
    public static string? Ausfuehren(Datenbestand bestand)
    {
        List<Alarmzeile> faellig = ErstelleAlarmzeilen.Ausfuehren(bestand, null)
            .Where(z => z.Zustand == Alarmzustand.Faellig)
            .ToList();
        if (faellig.Count == 0)
        {
            return null;
        }

        CultureInfo deutsch = CultureInfo.GetCultureInfo("de-DE");
        var text = new StringBuilder(faellig.Count == 1 ? "Folgende Wartung ist fällig:" : "Folgende Wartungen sind fällig:");
        text.AppendLine().AppendLine();
        foreach (Alarmzeile zeile in faellig)
        {
            double ueber = -zeile.VerbleibendKm;
            string stand = Math.Round(ueber) >= 1
                ? string.Format(deutsch, "{0:N0} km überschritten", ueber)
                : "Intervall genau erreicht";
            text.AppendLine(deutsch, $"• {zeile.Rad} – {zeile.Taetigkeit}: {stand}");
        }

        return text.ToString().TrimEnd();
    }
}

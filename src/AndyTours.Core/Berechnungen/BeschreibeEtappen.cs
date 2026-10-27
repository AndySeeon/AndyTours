using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Beschreibt Etappen einer Mehrtagestour mit Datum und Name, für die Hinweise im Tourdetail.
/// </summary>
public static class BeschreibeEtappen
{
    /// <summary>Deutsche Kultur für Wochentag und Datum.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Liefert Datum und Name der Einheiten, durch Strichpunkte getrennt.
    /// </summary>
    /// <remarks>
    /// Beispiel: „Mi, 16.07.2025 Brenner; Do, 17.07.2025“. Ein leerer Name entfällt. Getrennt wird mit Strichpunkt,
    /// weil das Datum selbst schon ein Komma enthält. Mit einer Einheit dient das Ergebnis auch für den Hinweis beim
    /// Überfahren des Profils.
    /// </remarks>
    /// <param name="einheiten">Die Einheiten in der gewünschten Reihenfolge.</param>
    /// <returns>Die Beschreibung; leer ohne Einheiten.</returns>
    public static string Ausfuehren(IEnumerable<Trainingseinheit> einheiten) =>
        string.Join("; ", einheiten.Select(Beschreibe));

    /// <summary>Datum und Name einer Einheit.</summary>
    private static string Beschreibe(Trainingseinheit einheit)
    {
        string datum = einheit.Datum.ToString("ddd, dd.MM.yyyy", Deutsch);
        return string.IsNullOrWhiteSpace(einheit.Name) ? datum : $"{datum} {einheit.Name.Trim()}";
    }
}

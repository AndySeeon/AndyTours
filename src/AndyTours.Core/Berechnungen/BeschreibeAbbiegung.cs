using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Beschreibt einen Abbiegepunkt in Worten, z. B. „Rechts“, „Leicht links“ oder „Kreisverkehr 2. Ausfahrt“.
/// </summary>
/// <remarks>
/// Der Text erscheint als Hinweis an der Markierung auf der Karte und als Name des Abbiegepunkts in der GPX-Datei,
/// den der Garmin beim Nachfahren anzeigt.
/// </remarks>
public static class BeschreibeAbbiegung
{
    /// <summary>
    /// Liefert den Text eines Abbiegepunkts.
    /// </summary>
    /// <param name="abbiegepunkt">Der Abbiegepunkt.</param>
    /// <returns>Der Text; beim Kreisverkehr mit Ausfahrt, falls bekannt.</returns>
    public static string Ausfuehren(Abbiegepunkt abbiegepunkt) => abbiegepunkt.Richtung switch
    {
        Abbiegerichtung.Links => "Links",
        Abbiegerichtung.Rechts => "Rechts",
        Abbiegerichtung.LeichtLinks => "Leicht links",
        Abbiegerichtung.LeichtRechts => "Leicht rechts",
        Abbiegerichtung.ScharfLinks => "Scharf links",
        Abbiegerichtung.ScharfRechts => "Scharf rechts",
        Abbiegerichtung.Kreisverkehr when abbiegepunkt.Ausfahrt is int ausfahrt and > 0 => $"Kreisverkehr {ausfahrt}. Ausfahrt",
        Abbiegerichtung.Kreisverkehr => "Kreisverkehr",
        Abbiegerichtung.Wenden => "Wenden",
        _ => abbiegepunkt.Richtung.ToString(),
    };
}

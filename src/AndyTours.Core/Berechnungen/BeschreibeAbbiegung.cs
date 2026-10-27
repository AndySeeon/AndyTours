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

    /// <summary>
    /// Liefert die kurze Form für den Garmin Edge, der den Text einer Abbiegung beim Ankündigen anzeigt.
    /// </summary>
    /// <remarks>
    /// Die Texte haben höchstens <see cref="KurzeLaenge"/> Zeichen, damit der Edge sie nicht abschneidet. Sie gleichen
    /// den langen Texten, nur der Kreisverkehr heißt „Kreisel“ und die Ausfahrt „Ausf.“, z. B. „Kreisel 2. Ausf.“.
    /// </remarks>
    /// <param name="richtung">Richtung der Abbiegung.</param>
    /// <param name="ausfahrt">Nummer der Ausfahrt bei einem Kreisverkehr, sonst <c>null</c>.</param>
    /// <returns>Der kurze Text.</returns>
    public static string Kurz(Abbiegerichtung richtung, int? ausfahrt) => richtung switch
    {
        Abbiegerichtung.Kreisverkehr when ausfahrt is int nummer and > 0 and < 10 => $"Kreisel {nummer}. Ausf.",
        Abbiegerichtung.Kreisverkehr => "Kreisel",
        _ => Ausfuehren(new Abbiegepunkt(new Geoposition(0, 0), richtung)),
    };

    /// <summary>Höchste Länge der kurzen Texte für den Edge.</summary>
    public const int KurzeLaenge = 16;
}

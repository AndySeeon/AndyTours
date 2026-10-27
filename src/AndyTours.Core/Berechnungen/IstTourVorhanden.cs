using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft beim Import, ob eine Tour schon als Einheit in AndyTours steht.
/// </summary>
public static class IstTourVorhanden
{
    /// <summary>
    /// Liefert <c>true</c>, wenn es eine Einheit mit gleichem Datum und gleicher Startzeit gibt.
    /// </summary>
    /// <remarks>
    /// Die Startzeit wird auf die Minute verglichen, weil die aus BikeXperience übernommenen Einheiten nur
    /// Stunde und Minute haben. Einheiten ohne Startzeit zählen nicht.
    /// </remarks>
    /// <param name="einheiten">Die vorhandenen Einheiten.</param>
    /// <param name="datum">Datum der Tour.</param>
    /// <param name="startzeit">Startzeit der Tour.</param>
    /// <returns><c>true</c>, wenn die Tour schon vorhanden ist.</returns>
    public static bool Ausfuehren(IEnumerable<Trainingseinheit> einheiten, DateOnly datum, TimeOnly startzeit) =>
        einheiten.Any(e => e.Datum == datum && e.Startzeit is TimeOnly zeit && zeit.Hour == startzeit.Hour && zeit.Minute == startzeit.Minute);
}

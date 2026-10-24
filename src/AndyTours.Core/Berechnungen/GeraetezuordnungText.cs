using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt den Text im Feld <see cref="Equipment.Geraetezuordnung"/> in die Liste der gemerkten
/// <see cref="Radkennung"/>en um und zurück.
/// </summary>
/// <remarks>
/// Beide Richtungen stehen zusammen, weil sie dasselbe Format teilen: Die Kennungen stehen durch Strichpunkte
/// getrennt hintereinander, jede mit ihrer Art davor, z. B. <c>Rad:7af22db6-…; Sensor:3452708077</c>.
/// Ein Eintrag ohne bekannte Art stammt aus BikeXperience (z. B. „Bike1“) und bleibt als
/// <see cref="Radkennungsart.Altwert"/> erhalten, damit er sichtbar ist und entfernt werden kann.
/// </remarks>
public static class GeraetezuordnungText
{
    /// <summary>Vorsilbe einer Rad-Kennung.</summary>
    private const string RadVorsilbe = "Rad:";

    /// <summary>Vorsilbe einer Sensornummer.</summary>
    private const string SensorVorsilbe = "Sensor:";

    /// <summary>
    /// Liest die Kennungen aus dem Text.
    /// </summary>
    /// <remarks>
    /// Leerzeichen am Rand und leere Einträge werden übergangen, doppelte nur einmal geliefert. Die Vorsilben
    /// werden ohne Unterschied zwischen Groß- und Kleinschreibung erkannt.
    /// </remarks>
    /// <param name="text">Inhalt des Felds; leer = keine Kennungen.</param>
    /// <returns>Die Kennungen in der Reihenfolge des Texts.</returns>
    public static List<Radkennung> Lesen(string text)
    {
        List<Radkennung> kennungen = [];
        foreach (string teil in text.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            Radkennung kennung =
                teil.StartsWith(RadVorsilbe, StringComparison.OrdinalIgnoreCase) ? new(Radkennungsart.Rad, teil[RadVorsilbe.Length..].Trim())
                : teil.StartsWith(SensorVorsilbe, StringComparison.OrdinalIgnoreCase) ? new(Radkennungsart.Sensor, teil[SensorVorsilbe.Length..].Trim())
                : new(Radkennungsart.Altwert, teil);

            if (kennung.Wert.Length > 0 && !kennungen.Contains(kennung))
            {
                kennungen.Add(kennung);
            }
        }

        return kennungen;
    }

    /// <summary>
    /// Schreibt die Kennungen als Text für das Feld.
    /// </summary>
    /// <param name="kennungen">Die Kennungen; doppelte werden nur einmal geschrieben.</param>
    /// <returns>Der Text, z. B. „Rad:7af22db6-…; Sensor:3452708077“; leer, wenn es keine gibt.</returns>
    public static string Schreiben(IEnumerable<Radkennung> kennungen) =>
        string.Join("; ", kennungen.Distinct().Select(k => k.Art switch
        {
            Radkennungsart.Rad => RadVorsilbe + k.Wert,
            Radkennungsart.Sensor => SensorVorsilbe + k.Wert,
            _ => k.Wert,
        }));
}

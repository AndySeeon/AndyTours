using System.Text.Json;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt die GeoJSON-Antwort von openrouteservice in ein <see cref="Teilstueck"/> um.
/// </summary>
/// <remarks>
/// Aufbau der Antwort (Auszug):
/// <code>
/// { "features": [ { "geometry": { "coordinates": [ [länge, breite, höhe], … ] },
///                   "properties": { "segments": [ { "steps": [ { "type": 1, "way_points": [3, 9], "exit_number": 2 }, … ] } ] } } ] }
/// </code>
/// Anweisungsarten laut openrouteservice: 0 links, 1 rechts, 2 scharf links, 3 scharf rechts, 4 leicht links,
/// 5 leicht rechts, 6 geradeaus, 7 in den Kreisverkehr, 8 aus dem Kreisverkehr, 9 wenden, 10 Ziel, 11 Start,
/// 12 links halten, 13 rechts halten. Geradeaus, Start, Ziel und das Verlassen des Kreisverkehrs ergeben keinen
/// Abbiegepunkt. Der Abbiegepunkt liegt am ersten Punkt der Anweisung (<c>way_points[0]</c>).
/// </remarks>
public static class LeseOpenrouteserviceAntwort
{
    /// <summary>
    /// Liefert das Teilstück mit Punkten, Höhen und Abbiegepunkten.
    /// </summary>
    /// <param name="json">Text der Antwort.</param>
    /// <returns>Das Teilstück oder <c>null</c>, wenn die Antwort nicht lesbar ist oder weniger als zwei Punkte hat.</returns>
    public static Teilstueck? Ausfuehren(string json)
    {
        try
        {
            using JsonDocument dokument = JsonDocument.Parse(json);
            JsonElement merkmal = dokument.RootElement.GetProperty("features")[0];
            List<Verlaufspunkt> punkte = [.. merkmal.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().Select(Punkt)];
            if (punkte.Count < 2)
            {
                return null;
            }

            List<Abbiegepunkt> abbiegepunkte = [];
            if (merkmal.TryGetProperty("properties", out JsonElement eigenschaften)
                && eigenschaften.TryGetProperty("segments", out JsonElement abschnitte))
            {
                foreach (JsonElement anweisung in abschnitte.EnumerateArray()
                    .Where(a => a.TryGetProperty("steps", out _))
                    .SelectMany(a => a.GetProperty("steps").EnumerateArray()))
                {
                    int nummer = anweisung.GetProperty("way_points")[0].GetInt32();
                    if (nummer >= 0 && nummer < punkte.Count && Richtung(anweisung.GetProperty("type").GetInt32()) is Abbiegerichtung richtung)
                    {
                        int? ausfahrt = richtung == Abbiegerichtung.Kreisverkehr
                            && anweisung.TryGetProperty("exit_number", out JsonElement nr) && nr.GetInt32() > 0
                            ? nr.GetInt32()
                            : null;
                        abbiegepunkte.Add(new Abbiegepunkt(punkte[nummer].Position!, richtung, ausfahrt));
                    }
                }
            }

            return new Teilstueck(punkte, abbiegepunkte, IstLuftlinie: false);
        }
        catch (Exception fehler) when (fehler is JsonException or KeyNotFoundException or InvalidOperationException
            or IndexOutOfRangeException or FormatException)
        {
            return null;
        }
    }

    /// <summary>Ein Punkt aus <c>[länge, breite, höhe]</c>; die Höhe kann fehlen.</summary>
    private static Verlaufspunkt Punkt(JsonElement koordinate) => new()
    {
        Position = new Geoposition(koordinate[1].GetDouble(), koordinate[0].GetDouble()),
        Hoehe = koordinate.GetArrayLength() > 2 ? koordinate[2].GetDouble() : null,
    };

    /// <summary>Richtung zu einer Anweisungsart oder <c>null</c>, wenn sie keinen Abbiegepunkt ergibt.</summary>
    private static Abbiegerichtung? Richtung(int art) => art switch
    {
        0 => Abbiegerichtung.Links,
        1 => Abbiegerichtung.Rechts,
        2 => Abbiegerichtung.ScharfLinks,
        3 => Abbiegerichtung.ScharfRechts,
        4 or 12 => Abbiegerichtung.LeichtLinks,
        5 or 13 => Abbiegerichtung.LeichtRechts,
        7 => Abbiegerichtung.Kreisverkehr,
        9 => Abbiegerichtung.Wenden,
        _ => null,
    };
}

using System.Text.Json;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt die GeoJSON-Antwort eines BRouter-Servers in ein <see cref="Teilstueck"/> um.
/// </summary>
/// <remarks>
/// Aufbau der Antwort (Auszug):
/// <code>
/// { "features": [ { "geometry": { "coordinates": [ [länge, breite, höhe], … ] },
///                   "properties": { "voicehints": [ [punktnummer, befehl, ausfahrt, abstand, winkel], … ] } } ] }
/// </code>
/// Die Befehle der Abbiegehinweise stammen aus BRouters <c>VoiceHint</c>: 1 geradeaus, 2 links, 3 leicht links,
/// 4 scharf links, 5 rechts, 6 leicht rechts, 7 scharf rechts, 8 links halten, 9 rechts halten, 10/11/15 wenden,
/// 12 abseits der Route, 13/14 Kreisverkehr, 16 Luftlinie, 17 links ausfahren, 18 rechts ausfahren, 100 Ziel.
/// „Geradeaus“, „abseits der Route“, „Luftlinie“ und „Ziel“ ergeben keinen Abbiegepunkt.
/// </remarks>
public static class LeseBRouterAntwort
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
                && eigenschaften.TryGetProperty("voicehints", out JsonElement hinweise))
            {
                foreach (JsonElement hinweis in hinweise.EnumerateArray())
                {
                    int nummer = hinweis[0].GetInt32();
                    int befehl = hinweis[1].GetInt32();
                    int ausfahrt = Math.Abs(hinweis[2].GetInt32());
                    if (nummer >= 0 && nummer < punkte.Count && Richtung(befehl) is Abbiegerichtung richtung)
                    {
                        int? kreisverkehrAusfahrt = richtung == Abbiegerichtung.Kreisverkehr && ausfahrt > 0 ? ausfahrt : null;
                        abbiegepunkte.Add(new Abbiegepunkt(punkte[nummer].Position!, richtung, kreisverkehrAusfahrt));
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

    /// <summary>Richtung zu einem BRouter-Befehl oder <c>null</c>, wenn er keinen Abbiegepunkt ergibt.</summary>
    private static Abbiegerichtung? Richtung(int befehl) => befehl switch
    {
        2 => Abbiegerichtung.Links,
        3 or 8 or 17 => Abbiegerichtung.LeichtLinks,
        4 => Abbiegerichtung.ScharfLinks,
        5 => Abbiegerichtung.Rechts,
        6 or 9 or 18 => Abbiegerichtung.LeichtRechts,
        7 => Abbiegerichtung.ScharfRechts,
        10 or 11 or 15 => Abbiegerichtung.Wenden,
        13 or 14 => Abbiegerichtung.Kreisverkehr,
        _ => null,
    };
}

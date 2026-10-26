using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="LeseOpenrouteserviceAntwort"/> mit kurzen, selbst geschriebenen Antworten.
/// </summary>
public class LeseOpenrouteserviceAntwortTests
{
    /// <summary>Antwort mit fünf Punkten: Start, links, geradeaus, Kreisverkehr 3., Kreisverkehr verlassen, Ziel.</summary>
    private const string Antwort = """
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "properties": {
                "segments": [
                  {
                    "distance": 420.0,
                    "steps": [
                      { "type": 11, "instruction": "Start", "way_points": [0, 1] },
                      { "type": 0, "instruction": "Links", "way_points": [1, 2] },
                      { "type": 6, "instruction": "Geradeaus", "way_points": [2, 3] },
                      { "type": 7, "instruction": "Kreisverkehr", "exit_number": 3, "way_points": [3, 4] },
                      { "type": 8, "instruction": "Ausfahrt", "way_points": [4, 4] },
                      { "type": 10, "instruction": "Ziel", "way_points": [4, 4] }
                    ]
                  }
                ]
              },
              "geometry": {
                "type": "LineString",
                "coordinates": [
                  [11.5755, 48.1374, 516.0],
                  [11.5760, 48.1380, 517.5],
                  [11.5770, 48.1385, 519.0],
                  [11.5780, 48.1390, 520.0],
                  [11.5790, 48.1395, 521.0]
                ]
              }
            }
          ]
        }
        """;

    [Fact]
    public void Punkte_mit_Position_und_Hoehe()
    {
        Teilstueck teilstueck = LeseOpenrouteserviceAntwort.Ausfuehren(Antwort)!;

        Assert.False(teilstueck.IstLuftlinie);
        Assert.Equal(5, teilstueck.Punkte.Count);
        Assert.Equal(new Geoposition(48.1395, 11.5790), teilstueck.Punkte[4].Position);
        Assert.Equal(519.0, teilstueck.Punkte[2].Hoehe);
    }

    [Fact]
    public void Abbiegepunkte_ohne_Start_Ziel_geradeaus_und_Ausfahrt()
    {
        Teilstueck teilstueck = LeseOpenrouteserviceAntwort.Ausfuehren(Antwort)!;

        Assert.Equal(
            [
                new Abbiegepunkt(new Geoposition(48.1380, 11.5760), Abbiegerichtung.Links),
                new Abbiegepunkt(new Geoposition(48.1390, 11.5780), Abbiegerichtung.Kreisverkehr, 3),
            ],
            teilstueck.Abbiegepunkte);
    }

    [Theory]
    [InlineData(1, Abbiegerichtung.Rechts)]
    [InlineData(2, Abbiegerichtung.ScharfLinks)]
    [InlineData(3, Abbiegerichtung.ScharfRechts)]
    [InlineData(4, Abbiegerichtung.LeichtLinks)]
    [InlineData(5, Abbiegerichtung.LeichtRechts)]
    [InlineData(9, Abbiegerichtung.Wenden)]
    [InlineData(12, Abbiegerichtung.LeichtLinks)]
    [InlineData(13, Abbiegerichtung.LeichtRechts)]
    public void Anweisungsarten_werden_zu_Richtungen(int art, Abbiegerichtung richtung)
    {
        string json = $$"""
            { "features": [ { "properties": { "segments": [ { "steps": [ { "type": {{art}}, "way_points": [1, 1] } ] } ] },
              "geometry": { "coordinates": [ [11.0, 48.0, 500], [11.1, 48.1, 510] ] } } ] }
            """;

        Assert.Equal([new Abbiegepunkt(new Geoposition(48.1, 11.1), richtung)], LeseOpenrouteserviceAntwort.Ausfuehren(json)!.Abbiegepunkte);
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein JSON")]
    [InlineData("""{ "error": { "code": 2010, "message": "Could not find routable point" } }""")]
    [InlineData("""{ "features": [ { "geometry": { "coordinates": [ [11.0, 48.0, 500] ] } } ] }""")]
    public void Unbrauchbare_Antwort_ergibt_kein_Teilstueck(string json)
    {
        Assert.Null(LeseOpenrouteserviceAntwort.Ausfuehren(json));
    }
}

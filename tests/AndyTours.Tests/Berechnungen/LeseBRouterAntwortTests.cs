using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="LeseBRouterAntwort"/> mit kurzen, selbst geschriebenen Antworten.
/// </summary>
public class LeseBRouterAntwortTests
{
    /// <summary>Antwort mit fünf Punkten und den Hinweisen „rechts“, „geradeaus“, „links halten“, „Kreisverkehr 2.“.</summary>
    private const string Antwort = """
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "properties": {
                "track-length": "420",
                "voicehints": [ [1, 5, 0, 65.0, 89], [2, 1, 0, 30.0, 2], [3, 8, 0, 20.0, -20], [4, 13, 2, 0.0, 45] ]
              },
              "geometry": {
                "type": "LineString",
                "coordinates": [
                  [11.5755, 48.1374, 516.0],
                  [11.5760, 48.1380, 517.5],
                  [11.5770, 48.1385, 519.0],
                  [11.5780, 48.1390, 520.0],
                  [11.5790, 48.1395]
                ]
              }
            }
          ]
        }
        """;

    [Fact]
    public void Punkte_mit_Position_und_Hoehe()
    {
        Teilstueck teilstueck = LeseBRouterAntwort.Ausfuehren(Antwort)!;

        Assert.False(teilstueck.IstLuftlinie);
        Assert.Equal(5, teilstueck.Punkte.Count);
        Assert.Equal(new Geoposition(48.1374, 11.5755), teilstueck.Punkte[0].Position);
        Assert.Equal(517.5, teilstueck.Punkte[1].Hoehe);
        Assert.Null(teilstueck.Punkte[4].Hoehe);
    }

    [Fact]
    public void Abbiegepunkte_ohne_geradeaus()
    {
        Teilstueck teilstueck = LeseBRouterAntwort.Ausfuehren(Antwort)!;

        Assert.Equal(
            [
                new Abbiegepunkt(new Geoposition(48.1380, 11.5760), Abbiegerichtung.Rechts),
                new Abbiegepunkt(new Geoposition(48.1390, 11.5780), Abbiegerichtung.LeichtLinks),
                new Abbiegepunkt(new Geoposition(48.1395, 11.5790), Abbiegerichtung.Kreisverkehr, 2),
            ],
            teilstueck.Abbiegepunkte);
    }

    [Fact]
    public void Ohne_Hinweise_keine_Abbiegepunkte()
    {
        const string ohneHinweise = """
            { "features": [ { "properties": {}, "geometry": { "coordinates": [ [11.0, 48.0, 500], [11.1, 48.1, 510] ] } } ] }
            """;

        Assert.Empty(LeseBRouterAntwort.Ausfuehren(ohneHinweise)!.Abbiegepunkte);
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein JSON")]
    [InlineData("""{ "features": [] }""")]
    [InlineData("""{ "features": [ { "geometry": { "coordinates": [ [11.0, 48.0] ] } } ] }""")]
    [InlineData("""{ "fehler": "target island detected" }""")]
    public void Unbrauchbare_Antwort_ergibt_kein_Teilstueck(string json)
    {
        Assert.Null(LeseBRouterAntwort.Ausfuehren(json));
    }
}

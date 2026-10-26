using System.Xml.Linq;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PlanungsGpx"/>.
/// </summary>
public class PlanungsGpxTests
{
    private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";

    private static readonly Geoposition A = new(48.1374, 11.5755);
    private static readonly Geoposition B = new(48.15, 11.62);
    private static readonly Geoposition C = new(48.16, 11.65);

    /// <summary>Planung A–B–C: erstes Teilstück ermittelt mit zwei Abbiegepunkten, zweites als Luftlinie.</summary>
    private static Tourenplanung Planung() => new(
        Fahrprofil.Mountainbike,
        [A, B, C],
        [
            new Teilstueck(
                [
                    new Verlaufspunkt { Position = A, Hoehe = 516 },
                    new Verlaufspunkt { Position = new Geoposition(48.14, 11.60), Hoehe = 520.5 },
                    new Verlaufspunkt { Position = B, Hoehe = 518 },
                ],
                [
                    new Abbiegepunkt(new Geoposition(48.14, 11.60), Abbiegerichtung.Rechts),
                    new Abbiegepunkt(B, Abbiegerichtung.Kreisverkehr, 2),
                ],
                false),
            ErstelleLuftlinie.Ausfuehren(B, C),
        ]);

    [Fact]
    public void Datei_ist_GPX_1_1_mit_einem_Segment_je_Teilstueck()
    {
        XElement wurzel = XDocument.Parse(PlanungsGpx.Schreiben(Planung(), "Planung 2026-10-26")).Root!;

        Assert.Equal(Gpx + "gpx", wurzel.Name);
        Assert.Equal("1.1", (string?)wurzel.Attribute("version"));
        Assert.Equal("Planung 2026-10-26", (string?)wurzel.Element(Gpx + "trk")!.Element(Gpx + "name"));
        List<XElement> segmente = [.. wurzel.Element(Gpx + "trk")!.Elements(Gpx + "trkseg")];
        Assert.Equal([3, 2], segmente.Select(s => s.Elements(Gpx + "trkpt").Count()));
        Assert.Equal("520.5", (string?)segmente[0].Elements(Gpx + "trkpt").ElementAt(1).Element(Gpx + "ele"));
        Assert.Equal("48.1374", (string?)segmente[0].Element(Gpx + "trkpt")!.Attribute("lat"));
    }

    [Fact]
    public void Abbiegepunkte_als_Wegpunkte_mit_Richtung()
    {
        XElement wurzel = XDocument.Parse(PlanungsGpx.Schreiben(Planung(), "Test")).Root!;

        List<XElement> wpts = [.. wurzel.Elements(Gpx + "wpt")];
        Assert.Equal(["Rechts", "Kreisverkehr 2. Ausfahrt"], wpts.Select(w => (string?)w.Element(Gpx + "name")));
        Assert.Equal(["Rechts", "Kreisverkehr"], wpts.Select(w => (string?)w.Element(Gpx + "type")));
        Assert.Equal("11.6", (string?)wpts[0].Attribute("lon"));
    }

    [Fact]
    public void Wpt_stehen_vor_dem_Track()
    {
        XElement wurzel = XDocument.Parse(PlanungsGpx.Schreiben(Planung(), "Test")).Root!;

        Assert.Equal(["metadata", "wpt", "wpt", "trk"], wurzel.Elements().Select(e => e.Name.LocalName));
    }

    [Fact]
    public void Hin_und_zurueck_bleibt_die_Planung_erhalten()
    {
        Tourenplanung original = Planung();

        Tourenplanung gelesen = PlanungsGpx.Lesen(PlanungsGpx.Schreiben(original, "Test"))!;

        Assert.Equal(Fahrprofil.Mountainbike, gelesen.Profil);
        Assert.Equal(original.Wegpunkte, gelesen.Wegpunkte);
        Assert.Equal(2, gelesen.Teilstuecke.Count);
        for (int i = 0; i < 2; i++)
        {
            Assert.Equal(original.Teilstuecke[i]!.Punkte, gelesen.Teilstuecke[i]!.Punkte);
            Assert.Equal(original.Teilstuecke[i]!.Abbiegepunkte, gelesen.Teilstuecke[i]!.Abbiegepunkte);
            Assert.Equal(original.Teilstuecke[i]!.IstLuftlinie, gelesen.Teilstuecke[i]!.IstLuftlinie);
        }
    }

    [Fact]
    public void Offenes_Teilstueck_wird_als_Luftlinie_gespeichert()
    {
        var planung = new Tourenplanung(Fahrprofil.Rennrad, [A, B], [null]);

        Tourenplanung gelesen = PlanungsGpx.Lesen(PlanungsGpx.Schreiben(planung, "Test"))!;

        Assert.True(gelesen.Teilstuecke[0]!.IstLuftlinie);
        Assert.Equal([A, B], gelesen.Teilstuecke[0]!.Punkte.Select(p => p.Position));
    }

    [Fact]
    public void Weniger_als_zwei_Wegpunkte_lassen_sich_nicht_schreiben()
    {
        var planung = new Tourenplanung(Fahrprofil.Rennrad, [A], []);

        Assert.Throws<ArgumentException>(() => PlanungsGpx.Schreiben(planung, "Test"));
    }

    [Fact]
    public void GPX_Export_einer_Tour_ist_keine_Planung()
    {
        Trainingseinheit einheit = new() { Name = "Tour" };
        string export = ErstelleGpx.Ausfuehren([new Etappe(einheit, [new Verlaufspunkt { Position = A }, new Verlaufspunkt { Position = B }])])!;

        Assert.Null(PlanungsGpx.Lesen(export));
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein XML")]
    [InlineData("<gpx")]
    public void Unlesbare_Datei_ist_keine_Planung(string text)
    {
        Assert.Null(PlanungsGpx.Lesen(text));
    }

    [Fact]
    public void Fehlendes_Segment_ist_keine_Planung()
    {
        XDocument dokument = XDocument.Parse(PlanungsGpx.Schreiben(Planung(), "Test"));
        dokument.Root!.Element(Gpx + "trk")!.Elements(Gpx + "trkseg").Last().Remove();

        Assert.Null(PlanungsGpx.Lesen(dokument.ToString()));
    }
}

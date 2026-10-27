using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="LeseGpxStrecke"/>.
/// </summary>
public class LeseGpxStreckeTests
{
    private static readonly Geoposition A = new(48.1374, 11.5755);
    private static readonly Geoposition B = new(48.15, 11.62);
    private static readonly Geoposition C = new(48.16, 11.65);

    /// <summary>Eine GPX-1.1-Datei mit dem angegebenen Inhalt unter <c>gpx</c>.</summary>
    private static string Gpx(string inhalt) =>
        $"""<?xml version="1.0" encoding="UTF-8"?><gpx version="1.1" creator="Test" xmlns="http://www.topografix.com/GPX/1/1">{inhalt}</gpx>""";

    [Fact]
    public void Jedes_Tracksegment_wird_ein_Abschnitt()
    {
        string text = Gpx("""
            <trk><trkseg><trkpt lat="48.1374" lon="11.5755"><ele>516</ele></trkpt><trkpt lat="48.15" lon="11.62" /></trkseg>
            <trkseg><trkpt lat="48.16" lon="11.65" /></trkseg></trk>
            """);

        IReadOnlyList<IReadOnlyList<Geoposition>> abschnitte = LeseGpxStrecke.Ausfuehren(text)!;

        Assert.Equal(2, abschnitte.Count);
        Assert.Equal([A, B], abschnitte[0]);
        Assert.Equal([C], abschnitte[1]);
    }

    [Fact]
    public void Mehrere_Tracks_werden_alle_gelesen()
    {
        string text = Gpx("""
            <trk><trkseg><trkpt lat="48.1374" lon="11.5755" /></trkseg></trk>
            <trk><trkseg><trkpt lat="48.15" lon="11.62" /></trkseg></trk>
            """);

        Assert.Equal(2, LeseGpxStrecke.Ausfuehren(text)!.Count);
    }

    [Fact]
    public void Ohne_Track_werden_die_Routen_gelesen()
    {
        string text = Gpx("""<rte><rtept lat="48.1374" lon="11.5755" /><rtept lat="48.15" lon="11.62" /></rte>""");

        IReadOnlyList<IReadOnlyList<Geoposition>> abschnitte = LeseGpxStrecke.Ausfuehren(text)!;

        Assert.Equal([A, B], Assert.Single(abschnitte));
    }

    [Fact]
    public void Mit_Track_bleiben_Routen_und_Wegpunkte_unberuecksichtigt()
    {
        string text = Gpx("""
            <wpt lat="1" lon="1" /><rte><rtept lat="2" lon="2" /></rte>
            <trk><trkseg><trkpt lat="48.1374" lon="11.5755" /></trkseg></trk>
            """);

        Assert.Equal([A], Assert.Single(LeseGpxStrecke.Ausfuehren(text)!));
    }

    [Fact]
    public void GPX_1_0_ohne_Namensraum_der_Version_1_1_wird_gelesen()
    {
        string text = """
            <gpx version="1.0" xmlns="http://www.topografix.com/GPX/1/0"><trk><trkseg><trkpt lat="48.1374" lon="11.5755" /></trkseg></trk></gpx>
            """;

        Assert.Equal([A], Assert.Single(LeseGpxStrecke.Ausfuehren(text)!));
    }

    [Fact]
    public void Gespeicherte_Planung_ergibt_einen_Abschnitt_je_Teilstueck()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C], [null, null]);

        IReadOnlyList<IReadOnlyList<Geoposition>> abschnitte = LeseGpxStrecke.Ausfuehren(PlanungsGpx.Schreiben(planung, "Planung"))!;

        Assert.Equal(2, abschnitte.Count);
        Assert.Equal([A, B], abschnitte[0]);
        Assert.Equal([B, C], abschnitte[1]);
    }

    [Fact]
    public void Ungueltige_Punkte_und_leere_Segmente_werden_ausgelassen()
    {
        string text = Gpx("""
            <trk><trkseg /><trkseg><trkpt lat="abc" lon="11" /><trkpt lat="91" lon="11" /><trkpt lat="48" lon="181" />
            <trkpt lon="11" /><trkpt lat="48.1374" lon="11.5755" /></trkseg></trk>
            """);

        Assert.Equal([A], Assert.Single(LeseGpxStrecke.Ausfuehren(text)!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein XML")]
    [InlineData("<kml><Document /></kml>")]
    public void Kein_lesbares_GPX_ergibt_null(string text)
    {
        Assert.Null(LeseGpxStrecke.Ausfuehren(text));
    }

    [Fact]
    public void Nur_Wegpunkte_ergibt_null()
    {
        Assert.Null(LeseGpxStrecke.Ausfuehren(Gpx("""<wpt lat="48.1374" lon="11.5755" />""")));
    }

    [Fact]
    public void Nur_ungueltige_Punkte_ergibt_null()
    {
        Assert.Null(LeseGpxStrecke.Ausfuehren(Gpx("""<trk><trkseg><trkpt lat="x" lon="y" /></trkseg></trk>""")));
    }
}

using System.Globalization;
using System.Xml.Linq;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleGpx"/>.
/// </summary>
public class ErstelleGpxTests
{
    private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";

    /// <summary>Messpunkt mit Position und Höhe; ohne Breite fehlt die Position.</summary>
    private static Verlaufspunkt Punkt(double? breite, double laenge = 12.5, double? hoehe = 600) =>
        new() { Position = breite is double b ? new Geoposition(b, laenge) : null, Hoehe = hoehe };

    private static Etappe Etappe(string name, params Verlaufspunkt[] punkte) =>
        new(new Trainingseinheit { Name = name }, punkte);

    private static XElement Lese(string? text) => XDocument.Parse(text!).Root!;

    [Fact]
    public void Track_mit_Namen_Position_und_Hoehe_ohne_Uhrzeit()
    {
        XElement gpx = Lese(ErstelleGpx.Ausfuehren([Etappe("Bründlingalm", Punkt(47.7512345, 12.6, 612.34), Punkt(47.76, 12.61, null))]));

        Assert.Equal(Gpx + "gpx", gpx.Name);
        Assert.Equal("1.1", (string?)gpx.Attribute("version"));
        XElement track = Assert.Single(gpx.Elements(Gpx + "trk"));
        Assert.Equal("Bründlingalm", (string?)track.Element(Gpx + "name"));

        List<XElement> punkte = track.Descendants(Gpx + "trkpt").ToList();
        Assert.Equal(2, punkte.Count);
        Assert.Equal("47.7512345", (string?)punkte[0].Attribute("lat"));
        Assert.Equal("12.6", (string?)punkte[0].Attribute("lon"));
        Assert.Equal("612.3", (string?)punkte[0].Element(Gpx + "ele"));
        Assert.Null(punkte[1].Element(Gpx + "ele"));
        Assert.Empty(gpx.Descendants(Gpx + "time"));
    }

    [Fact]
    public void Dezimalpunkt_auch_bei_deutscher_Spracheinstellung()
    {
        CultureInfo vorher = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            string text = ErstelleGpx.Ausfuehren([Etappe("Tour", Punkt(47.5, 12.25, 600.5))])!;

            Assert.Contains("lat=\"47.5\"", text);
            Assert.Contains("<ele>600.5</ele>", text);
        }
        finally
        {
            CultureInfo.CurrentCulture = vorher;
        }
    }

    [Fact]
    public void Punkte_entsprechen_der_Strecke_der_Karte_Punkte_ohne_Position_fehlen()
    {
        Verlaufspunkt[] verlauf = [Punkt(47.1), Punkt(null), Punkt(47.2), Punkt(null), Punkt(47.3)];

        XElement gpx = Lese(ErstelleGpx.Ausfuehren([Etappe("Tunnel", verlauf)]));

        List<Geoposition> imGpx = gpx.Descendants(Gpx + "trkpt")
            .Select(p => new Geoposition((double)p.Attribute("lat")!, (double)p.Attribute("lon")!))
            .ToList();
        Assert.Equal(ErmittleStrecke.Ausfuehren(verlauf)!.Punkte, imGpx);
    }

    [Fact]
    public void Mehrtagestour_ein_Track_je_Etappe_in_Reihenfolge_Etappen_ohne_GPS_fehlen()
    {
        Etappe[] etappen =
        [
            Etappe("Tag 1", Punkt(47.1)),
            Etappe("Rolle", Punkt(null)),
            new(new Trainingseinheit { Name = "Ohne Aufzeichnung" }, null),
            Etappe("Tag 3", Punkt(47.3), Punkt(47.4)),
        ];

        XElement gpx = Lese(ErstelleGpx.Ausfuehren(etappen));

        Assert.Equal(["Tag 1", "Tag 3"], gpx.Elements(Gpx + "trk").Select(t => (string?)t.Element(Gpx + "name")));
    }

    [Fact]
    public void Ohne_jede_Position_gibt_es_keine_Datei()
    {
        Assert.Null(ErstelleGpx.Ausfuehren([Etappe("Rolle", Punkt(null), Punkt(null))]));
        Assert.Null(ErstelleGpx.Ausfuehren([]));
    }
}

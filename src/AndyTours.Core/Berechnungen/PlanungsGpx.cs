using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Schreibt eine <see cref="Tourenplanung"/> als GPX-Datei und liest sie wieder („Speichern…“ und „Öffnen…“ im Fenster
/// „Tour planen“).
/// </summary>
/// <remarks>
/// Aufbau der Datei (GPX 1.1, von jedem GPX-Programm lesbar):
/// <list type="bullet">
/// <item><c>metadata/extensions/andytours:planung</c> mit dem Fahrprofil und den Wegpunkten des Benutzers. Andere
/// Programme übergehen diese Erweiterung; AndyTours erkennt daran eine eigene Planung.</item>
/// <item>Je Abbiegepunkt ein <c>wpt</c> mit dem Text der Abbiegung als Name (z. B. „Kreisverkehr 2. Ausfahrt“) und
/// der Richtung als Typ, dazu in <c>andytours:abbiegung</c> Teilstück, Richtung und Ausfahrt.</item>
/// <item>Ein Track mit einem Segment je Teilstück, Punkte mit Höhe, falls bekannt. Eine Luftlinie trägt
/// <c>andytours:luftlinie</c> in den Erweiterungen ihres Segments.</item>
/// </list>
/// Zahlen stehen mit Punkt als Dezimaltrennzeichen, unabhängig von der Spracheinstellung.
/// </remarks>
public static class PlanungsGpx
{
    /// <summary>Namensraum von GPX 1.1.</summary>
    private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";

    /// <summary>Namensraum der AndyTours-Erweiterung.</summary>
    private static readonly XNamespace AndyTours = "urn:andytours:planung:1";

    /// <summary>
    /// Liefert den Text der GPX-Datei einer Planung.
    /// </summary>
    /// <remarks>
    /// Ein noch nicht ermitteltes Teilstück wird als Luftlinie geschrieben.
    /// </remarks>
    /// <param name="planung">Die Planung mit mindestens zwei Wegpunkten.</param>
    /// <param name="name">Name der Strecke, z. B. der Dateiname ohne Endung.</param>
    /// <returns>Der Inhalt der Datei.</returns>
    /// <exception cref="ArgumentException">Die Planung hat weniger als zwei Wegpunkte.</exception>
    public static string Schreiben(Tourenplanung planung, string name)
    {
        if (planung.Wegpunkte.Count < 2)
        {
            throw new ArgumentException("Eine Planung braucht zum Speichern mindestens zwei Wegpunkte.", nameof(planung));
        }

        List<Teilstueck> teilstuecke = [.. planung.Teilstuecke.Select((t, i) =>
            t ?? ErstelleLuftlinie.Ausfuehren(planung.Wegpunkte[i], planung.Wegpunkte[i + 1]))];

        var dokument = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(Gpx + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", "AndyTours"),
                new XAttribute(XNamespace.Xmlns + "andytours", AndyTours.NamespaceName),
                new XElement(Gpx + "metadata",
                    new XElement(Gpx + "name", name),
                    new XElement(Gpx + "extensions",
                        new XElement(AndyTours + "planung",
                            new XAttribute("profil", planung.Profil.ToString()),
                            planung.Wegpunkte.Select(w => new XElement(AndyTours + "wegpunkt", Lage(w)))))),
                teilstuecke.SelectMany((t, i) => t.Abbiegepunkte.Select(a => Abbiegung(a, i))),
                new XElement(Gpx + "trk",
                    new XElement(Gpx + "name", name),
                    teilstuecke.Select(Segment))));
        return dokument.Declaration + Environment.NewLine + dokument.Root;
    }

    /// <summary>
    /// Liest eine von AndyTours gespeicherte Planung.
    /// </summary>
    /// <param name="text">Inhalt der Datei.</param>
    /// <returns>
    /// Die Planung oder <c>null</c>, wenn der Text kein lesbares GPX ist, die AndyTours-Erweiterung fehlt (z. B. bei
    /// einem GPX-Export einer gefahrenen Tour) oder Wegpunkte und Track nicht zusammenpassen.
    /// </returns>
    public static Tourenplanung? Lesen(string text)
    {
        try
        {
            XElement? wurzel = XDocument.Parse(text).Root;
            XElement? erweiterung = wurzel?.Element(Gpx + "metadata")?.Element(Gpx + "extensions")?.Element(AndyTours + "planung");
            if (wurzel is null || erweiterung is null
                || !Enum.TryParse((string?)erweiterung.Attribute("profil"), out Fahrprofil profil) || !Enum.IsDefined(profil))
            {
                return null;
            }

            List<Geoposition> wegpunkte = [.. erweiterung.Elements(AndyTours + "wegpunkt").Select(Position)];
            List<XElement> segmente = [.. wurzel.Elements(Gpx + "trk").Take(1).Elements(Gpx + "trkseg")];
            if (wegpunkte.Count < 2 || segmente.Count != wegpunkte.Count - 1)
            {
                return null;
            }

            List<List<Abbiegepunkt>> abbiegepunkte = [.. segmente.Select(_ => new List<Abbiegepunkt>())];
            foreach (XElement wpt in wurzel.Elements(Gpx + "wpt"))
            {
                XElement? abbiegung = wpt.Element(Gpx + "extensions")?.Element(AndyTours + "abbiegung");
                if (abbiegung is not null
                    && (int?)abbiegung.Attribute("teilstueck") is int nummer && nummer >= 0 && nummer < segmente.Count
                    && Enum.TryParse((string?)abbiegung.Attribute("richtung"), out Abbiegerichtung richtung) && Enum.IsDefined(richtung))
                {
                    abbiegepunkte[nummer].Add(new Abbiegepunkt(Position(wpt), richtung, (int?)abbiegung.Attribute("ausfahrt")));
                }
            }

            List<Teilstueck?> teilstuecke = [];
            for (int i = 0; i < segmente.Count; i++)
            {
                List<Verlaufspunkt> punkte = [.. segmente[i].Elements(Gpx + "trkpt").Select(p => new Verlaufspunkt
                {
                    Position = Position(p),
                    Hoehe = (double?)p.Element(Gpx + "ele"),
                })];
                if (punkte.Count < 2)
                {
                    return null;
                }

                bool istLuftlinie = segmente[i].Element(Gpx + "extensions")?.Element(AndyTours + "luftlinie") is not null;
                teilstuecke.Add(new Teilstueck(punkte, abbiegepunkte[i], istLuftlinie));
            }

            return new Tourenplanung(profil, wegpunkte, teilstuecke);
        }
        catch (Exception fehler) when (fehler is XmlException or FormatException or ArgumentNullException)
        {
            return null;
        }
    }

    /// <summary>Ein Abbiegepunkt als GPX-Wegpunkt.</summary>
    private static XElement Abbiegung(Abbiegepunkt abbiegepunkt, int teilstueck) =>
        new(Gpx + "wpt", Lage(abbiegepunkt.Position),
            new XElement(Gpx + "name", BeschreibeAbbiegung.Ausfuehren(abbiegepunkt)),
            new XElement(Gpx + "type", abbiegepunkt.Richtung.ToString()),
            new XElement(Gpx + "extensions",
                new XElement(AndyTours + "abbiegung",
                    new XAttribute("teilstueck", teilstueck),
                    new XAttribute("richtung", abbiegepunkt.Richtung.ToString()),
                    abbiegepunkt.Ausfahrt is int ausfahrt ? new XAttribute("ausfahrt", ausfahrt) : null)));

    /// <summary>Ein Teilstück als Tracksegment.</summary>
    private static XElement Segment(Teilstueck teilstueck) =>
        new(Gpx + "trkseg",
            teilstueck.Punkte.Where(p => p.Position is not null).Select(p => new XElement(Gpx + "trkpt", Lage(p.Position!),
                p.Hoehe is double hoehe ? new XElement(Gpx + "ele", Zahl(Math.Round(hoehe, 1))) : null)),
            teilstueck.IstLuftlinie ? new XElement(Gpx + "extensions", new XElement(AndyTours + "luftlinie")) : null);

    /// <summary>Die Attribute <c>lat</c> und <c>lon</c> einer Position.</summary>
    private static XAttribute[] Lage(Geoposition position) =>
        [new XAttribute("lat", Zahl(position.Breite)), new XAttribute("lon", Zahl(position.Laenge))];

    /// <summary>Die Position aus den Attributen <c>lat</c> und <c>lon</c>.</summary>
    private static Geoposition Position(XElement element) =>
        new(
            double.Parse((string?)element.Attribute("lat") ?? throw new FormatException("lat fehlt"), CultureInfo.InvariantCulture),
            double.Parse((string?)element.Attribute("lon") ?? throw new FormatException("lon fehlt"), CultureInfo.InvariantCulture));

    /// <summary>Zahl mit Punkt als Dezimaltrennzeichen.</summary>
    private static string Zahl(double wert) => wert.ToString("0.#######", CultureInfo.InvariantCulture);
}

using System.Globalization;
using System.Xml.Linq;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt aus den Etappen des Tourdetails den Inhalt einer GPX-Datei („Als GPX exportieren…“).
/// </summary>
public static class ErstelleGpx
{
    /// <summary>Namensraum von GPX 1.1.</summary>
    private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";

    /// <summary>
    /// Liefert den Text einer GPX-1.1-Datei mit einem Track je Etappe.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Jede Etappe mit Positionen wird ein Track (<c>trk</c>) mit dem Namen ihrer Einheit, in der Reihenfolge
    /// der Etappen (schon nach Datum und Startzeit geordnet).</item>
    /// <item>Jeder Punkt (<c>trkpt</c>) trägt Breite und Länge, dazu die Höhe (<c>ele</c>), falls bekannt.
    /// Uhrzeiten enthält die Datei bewusst nicht; GPX verlangt je Punkt nur Breite und Länge.</item>
    /// <item>Messpunkte ohne Position (z. B. im Tunnel) und Etappen ohne Positionen werden übergangen – wie in der
    /// Streckenkarte (<see cref="ErmittleStrecke"/>).</item>
    /// <item>Zahlen stehen mit Punkt als Dezimaltrennzeichen, unabhängig von der Spracheinstellung.</item>
    /// </list>
    /// </remarks>
    /// <param name="etappen">Die Etappen in ihrer Reihenfolge.</param>
    /// <returns>Der Inhalt der Datei oder <c>null</c>, wenn keine Etappe eine Position hat.</returns>
    public static string? Ausfuehren(IReadOnlyList<Etappe> etappen)
    {
        List<XElement> tracks = etappen
            .Select(etappe => (etappe.Einheit.Name, Punkte: (etappe.Verlauf ?? []).Where(p => p.Position is not null).ToList()))
            .Where(e => e.Punkte.Count > 0)
            .Select(e => new XElement(Gpx + "trk",
                new XElement(Gpx + "name", e.Name),
                new XElement(Gpx + "trkseg", e.Punkte.Select(Trackpunkt))))
            .ToList();
        if (tracks.Count == 0)
        {
            return null;
        }

        var dokument = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(Gpx + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", "AndyTours"),
                tracks));
        return dokument.Declaration + Environment.NewLine + dokument.Root;
    }

    /// <summary>Ein Trackpunkt mit Position und, falls bekannt, Höhe.</summary>
    private static XElement Trackpunkt(Verlaufspunkt punkt)
    {
        var element = new XElement(Gpx + "trkpt",
            new XAttribute("lat", Zahl(punkt.Position!.Breite)),
            new XAttribute("lon", Zahl(punkt.Position.Laenge)));
        if (punkt.Hoehe is double hoehe)
        {
            element.Add(new XElement(Gpx + "ele", Zahl(Math.Round(hoehe, 1))));
        }

        return element;
    }

    /// <summary>Zahl mit Punkt als Dezimaltrennzeichen.</summary>
    private static string Zahl(double wert) => wert.ToString("0.#######", CultureInfo.InvariantCulture);
}

using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Wie Overlays aussehen: Farbwerte und Anzeigenamen der <see cref="Overlayfarbe"/>n und die Overlay-Ebene der Karten.
/// </summary>
/// <remarks>
/// Overlay-Linien sind 3 px breit und leicht durchscheinend. Damit unterscheiden sie sich von der eigenen Strecke
/// (4 px, deckend), auch wenn die Farbe gleich ist (z. B. die violette dritte Etappe einer Mehrtagestour). Die Ebene liegt
/// direkt über den Kartenausschnitten, also unter Strecke, Wegpunkten und Abbiegepunkten.
/// </remarks>
public static class Overlaydarstellung
{
    /// <summary>Name der Overlay-Ebene einer Karte.</summary>
    public const string Ebenenname = "Overlays";

    /// <summary>Deckkraft der Linien (0–255): leicht durchscheinend.</summary>
    private const int Deckkraft = 190;

    /// <summary>Linienbreite in Pixeln.</summary>
    private const double Linienbreite = 3;

    /// <summary>Rand um ein Overlay bei „ganze Strecke zeigen“, als Anteil seiner Breite bzw. Höhe.</summary>
    private const double Rand = 0.08;

    /// <summary>Anzeigenamen und Farbwerte (Rot, Grün, Blau) in der Reihenfolge der Aufzählung.</summary>
    private static readonly Dictionary<Overlayfarbe, (string Name, byte R, byte G, byte B)> Farben = new()
    {
        [Overlayfarbe.Violett] = ("Violett", 0x9B, 0x30, 0xD9),
        [Overlayfarbe.Blau] = ("Blau", 0x1F, 0x4E, 0xD8),
        [Overlayfarbe.Hellblau] = ("Hellblau", 0x3B, 0xA7, 0xF0),
        [Overlayfarbe.Tuerkis] = ("Türkis", 0x00, 0xA8, 0xA8),
        [Overlayfarbe.Gruen] = ("Grün", 0x2E, 0x9E, 0x3A),
        [Overlayfarbe.Gelb] = ("Gelb", 0xE8, 0xC2, 0x00),
        [Overlayfarbe.Orange] = ("Orange", 0xF0, 0x7A, 0x10),
        [Overlayfarbe.Rot] = ("Rot", 0xD6, 0x22, 0x22),
        [Overlayfarbe.Pink] = ("Pink", 0xE8, 0x3E, 0x9C),
        [Overlayfarbe.Braun] = ("Braun", 0x8B, 0x5A, 0x2B),
        [Overlayfarbe.Grau] = ("Grau", 0x80, 0x80, 0x80),
        [Overlayfarbe.Schwarz] = ("Schwarz", 0x20, 0x20, 0x20),
    };

    /// <summary>Der angezeigte Name einer Farbe, z. B. „Türkis“.</summary>
    /// <param name="farbe">Die Farbe.</param>
    /// <returns>Der Name.</returns>
    public static string Anzeigename(Overlayfarbe farbe) => Farben[farbe].Name;

    /// <summary>Die Farbe als WPF-Farbe (für die Farbfelder in Verwaltung und Einstellungen).</summary>
    /// <param name="farbe">Die Farbe.</param>
    /// <returns>Die deckende WPF-Farbe.</returns>
    public static System.Windows.Media.Color Farbwert(Overlayfarbe farbe)
    {
        (_, byte r, byte g, byte b) = Farben[farbe];
        return System.Windows.Media.Color.FromRgb(r, g, b);
    }

    /// <summary>Erzeugt die leere Overlay-Ebene einer Karte; gefüllt wird sie mit <see cref="Fuelle"/>.</summary>
    /// <returns>Die Ebene.</returns>
    public static MemoryLayer ErzeugeEbene() => new(Ebenenname) { Style = null };

    /// <summary>
    /// Füllt die Overlay-Ebene einer Karte mit den eingeschalteten Overlays (<see cref="Overlaybestand"/>) und zeichnet
    /// die Karte neu.
    /// </summary>
    /// <remarks>
    /// Jeder Abschnitt wird eine eigene Linie; ein Abschnitt aus nur einem Punkt wird ein kleiner Punkt.
    /// </remarks>
    /// <param name="karte">Die Karte mit einer Ebene namens <see cref="Ebenenname"/>.</param>
    public static void Fuelle(Map karte)
    {
        var ebene = (MemoryLayer)karte.Layers.First(e => e.Name == Ebenenname);
        ebene.Features = [.. Overlaybestand.Eingeschaltete.SelectMany(overlay =>
        {
            (_, byte r, byte g, byte b) = Farben[overlay.Farbe ?? Overlaybestand.Farbe];
            var farbe = Mapsui.Styles.Color.FromArgb(Deckkraft, r, g, b);
            return overlay.Abschnitte.Select(abschnitt => Linie(abschnitt, farbe));
        })];

        // Refresh statt RefreshData: RefreshData lädt nur neu, gezeichnet würde erst bei der nächsten Mausaktion
        karte.Refresh();
    }

    /// <summary>
    /// Zeigt die ganze Strecke eines Overlays auf der Karte, mit etwas Rand.
    /// </summary>
    /// <param name="karte">Die Karte.</param>
    /// <param name="overlay">Das Overlay.</param>
    public static void ZeigeGanz(Map karte, Overlay overlay)
    {
        (Geoposition suedWest, Geoposition nordOst) = UmschliesseOverlay.Ausfuehren(overlay);
        MPoint sw = InKartenkoordinaten(suedWest);
        MPoint no = InKartenkoordinaten(nordOst);

        // Mindestgröße, damit ein sehr kurzes Overlay (oder ein einzelner Punkt) nicht bis zum Anschlag vergrößert wird
        double breite = Math.Max(no.X - sw.X, 500);
        double hoehe = Math.Max(no.Y - sw.Y, 500);
        MPoint mitte = new((sw.X + no.X) / 2, (sw.Y + no.Y) / 2);
        karte.Navigator.ZoomToBox(
            new MRect(
                mitte.X - breite * (0.5 + Rand), mitte.Y - hoehe * (0.5 + Rand),
                mitte.X + breite * (0.5 + Rand), mitte.Y + hoehe * (0.5 + Rand)),
            MBoxFit.Fit);
    }

    /// <summary>Die Linie eines Abschnitts; ein einzelner Punkt wird ein kleiner Kreis.</summary>
    private static IFeature Linie(IReadOnlyList<Geoposition> abschnitt, Mapsui.Styles.Color farbe)
    {
        Coordinate[] punkte = [.. abschnitt.Select(InKartenkoordinaten).Select(p => new Coordinate(p.X, p.Y))];
        if (punkte.Length > 1)
        {
            return new GeometryFeature(new LineString(punkte)) { Styles = [new VectorStyle { Line = new Pen(farbe, Linienbreite) }] };
        }

        return new PointFeature(punkte[0].X, punkte[0].Y)
        {
            Styles = [new SymbolStyle { SymbolType = SymbolType.Ellipse, SymbolScale = 0.3, Fill = new Mapsui.Styles.Brush(farbe), Outline = null }],
        };
    }

    /// <summary>Rechnet eine Position in Grad in die Koordinaten der Karte (Web-Mercator) um.</summary>
    private static MPoint InKartenkoordinaten(Geoposition position) =>
        SphericalMercator.FromLonLat(new MPoint(position.Laenge, position.Breite));
}

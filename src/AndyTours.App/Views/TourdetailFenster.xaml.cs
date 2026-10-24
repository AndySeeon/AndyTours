using System.ComponentModel;
// WPF-Projekte binden System.IO nicht automatisch ein
using System.IO;
using System.Windows;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using NetTopologySuite.Geometries;
using AndyTours.App.Funktionen;
using AndyTours.App.ViewModels;
using AndyTours.Core.Modell;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Tourdetail“.
/// </summary>
/// <remarks>
/// Das Höhenprofil liegt vollständig im <see cref="TourdetailViewModel"/>. Die Karte der Karteikarte „Karte“ baut
/// dieses Code-Behind auf, weil Mapsui (siehe <c>docs/adr/0003-kartenbibliothek.md</c>) seine Ebenen nicht über
/// Bindungen bekommt:
/// <list type="bullet">
/// <item>unten die Kartenausschnitte der gewählten Kartenart, mit Zwischenspeicher je Kartenart im Ordner
/// <see cref="Programmpfade.Kartenspeicher"/>,</item>
/// <item>darüber die Strecke als einfarbige Linie,</item>
/// <item>oben Start (grüner Kreis) und Ziel (rotes Quadrat).</item>
/// </list>
/// Mausrad und Ziehen bringt Mapsui selbst mit. Das Fenster wird nicht modal geöffnet und ohne Rückfrage geschlossen.
/// </remarks>
public partial class TourdetailFenster : Window
{
    /// <summary>Farbe der Strecke.</summary>
    private static readonly Mapsui.Styles.Color Streckenfarbe = Mapsui.Styles.Color.FromArgb(230, 0x2E, 0x6D, 0xB4);

    /// <summary>Farbe des Startzeichens (passend zur Zeichenerklärung im XAML).</summary>
    private static readonly Mapsui.Styles.Color Startfarbe = Mapsui.Styles.Color.FromArgb(255, 0x27, 0xAE, 0x60);

    /// <summary>Farbe des Zielzeichens (passend zur Zeichenerklärung im XAML).</summary>
    private static readonly Mapsui.Styles.Color Zielfarbe = Mapsui.Styles.Color.FromArgb(255, 0xC0, 0x39, 0x2B);

    /// <summary>Rand um die Strecke bei „Ganze Strecke“, als Anteil ihrer Breite bzw. Höhe.</summary>
    private const double Rand = 0.08;

    /// <summary>ViewModel der Karteikarte „Karte“.</summary>
    private readonly StreckenkarteViewModel _streckenkarte;

    /// <summary>Bereits erzeugte Kartenebenen je Kartenart (beim Zurückschalten wiederverwendet).</summary>
    private readonly Dictionary<Kartenart, TileLayer> _kartenebenen = [];

    /// <summary>Umschließendes Rechteck der Strecke in Kartenkoordinaten (Web-Mercator) mit Rand.</summary>
    private MRect? _ganzeStrecke;

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Kopfzeile und Verlauf der Tour.</param>
    public TourdetailFenster(TourdetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _streckenkarte = viewModel.Streckenkarte;

        if (_streckenkarte.Strecke is Strecke strecke)
        {
            BaueKarte(strecke);
            _streckenkarte.PropertyChanged += StreckenkarteGeaendert;
            _streckenkarte.GanzeStreckeAngefordert += (_, _) => ZeigeGanzeStrecke();
            Kartenabruf.Fehlgeschlagen += LadefehlerGemeldet;
            Closed += (_, _) =>
            {
                Kartenabruf.Fehlgeschlagen -= LadefehlerGemeldet;
                Karte.Map.Dispose();
            };
        }
    }

    /// <summary>
    /// Ein Kartenausschnitt konnte nicht geladen werden: Hinweis im Thread der Oberfläche einblenden.
    /// </summary>
    /// <remarks>
    /// Die Verbindung ist allen Tourdetail-Fenstern gemeinsam; ohne Internet trifft der Hinweis aber ohnehin jedes
    /// Fenster, dessen Karte gerade lädt.
    /// </remarks>
    private void LadefehlerGemeldet(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(_streckenkarte.MeldeLadefehler);

    /// <summary>
    /// Baut die Karte auf: Kartenebene, Linie, Start- und Zielzeichen, Ausschnitt mit der ganzen Strecke.
    /// </summary>
    /// <param name="strecke">Die Strecke der Tour.</param>
    private void BaueKarte(Strecke strecke)
    {
        var karte = new Map();

        // Mapsui legt von sich aus ein Protokoll- und ein Leistungs-Widget auf die Karte (im Debug-Build sichtbar)
        karte.Widgets.Clear();
        karte.Navigator.RotationLock = true;

        karte.Layers.Add(Kartenebene(_streckenkarte.Kartenart));

        Coordinate[] linie = [.. strecke.Punkte.Select(InKartenkoordinaten).Select(p => new Coordinate(p.X, p.Y))];
        Geometry geometrie = linie.Length > 1 ? new LineString(linie) : new NetTopologySuite.Geometries.Point(linie[0]);
        karte.Layers.Add(new MemoryLayer("Strecke")
        {
            Features = [new GeometryFeature(geometrie)],
            Style = new VectorStyle { Line = new Pen(Streckenfarbe, 4) },
        });

        karte.Layers.Add(new MemoryLayer("Start und Ziel")
        {
            Features =
            [
                Zeichen(strecke.Punkte[0], SymbolType.Ellipse, Startfarbe, 0.8),
                Zeichen(strecke.Punkte[^1], SymbolType.Rectangle, Zielfarbe, 0.45),
            ],
            Style = null,
        });

        MPoint suedWest = InKartenkoordinaten(strecke.SuedWest);
        MPoint nordOst = InKartenkoordinaten(strecke.NordOst);

        // Mindestgröße, damit eine sehr kurze Strecke (oder ein einzelner Punkt) nicht bis zum Anschlag vergrößert wird
        double breite = Math.Max(nordOst.X - suedWest.X, 500);
        double hoehe = Math.Max(nordOst.Y - suedWest.Y, 500);
        MPoint mitte = new((suedWest.X + nordOst.X) / 2, (suedWest.Y + nordOst.Y) / 2);
        _ganzeStrecke = new MRect(
            mitte.X - breite * (0.5 + Rand), mitte.Y - hoehe * (0.5 + Rand),
            mitte.X + breite * (0.5 + Rand), mitte.Y + hoehe * (0.5 + Rand));

        Karte.Map = karte;

        // Mapsui merkt sich den Aufruf, bis die Karte ihre Größe kennt (die Karteikarte ist anfangs nicht sichtbar)
        ZeigeGanzeStrecke();
    }

    /// <summary>Zeigt die ganze Strecke (beim Öffnen und bei „Ganze Strecke“).</summary>
    private void ZeigeGanzeStrecke()
    {
        if (_ganzeStrecke is not null)
        {
            Karte.Map.Navigator.ZoomToBox(_ganzeStrecke, MBoxFit.Fit);
        }
    }

    /// <summary>
    /// Wechselt bei geänderter Kartenart die unterste Ebene; Strecke und Ausschnitt bleiben, wie sie sind.
    /// </summary>
    private void StreckenkarteGeaendert(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StreckenkarteViewModel.Kartenart))
        {
            return;
        }

        LayerCollection ebenen = Karte.Map.Layers;
        ebenen.Remove(ebenen.Get(0));
        ebenen.Insert(0, Kartenebene(_streckenkarte.Kartenart));
        Karte.Map.RefreshData();
    }

    /// <summary>
    /// Liefert die Kartenebene einer Kartenart; beim ersten Mal wird sie erzeugt.
    /// </summary>
    /// <remarks>
    /// Die Ausschnitte werden je Kartenart in einem eigenen Unterordner des Kartenspeichers abgelegt und von dort
    /// gelesen, sobald sie einmal geladen sind – auch ohne Internet. Der Zwischenspeicher verfällt nicht.
    /// OpenStreetMap liefert bis Stufe 19, OpenTopoMap bis Stufe 17.
    /// </remarks>
    /// <param name="kartenart">Die gewünschte Kartenart.</param>
    /// <returns>Die Kartenebene.</returns>
    private TileLayer Kartenebene(Kartenart kartenart)
    {
        if (_kartenebenen.TryGetValue(kartenart, out TileLayer? vorhanden))
        {
            return vorhanden;
        }

        var zwischenspeicher = new FileCache(Path.Combine(Programmpfade.Kartenspeicher, kartenart.ToString()), "png");
        HttpTileSource quelle = kartenart switch
        {
            Kartenart.OpenTopoMap => new HttpTileSource(
                new GlobalSphericalMercator(YAxis.OSM, 0, 17), "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png",
                ["a", "b", "c"], name: kartenart.ToString(), persistentCache: zwischenspeicher,
                configureHttpRequestMessage: Kartenabruf.SetzeKennung),
            _ => new HttpTileSource(
                new GlobalSphericalMercator(YAxis.OSM, 0, 19), "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                name: kartenart.ToString(), persistentCache: zwischenspeicher,
                configureHttpRequestMessage: Kartenabruf.SetzeKennung),
        };

        var ebene = new TileLayer(quelle, httpClient: Kartenabruf.Verbindung) { Name = kartenart.ToString() };
        _kartenebenen[kartenart] = ebene;
        return ebene;
    }

    /// <summary>
    /// Ein Start- oder Zielzeichen an einer Position.
    /// </summary>
    /// <remarks>
    /// Der Startkreis ist größer als das Zielquadrat: Bei einer Rundtour liegen beide übereinander, und der Kreis
    /// bleibt als Ring um das Quadrat sichtbar.
    /// </remarks>
    private static PointFeature Zeichen(Geoposition position, SymbolType form, Mapsui.Styles.Color farbe, double groesse) =>
        new(InKartenkoordinaten(position))
        {
            Styles =
            [
                new SymbolStyle
                {
                    SymbolType = form,
                    SymbolScale = groesse,
                    Fill = new Mapsui.Styles.Brush(farbe),
                    Outline = new Pen(Mapsui.Styles.Color.White, 2),
                },
            ],
        };

    /// <summary>Rechnet eine Position in Grad in die Koordinaten der Karte (Web-Mercator) um.</summary>
    private static MPoint InKartenkoordinaten(Geoposition position) =>
        SphericalMercator.FromLonLat(new MPoint(position.Laenge, position.Breite));
}

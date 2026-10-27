using System.ComponentModel;
using System.Windows;
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
/// <item>unten die Kartenausschnitte der gewählten Kartenart, mit Zwischenspeicher (<see cref="Kartenebenen"/>),</item>
/// <item>darüber die eingeschalteten Overlays (<see cref="Overlaydarstellung"/>); sie folgen jeder Änderung in der
/// Overlay-Verwaltung sofort (<see cref="Overlaybestand"/>),</item>
/// <item>darüber die Strecke als einfarbige Linie; bei einer Mehrtagestour eine Linie je Etappe, deren Farben
/// reihum wechseln, sodass benachbarte Etappen sich unterscheiden,</item>
/// <item>oben Start (grüner Kreis) und Ziel (rotes Quadrat) der ganzen Tour, bei einer Mehrtagestour dazu ein kleiner
/// Punkt an jedem Etappenwechsel.</item>
/// </list>
/// Mausrad und Ziehen bringt Mapsui selbst mit. Das Fenster wird nicht modal geöffnet und ohne Rückfrage geschlossen.
/// </remarks>
public partial class TourdetailFenster : Window
{
    /// <summary>
    /// Farben der Strecke: eine Tour in der ersten Farbe, die Etappen einer Mehrtagestour reihum. Grün und Rot fehlen
    /// bewusst, damit Start- und Zielzeichen sich abheben.
    /// </summary>
    private static readonly Mapsui.Styles.Color[] Streckenfarben =
    [
        Mapsui.Styles.Color.FromArgb(230, 0x2E, 0x6D, 0xB4),
        Mapsui.Styles.Color.FromArgb(230, 0xE6, 0x7E, 0x22),
        Mapsui.Styles.Color.FromArgb(230, 0x8E, 0x44, 0xAD),
        Mapsui.Styles.Color.FromArgb(230, 0x16, 0xA0, 0x85),
    ];

    /// <summary>Farbe der Punkte an den Etappenwechseln (passend zur Zeichenerklärung im XAML).</summary>
    private static readonly Mapsui.Styles.Color Wechselfarbe = Mapsui.Styles.Color.FromArgb(255, 0x55, 0x55, 0x55);

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

    /// <summary>Umschließendes Rechteck aller Strecken in Kartenkoordinaten (Web-Mercator) mit Rand.</summary>
    private MRect? _ganzeStrecke;

    /// <summary>Name der Ebene mit den Linien der Strecken.</summary>
    private const string Streckenebene = "Strecke";

    /// <summary>Name der Ebene mit den Zeichen für Start, Ziel und Etappenwechsel.</summary>
    private const string Zeichenebene = "Start, Ziel und Etappenwechsel";

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Kopfzeile und Verlauf der Tour bzw. der Etappen.</param>
    public TourdetailFenster(TourdetailViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        // Das Kartenbild bleibt in jeder Stufe so groß wie bei 100 %; nur die Bedienelemente um die Karte wachsen mit
        Oberflaechenskalierung.Ausnehmen(Karte);
        DataContext = viewModel;
        _streckenkarte = viewModel.Streckenkarte;

        if (_streckenkarte.HatStrecke)
        {
            BaueKarte();
            _streckenkarte.PropertyChanged += StreckenkarteGeaendert;
            _streckenkarte.GanzeStreckeAngefordert += (_, _) => ZeigeGanzeStrecke();
            _streckenkarte.StreckeGeaendert += (_, _) => ZeichneStrecke();
            Kartenabruf.Fehlgeschlagen += LadefehlerGemeldet;
            Overlaybestand.Geaendert += OverlaysGeaendert;
            Overlaybestand.GanzesOverlayAngefordert += GanzesOverlayAngefordert;
            Closed += (_, _) =>
            {
                Kartenabruf.Fehlgeschlagen -= LadefehlerGemeldet;
                Overlaybestand.Geaendert -= OverlaysGeaendert;
                Overlaybestand.GanzesOverlayAngefordert -= GanzesOverlayAngefordert;
                Karte.Map.Dispose();
            };
        }
    }

    /// <summary>
    /// Schaltfläche „Overlays…“: öffnet die Overlay-Verwaltung über den Befehl des Hauptfensters (dasselbe Fenster wie
    /// „Tourenplanung → Overlays…“; es gehört zum Hauptfenster und bleibt offen, wenn dieses Fenster geschlossen wird).
    /// </summary>
    private void OverlaysOeffnen_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow?.DataContext is HauptfensterViewModel hauptfenster)
        {
            hauptfenster.OverlaysOeffnenCommand.Execute(null);
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

    /// <summary>Die eingeschalteten Overlays oder ihre Farben haben sich geändert: Overlay-Ebene neu füllen.</summary>
    private void OverlaysGeaendert(object? sender, EventArgs e) => Overlaydarstellung.Fuelle(Karte.Map);

    /// <summary>Doppelklick auf ein Overlay in der Verwaltung: dessen ganze Strecke zeigen.</summary>
    private void GanzesOverlayAngefordert(object? sender, Overlay overlay) => Overlaydarstellung.ZeigeGanz(Karte.Map, overlay);

    /// <summary>
    /// Baut die Karte auf: Kartenebene, eine Linie je Strecke, Zeichen für Start, Ziel und Etappenwechsel, Ausschnitt
    /// mit allen Strecken.
    /// </summary>
    private void BaueKarte()
    {
        var karte = new Map();

        // Mapsui legt von sich aus ein Protokoll- und ein Leistungs-Widget auf die Karte (im Debug-Build sichtbar)
        karte.Widgets.Clear();
        karte.Navigator.RotationLock = true;

        karte.Layers.Add(Kartenebene(_streckenkarte.Kartenart));

        // Overlays direkt über den Kartenausschnitten, damit sie die eigene Strecke nicht verdecken
        karte.Layers.Add(Overlaydarstellung.ErzeugeEbene());
        karte.Layers.Add(new MemoryLayer(Streckenebene) { Style = null });
        karte.Layers.Add(new MemoryLayer(Zeichenebene) { Style = null });
        FuelleEbenen(karte);

        Karte.Map = karte;
        Overlaydarstellung.Fuelle(karte);

        // Mapsui merkt sich den Aufruf, bis die Karte ihre Größe kennt (die Karteikarte ist anfangs nicht sichtbar)
        ZeigeGanzeStrecke();
    }

    /// <summary>
    /// Zeichnet Linie und Zeichen neu, nachdem „Track bearbeiten“ die Strecke geändert hat. Der Ausschnitt bleibt,
    /// damit die Karte beim Ziehen eines Reglers nicht springt; „Ganze Strecke“ zeigt danach die bearbeitete Strecke.
    /// </summary>
    private void ZeichneStrecke()
    {
        FuelleEbenen(Karte.Map);

        // Refresh statt RefreshData: RefreshData lädt nur neu, gezeichnet würde erst bei der nächsten Mausaktion
        Karte.Map.Refresh();
    }

    /// <summary>
    /// Füllt die Ebenen mit einer Linie je Strecke und den Zeichen für Start, Ziel und Etappenwechsel und bestimmt
    /// das Rechteck für „Ganze Strecke“.
    /// </summary>
    /// <param name="karte">Die Karte mit den beiden Ebenen.</param>
    private void FuelleEbenen(Map karte)
    {
        IReadOnlyList<Strecke> strecken = _streckenkarte.Strecken;
        var streckenebene = (MemoryLayer)karte.Layers.First(ebene => ebene.Name == Streckenebene);
        var zeichenebene = (MemoryLayer)karte.Layers.First(ebene => ebene.Name == Zeichenebene);
        streckenebene.Features = [.. strecken.Select((strecke, i) => Linie(strecke, Streckenfarben[i % Streckenfarben.Length]))];

        // Wechsel zuerst, damit Start und Ziel darüber liegen
        zeichenebene.Features =
        [
            .. strecken.Skip(1).Select(strecke => Zeichen(strecke.Punkte[0], SymbolType.Ellipse, Wechselfarbe, 0.35)),
            Zeichen(strecken[0].Punkte[0], SymbolType.Ellipse, Startfarbe, 0.8),
            Zeichen(strecken[^1].Punkte[^1], SymbolType.Rectangle, Zielfarbe, 0.45),
        ];

        MPoint suedWest = InKartenkoordinaten(_streckenkarte.SuedWest!);
        MPoint nordOst = InKartenkoordinaten(_streckenkarte.NordOst!);

        // Mindestgröße, damit eine sehr kurze Strecke (oder ein einzelner Punkt) nicht bis zum Anschlag vergrößert wird
        double breite = Math.Max(nordOst.X - suedWest.X, 500);
        double hoehe = Math.Max(nordOst.Y - suedWest.Y, 500);
        MPoint mitte = new((suedWest.X + nordOst.X) / 2, (suedWest.Y + nordOst.Y) / 2);
        _ganzeStrecke = new MRect(
            mitte.X - breite * (0.5 + Rand), mitte.Y - hoehe * (0.5 + Rand),
            mitte.X + breite * (0.5 + Rand), mitte.Y + hoehe * (0.5 + Rand));
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
    /// Liefert die Kartenebene einer Kartenart; beim ersten Mal wird sie erzeugt (<see cref="Kartenebenen"/>).
    /// </summary>
    /// <param name="kartenart">Die gewünschte Kartenart.</param>
    /// <returns>Die Kartenebene.</returns>
    private TileLayer Kartenebene(Kartenart kartenart)
    {
        if (_kartenebenen.TryGetValue(kartenart, out TileLayer? vorhanden))
        {
            return vorhanden;
        }

        TileLayer ebene = Kartenebenen.Erzeuge(kartenart);
        _kartenebenen[kartenart] = ebene;
        return ebene;
    }

    /// <summary>
    /// Die Linie einer Strecke in ihrer Farbe; eine Strecke aus nur einem Punkt wird ein Punkt.
    /// </summary>
    private static GeometryFeature Linie(Strecke strecke, Mapsui.Styles.Color farbe)
    {
        Coordinate[] linie = [.. strecke.Punkte.Select(InKartenkoordinaten).Select(p => new Coordinate(p.X, p.Y))];
        Geometry geometrie = linie.Length > 1 ? new LineString(linie) : new NetTopologySuite.Geometries.Point(linie[0]);
        return new GeometryFeature(geometrie) { Styles = [new VectorStyle { Line = new Pen(farbe, 4) }] };
    }

    /// <summary>
    /// Ein Start-, Ziel- oder Wechselzeichen an einer Position.
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

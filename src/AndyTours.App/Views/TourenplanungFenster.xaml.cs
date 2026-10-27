using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Point = System.Windows.Point;
using AndyTours.App.Funktionen;
using AndyTours.App.ViewModels;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Tour planen“: zeichnet die Planung auf die Karte und wertet die Maus aus.
/// </summary>
/// <remarks>
/// Ebenen der Karte von unten nach oben (siehe <c>docs/adr/0003-kartenbibliothek.md</c>):
/// <list type="bullet">
/// <item>Kartenausschnitte von OpenStreetMap (<see cref="Kartenebenen"/>),</item>
/// <item>die eingeschalteten Overlays (<see cref="Overlaydarstellung"/>, <see cref="Overlaybestand"/>),</item>
/// <item>die Strecke: ermittelte Teilstücke durchgezogen, Luftlinien und noch offene Teilstücke gestrichelt,</item>
/// <item>die Vorschau beim Ziehen (gestrichelt zur Maus),</item>
/// <item>die Abbiegepunkte als kleine orange Punkte,</item>
/// <item>die Wegpunkte: Start grün, Ziel rot, dazwischen weiß mit blauem Rand.</item>
/// </list>
/// Maus:
/// <list type="bullet">
/// <item>Klick (Bewegung höchstens <see cref="Klickspielraum"/> Pixel) neben Strecke und Wegpunkte: Wegpunkt anhängen.</item>
/// <item>Ziehen auf einem Wegpunkt (Treffer bis <see cref="Wegpunktradius"/> Pixel): verschieben.</item>
/// <item>Ziehen auf der Linie (Treffer bis <see cref="Linienradius"/> Pixel): Zwischenpunkt einfügen.</item>
/// <item>Rechtsklick auf einen Wegpunkt: entfernen.</item>
/// <item>Jedes andere Ziehen verschiebt die Karte (wie gewohnt bei Mapsui).</item>
/// </list>
/// Beim Ziehen folgt nur die gestrichelte Vorschau der Maus; ermittelt wird erst beim Loslassen.
/// </remarks>
public partial class TourenplanungFenster : Window
{
    /// <summary>Höchste Mausbewegung in Pixeln, die noch als Klick gilt.</summary>
    private const double Klickspielraum = 4;

    /// <summary>Abstand in Pixeln, bis zu dem ein Wegpunkt getroffen ist.</summary>
    private const double Wegpunktradius = 10;

    /// <summary>Abstand in Pixeln, bis zu dem die Linie getroffen ist.</summary>
    private const double Linienradius = 6;

    /// <summary>Rand um Start- bzw. ganze Planung, als Anteil ihrer Breite bzw. Höhe.</summary>
    private const double Rand = 0.08;

    /// <summary>Farbe der Strecke (wie in der Streckenkarte).</summary>
    private static readonly Mapsui.Styles.Color Streckenfarbe = Mapsui.Styles.Color.FromArgb(230, 0x2E, 0x6D, 0xB4);

    /// <summary>Farbe von Luftlinien, offenen Teilstücken und Vorschau.</summary>
    private static readonly Mapsui.Styles.Color Luftlinienfarbe = Mapsui.Styles.Color.FromArgb(200, 0x55, 0x55, 0x55);

    /// <summary>Farbe der Abbiegepunkte.</summary>
    private static readonly Mapsui.Styles.Color Abbiegefarbe = Mapsui.Styles.Color.FromArgb(255, 0xE6, 0x7E, 0x22);

    /// <summary>Farbe des Startpunkts.</summary>
    private static readonly Mapsui.Styles.Color Startfarbe = Mapsui.Styles.Color.FromArgb(255, 0x27, 0xAE, 0x60);

    /// <summary>Farbe des Zielpunkts.</summary>
    private static readonly Mapsui.Styles.Color Zielfarbe = Mapsui.Styles.Color.FromArgb(255, 0xC0, 0x39, 0x2B);

    /// <summary>Name der Ebene mit der Strecke.</summary>
    private const string Streckenebene = "Strecke";

    /// <summary>Name der Ebene mit der Vorschau beim Ziehen.</summary>
    private const string Vorschauebene = "Vorschau";

    /// <summary>Name der Ebene mit den Abbiegepunkten.</summary>
    private const string Abbiegeebene = "Abbiegepunkte";

    /// <summary>Name der Ebene mit den Wegpunkten.</summary>
    private const string Wegpunktebene = "Wegpunkte";

    /// <summary>ViewModel der Planung.</summary>
    private readonly TourenplanungViewModel _viewModel;

    /// <summary>Mausposition beim Drücken der linken Taste; <c>null</c>, wenn sie nicht gedrückt ist.</summary>
    private Point? _druckpunkt;

    /// <summary>Nummer des gezogenen Wegpunkts oder <c>null</c>.</summary>
    private int? _gezogenerWegpunkt;

    /// <summary>Nummer des Teilstücks, dessen Linie gezogen wird, oder <c>null</c>.</summary>
    private int? _gezogenesTeilstueck;

    /// <summary><c>true</c>, sobald sich die Maus beim Ziehen über den Klickspielraum hinaus bewegt hat.</summary>
    private bool _wirdGezogen;

    /// <summary><c>true</c>, wenn das Schließen nach der Rückfrage bestätigt ist.</summary>
    private bool _schliessenBestaetigt;

    /// <summary>
    /// Erzeugt das Fenster, baut die Karte und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Planung und Startausschnitt.</param>
    public TourenplanungFenster(TourenplanungViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;

        BaueKarte();
        _viewModel.PlanungGeaendert += (_, _) => Zeichne();
        _viewModel.GanzePlanungAngefordert += (_, _) => ZeigeGanzePlanung();

        Karte.PreviewMouseLeftButtonDown += LinksGedrueckt;
        Karte.PreviewMouseMove += MausBewegt;
        Karte.PreviewMouseLeftButtonUp += LinksLosgelassen;
        Karte.PreviewMouseRightButtonUp += RechtsLosgelassen;
        Overlaybestand.Geaendert += OverlaysGeaendert;
        Overlaybestand.GanzesOverlayAngefordert += GanzesOverlayAngefordert;
        Closing += FensterSchliesst;
        Closed += (_, _) =>
        {
            Overlaybestand.Geaendert -= OverlaysGeaendert;
            Overlaybestand.GanzesOverlayAngefordert -= GanzesOverlayAngefordert;
            _viewModel.BeendeErmittlung();
            Karte.Map.Dispose();
        };
    }

    /// <summary>Baut die Karte mit ihren Ebenen und zeigt den Startausschnitt.</summary>
    private void BaueKarte()
    {
        var karte = new Map();

        // Mapsui legt von sich aus ein Protokoll- und ein Leistungs-Widget auf die Karte (im Debug-Build sichtbar)
        karte.Widgets.Clear();
        karte.Navigator.RotationLock = true;
        karte.Layers.Add(Kartenebenen.Erzeuge(Kartenart.OpenStreetMap));

        // Overlays direkt über den Kartenausschnitten, damit sie Strecke, Abbiege- und Wegpunkte nicht verdecken
        karte.Layers.Add(Overlaydarstellung.ErzeugeEbene());
        foreach (string name in new[] { Streckenebene, Vorschauebene, Abbiegeebene, Wegpunktebene })
        {
            karte.Layers.Add(new MemoryLayer(name) { Style = null });
        }

        Karte.Map = karte;
        Overlaydarstellung.Fuelle(karte);
        Zeichne();

        // Mapsui merkt sich den Aufruf, bis die Karte ihre Größe kennt
        karte.Navigator.ZoomToBox(Rechteck(_viewModel.StartSuedWest, _viewModel.StartNordOst, 0), MBoxFit.Fit);
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

    /// <summary>Die eingeschalteten Overlays oder ihre Farben haben sich geändert: Overlay-Ebene neu füllen.</summary>
    private void OverlaysGeaendert(object? sender, EventArgs e) => Overlaydarstellung.Fuelle(Karte.Map);

    /// <summary>Doppelklick auf ein Overlay in der Verwaltung: dessen ganze Strecke zeigen.</summary>
    private void GanzesOverlayAngefordert(object? sender, Overlay overlay) => Overlaydarstellung.ZeigeGanz(Karte.Map, overlay);

    /// <summary>Zeichnet Strecke, Abbiegepunkte und Wegpunkte neu.</summary>
    private void Zeichne()
    {
        Tourenplanung planung = _viewModel.Planung;
        List<IFeature> linien = [];
        for (int i = 0; i < planung.Teilstuecke.Count; i++)
        {
            Teilstueck? teilstueck = planung.Teilstuecke[i];
            List<Geoposition> punkte = teilstueck is null
                ? [planung.Wegpunkte[i], planung.Wegpunkte[i + 1]]
                : [.. teilstueck.Punkte.Where(p => p.Position is not null).Select(p => p.Position!)];
            linien.Add(teilstueck is { IstLuftlinie: false } ? Linie(punkte, Streckenfarbe, false) : Linie(punkte, Luftlinienfarbe, true));
        }

        Ebene(Streckenebene).Features = linien;
        Ebene(Abbiegeebene).Features = [.. planung.Teilstuecke
            .Where(t => t is not null)
            .SelectMany(t => t!.Abbiegepunkte)
            .Select(a => Zeichen(a.Position, Abbiegefarbe, Mapsui.Styles.Color.White, 0.35))];
        ZeichneWegpunkte(planung.Wegpunkte);
        Ebene(Vorschauebene).Features = [];

        // Refresh statt RefreshData: RefreshData lädt nur neu, gezeichnet würde erst bei der nächsten Mausaktion
        Karte.Map.Refresh();
    }

    /// <summary>Zeichnet die Wegpunkte; Start grün, Ziel rot, dazwischen weiß mit blauem Rand.</summary>
    private void ZeichneWegpunkte(IReadOnlyList<Geoposition> wegpunkte)
    {
        Ebene(Wegpunktebene).Features = [.. wegpunkte.Select((w, i) =>
            i == 0 ? Zeichen(w, Startfarbe, Mapsui.Styles.Color.White, 0.7)
            : i == wegpunkte.Count - 1 ? Zeichen(w, Zielfarbe, Mapsui.Styles.Color.White, 0.7)
            : Zeichen(w, Mapsui.Styles.Color.White, Streckenfarbe, 0.55))];
    }

    /// <summary>Zeigt die ganze Planung (nach dem Öffnen einer Datei).</summary>
    private void ZeigeGanzePlanung()
    {
        List<Geoposition> punkte = [.. _viewModel.Planung.Teilstuecke
            .Where(t => t is not null)
            .SelectMany(t => t!.Punkte)
            .Where(p => p.Position is not null)
            .Select(p => p.Position!)
            .Concat(_viewModel.Planung.Wegpunkte)];
        if (punkte.Count == 0)
        {
            return;
        }

        var suedWest = new Geoposition(punkte.Min(p => p.Breite), punkte.Min(p => p.Laenge));
        var nordOst = new Geoposition(punkte.Max(p => p.Breite), punkte.Max(p => p.Laenge));
        Karte.Map.Navigator.ZoomToBox(Rechteck(suedWest, nordOst, Rand), MBoxFit.Fit);
    }

    /// <summary>Linke Taste gedrückt: Wegpunkt oder Linie greifen, sonst einen möglichen Klick vormerken.</summary>
    private void LinksGedrueckt(object sender, MouseButtonEventArgs e)
    {
        Point punkt = e.GetPosition(Karte);
        _druckpunkt = punkt;
        _wirdGezogen = false;
        _gezogenerWegpunkt = GetroffenerWegpunkt(punkt);
        _gezogenesTeilstueck = _gezogenerWegpunkt is null ? GetroffenesTeilstueck(punkt) : null;
        if (_gezogenerWegpunkt is not null || _gezogenesTeilstueck is not null)
        {
            // Die Karte darf sich beim Ziehen eines Wegpunkts oder der Linie nicht mitbewegen
            Karte.Map.Navigator.PanLock = true;
            Karte.CaptureMouse();
            e.Handled = true;
        }
    }

    /// <summary>Maus bewegt: beim Ziehen die gestrichelte Vorschau nachführen.</summary>
    private void MausBewegt(object sender, MouseEventArgs e)
    {
        if (_druckpunkt is not Point druck || (_gezogenerWegpunkt is null && _gezogenesTeilstueck is null))
        {
            return;
        }

        Point punkt = e.GetPosition(Karte);
        if (!_wirdGezogen && Abstand(punkt, druck) <= Klickspielraum)
        {
            return;
        }

        _wirdGezogen = true;
        Geoposition maus = InGeoposition(punkt);
        IReadOnlyList<Geoposition> wegpunkte = _viewModel.Planung.Wegpunkte;
        List<Geoposition> nachbarn = [];
        if (_gezogenerWegpunkt is int nummer)
        {
            if (nummer > 0)
            {
                nachbarn.Add(wegpunkte[nummer - 1]);
            }

            if (nummer < wegpunkte.Count - 1)
            {
                nachbarn.Add(wegpunkte[nummer + 1]);
            }

            List<Geoposition> verschoben = [.. wegpunkte];
            verschoben[nummer] = maus;
            ZeichneWegpunkte(verschoben);
        }
        else if (_gezogenesTeilstueck is int teilstueck)
        {
            nachbarn.Add(wegpunkte[teilstueck]);
            nachbarn.Add(wegpunkte[teilstueck + 1]);
        }

        Ebene(Vorschauebene).Features = [.. nachbarn.Select(n => Linie([n, maus], Luftlinienfarbe, true))];
        Karte.Map.Refresh();
        e.Handled = true;
    }

    /// <summary>Linke Taste losgelassen: Wegpunkt verschieben, Zwischenpunkt einfügen oder Wegpunkt anhängen.</summary>
    private void LinksLosgelassen(object sender, MouseButtonEventArgs e)
    {
        if (_druckpunkt is not Point druck)
        {
            return;
        }

        Point punkt = e.GetPosition(Karte);
        int? wegpunkt = _gezogenerWegpunkt, teilstueck = _gezogenesTeilstueck;
        bool gezogen = _wirdGezogen;
        _druckpunkt = null;
        _gezogenerWegpunkt = _gezogenesTeilstueck = null;
        _wirdGezogen = false;

        if (wegpunkt is not null || teilstueck is not null)
        {
            Karte.Map.Navigator.PanLock = false;
            Karte.ReleaseMouseCapture();
            e.Handled = true;
            if (!gezogen)
            {
                return; // Nur ein Klick auf Wegpunkt oder Linie: nichts ändern
            }

            if (wegpunkt is int nummer)
            {
                _viewModel.Verschieben(nummer, InGeoposition(punkt));
            }
            else if (teilstueck is int stueck)
            {
                _viewModel.Einfuegen(stueck, InGeoposition(punkt));
            }

            return;
        }

        // Klick neben Strecke und Wegpunkte; wurde die Karte dabei verschoben, ist es kein Klick
        if (Abstand(punkt, druck) <= Klickspielraum)
        {
            _viewModel.Anhaengen(InGeoposition(punkt));
        }
    }

    /// <summary>Rechte Taste losgelassen: einen getroffenen Wegpunkt entfernen.</summary>
    private void RechtsLosgelassen(object sender, MouseButtonEventArgs e)
    {
        if (GetroffenerWegpunkt(e.GetPosition(Karte)) is int nummer)
        {
            _viewModel.Entfernen(nummer);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Fragt beim Schließen nach ungespeicherten Änderungen und merkt den Kartenausschnitt.
    /// </summary>
    /// <remarks>
    /// Die Rückfrage zeigt das ViewModel; bei „Nein“ bleibt das Fenster offen.
    /// </remarks>
    private void FensterSchliesst(object? sender, CancelEventArgs e)
    {
        if (!_schliessenBestaetigt && !_viewModel.DarfVerwerfen())
        {
            e.Cancel = true;
            return;
        }

        _schliessenBestaetigt = true;
        MRect ausschnitt = Karte.Map.Navigator.Viewport.ToExtent();
        if (ausschnitt.Width > 0 && ausschnitt.Height > 0)
        {
            _viewModel.MerkeAusschnitt(InGeoposition(ausschnitt.Min), InGeoposition(ausschnitt.Max));
        }
    }

    /// <summary>Nummer des Wegpunkts unter dem Mauszeiger (der oberste, also zuletzt gezeichnete) oder <c>null</c>.</summary>
    private int? GetroffenerWegpunkt(Point punkt)
    {
        IReadOnlyList<Geoposition> wegpunkte = _viewModel.Planung.Wegpunkte;
        for (int i = wegpunkte.Count - 1; i >= 0; i--)
        {
            Mapsui.Manipulations.ScreenPosition bildschirm = Karte.Map.Navigator.Viewport.WorldToScreen(InKartenkoordinaten(wegpunkte[i]));
            if (Abstand(punkt, new Point(bildschirm.X, bildschirm.Y)) <= Wegpunktradius)
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>Nummer des Teilstücks, dessen Linie unter dem Mauszeiger liegt, oder <c>null</c>.</summary>
    private int? GetroffenesTeilstueck(Point punkt)
    {
        Geoposition stelle = InGeoposition(punkt);
        if (FindeEinfuegestelle.Ausfuehren(_viewModel.Planung, stelle) is not (int teilstueck, double abstandMeter))
        {
            return null;
        }

        // Auflösung der Karte in Web-Mercator-Metern je Pixel; auf dem Boden schrumpft sie mit dem Kosinus der Breite
        double meterJePixel = Karte.Map.Navigator.Viewport.Resolution * Math.Cos(stelle.Breite * Math.PI / 180);
        return abstandMeter <= Linienradius * meterJePixel ? teilstueck : null;
    }

    /// <summary>Die Ebene mit diesem Namen.</summary>
    private MemoryLayer Ebene(string name) => (MemoryLayer)Karte.Map.Layers.First(ebene => ebene.Name == name);

    /// <summary>Eine Linie durch die Punkte; gestrichelt für Luftlinien, offene Teilstücke und Vorschau.</summary>
    private static GeometryFeature Linie(List<Geoposition> punkte, Mapsui.Styles.Color farbe, bool gestrichelt)
    {
        Coordinate[] linie = [.. punkte.Select(InKartenkoordinaten).Select(p => new Coordinate(p.X, p.Y))];
        var stift = new Pen(farbe, gestrichelt ? 3 : 4) { PenStyle = gestrichelt ? PenStyle.Dash : PenStyle.Solid };
        return new GeometryFeature(new LineString(linie)) { Styles = [new VectorStyle { Line = stift }] };
    }

    /// <summary>Ein runder Punkt an einer Position.</summary>
    private static PointFeature Zeichen(Geoposition position, Mapsui.Styles.Color fuellung, Mapsui.Styles.Color rand, double groesse) =>
        new(InKartenkoordinaten(position))
        {
            Styles =
            [
                new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = groesse,
                    Fill = new Mapsui.Styles.Brush(fuellung),
                    Outline = new Pen(rand, 2),
                },
            ],
        };

    /// <summary>Rechteck in Kartenkoordinaten um zwei Ecken, mit Rand und einer Mindestgröße von 500 m.</summary>
    private static MRect Rechteck(Geoposition suedWest, Geoposition nordOst, double rand)
    {
        MPoint sw = InKartenkoordinaten(suedWest), no = InKartenkoordinaten(nordOst);
        double breite = Math.Max(no.X - sw.X, 500), hoehe = Math.Max(no.Y - sw.Y, 500);
        MPoint mitte = new((sw.X + no.X) / 2, (sw.Y + no.Y) / 2);
        return new MRect(
            mitte.X - breite * (0.5 + rand), mitte.Y - hoehe * (0.5 + rand),
            mitte.X + breite * (0.5 + rand), mitte.Y + hoehe * (0.5 + rand));
    }

    /// <summary>Die geografische Position zu einem Punkt auf dem Bildschirm.</summary>
    private Geoposition InGeoposition(Point punkt) =>
        InGeoposition(Karte.Map.Navigator.Viewport.ScreenToWorld(punkt.X, punkt.Y));

    /// <summary>Die geografische Position zu einem Punkt in Kartenkoordinaten (Web-Mercator).</summary>
    private static Geoposition InGeoposition(MPoint kartenpunkt)
    {
        (double laenge, double breite) = SphericalMercator.ToLonLat(kartenpunkt.X, kartenpunkt.Y);
        return new Geoposition(breite, laenge);
    }

    /// <summary>Rechnet eine Position in Grad in die Koordinaten der Karte (Web-Mercator) um.</summary>
    private static MPoint InKartenkoordinaten(Geoposition position) =>
        SphericalMercator.FromLonLat(new MPoint(position.Laenge, position.Breite));

    /// <summary>Abstand zweier Bildschirmpunkte in Pixeln.</summary>
    private static double Abstand(Point a, Point b) => (a - b).Length;
}

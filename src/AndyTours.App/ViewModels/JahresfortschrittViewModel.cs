using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel der Karteikarte „Aktuell“: Kennzahlen des laufenden Jahres und Diagramm der Jahressummen.
/// </summary>
/// <remarks>
/// Das Diagramm ist ein OxyPlot-Datenmodell (<see cref="Diagramm"/>, siehe
/// <c>docs/adr/0001-diagrammbibliothek.md</c>); die Ansicht bindet es nur an. Es entsteht einmal und
/// bekommt bei jeder Aktualisierung nur neue Daten, weil es bei jeder Filteränderung neu berechnet wird.
/// <para>
/// Reihen von unten nach oben: Fläche mit Linie und Punkten, gestrichelte Ziellinie je Jahr mit Ziel,
/// Soll-Markierung im laufenden Jahr. Einen Hinweis beim Überfahren gibt es nicht.
/// </para>
/// Der gezeigte Wert bleibt bis zum Programmende erhalten und beginnt mit Kilometern.
/// </remarks>
public partial class JahresfortschrittViewModel : ObservableObject
{
    /// <summary>Hauptfarbe der Linie, Fläche und Punkte.</summary>
    private static readonly OxyColor Linienfarbe = OxyColor.FromRgb(0x2E, 0x6D, 0xB4);

    /// <summary>Farbe der Ziellinie und der Soll-Markierung.</summary>
    private static readonly OxyColor Zielfarbe = OxyColor.FromRgb(0xC0, 0x39, 0x2B);

    /// <summary>Halbe Breite der Ziellinie über einem Jahr (in Jahren).</summary>
    private const double HalbeZielbreite = 0.35;

    /// <summary>Senkrechte Achse; ihre Beschriftung nennt die Einheit des Werts.</summary>
    private readonly LinearAxis _wertachse;

    /// <summary>Fläche unter der Linie, mit der Linie selbst und einem Punkt je Jahr.</summary>
    private readonly AreaSeries _flaeche;

    /// <summary>Gestrichelte Ziellinie, je Jahr mit Ziel ein kurzes Stück.</summary>
    private readonly LineSeries _ziellinie;

    /// <summary>Soll-Stand zum heutigen Tag im laufenden Jahr.</summary>
    private readonly ScatterSeries _soll;

    /// <summary>Zuletzt übergebene Auswahl; <c>null</c>, solange noch nicht aktualisiert wurde.</summary>
    private (Datenbestand Bestand, long? AktivitaetId, string? Kategorie, DateOnly Heute)? _auswahl;

    /// <summary>Der gezeigte Wert; ein Klick ins Diagramm schaltet weiter.</summary>
    [ObservableProperty]
    private Fortschrittswert _wert = Fortschrittswert.Kilometer;

    /// <summary>Überschrift des Diagramms, z. B. „Kilometer (Summe Entfernung)“.</summary>
    [ObservableProperty]
    private string _ueberschrift = BeschreibeJahresfortschritt.Ueberschrift(Fortschrittswert.Kilometer);

    /// <summary>Erste Kennzahlzeile: Stand, Ziel, Prozent, Soll-Stand, Vorsprung bzw. Rückstand.</summary>
    [ObservableProperty]
    private string _stand = string.Empty;

    /// <summary>Zweite Kennzahlzeile: Wochenleistung, „Ziel erreicht“ oder Hinweis auf fehlendes Ziel.</summary>
    [ObservableProperty]
    private string _ausblick = string.Empty;

    /// <summary><c>true</c>, wenn die Auswahl Einheiten enthält; sonst erscheint statt des Diagramms ein Hinweis.</summary>
    [ObservableProperty]
    private bool _hatEinheiten;

    /// <summary>
    /// Erzeugt das ViewModel mit leerem Diagramm.
    /// </summary>
    public JahresfortschrittViewModel()
    {
        Diagramm = new PlotModel { Culture = CultureInfo.GetCultureInfo("de-DE"), PlotAreaBorderColor = OxyColors.LightGray };

        // Waagrecht die Jahre (ganzzahlig, ohne Tausenderpunkt), senkrecht die Summe ab 0; kein Zoomen oder Verschieben
        Diagramm.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = 1,
            MinorStep = 1,
            StringFormat = "0",
            MinimumPadding = 0.05,
            MaximumPadding = 0.05,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        });
        _wertachse = new LinearAxis
        {
            Position = AxisPosition.Left,
            Minimum = 0,
            MaximumPadding = 0.1,
            StringFormat = "#,0",
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColors.LightGray,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        };
        Diagramm.Axes.Add(_wertachse);

        _flaeche = new AreaSeries
        {
            ConstantY2 = 0,
            Color = Linienfarbe,
            Fill = OxyColor.FromAColor(70, Linienfarbe),
            StrokeThickness = 2,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerFill = Linienfarbe,
            MarkerStroke = OxyColors.White,
            MarkerStrokeThickness = 1.5,
        };
        _ziellinie = new LineSeries
        {
            Color = Zielfarbe,
            LineStyle = LineStyle.Dash,
            StrokeThickness = 2,
        };
        _soll = new ScatterSeries
        {
            MarkerType = MarkerType.Diamond,
            MarkerSize = 7,
            MarkerFill = Zielfarbe,
        };
        Diagramm.Series.Add(_flaeche);
        Diagramm.Series.Add(_ziellinie);
        Diagramm.Series.Add(_soll);
    }

    /// <summary>Das Diagramm; die Ansicht bindet es an ihre <c>PlotView</c>.</summary>
    public PlotModel Diagramm { get; }

    /// <summary>
    /// Berechnet Kennzahlen und Diagramm für eine neue Auswahl.
    /// </summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Aktivität der Werkzeugleiste; <c>null</c> = alle Aktivitäten.</param>
    /// <param name="kategorie">Kategorie der Filterleiste; <c>null</c> = alle.</param>
    /// <param name="heute">Das heutige Datum.</param>
    public void Aktualisiere(Datenbestand bestand, long? aktivitaetId, string? kategorie, DateOnly heute)
    {
        _auswahl = (bestand, aktivitaetId, kategorie, heute);
        Zeichne();
    }

    /// <summary>Klick ins Diagramm: Kilometer → Anzahl → Höhenmeter → Fahrzeit → Kilometer.</summary>
    [RelayCommand]
    private void WertWechseln()
    {
        Wert = (Fortschrittswert)(((int)Wert + 1) % Enum.GetValues<Fortschrittswert>().Length);
        Zeichne();
    }

    /// <summary>Ermittelt den Jahresfortschritt der gemerkten Auswahl und überträgt ihn in Texte und Diagramm.</summary>
    private void Zeichne()
    {
        Ueberschrift = BeschreibeJahresfortschritt.Ueberschrift(Wert);
        if (_auswahl is not { } auswahl)
        {
            return;
        }

        Jahresfortschritt fortschritt = ErmittleJahresfortschritt.Ausfuehren(auswahl.Bestand, auswahl.AktivitaetId, auswahl.Kategorie, Wert, auswahl.Heute);
        (Stand, Ausblick) = BeschreibeJahresfortschritt.Kennzahlen(fortschritt);
        HatEinheiten = fortschritt.Punkte.Count > 0;

        // Linie, Fläche und Punkte; die senkrechte Achse nennt die Einheit
        _wertachse.Title = Wert switch
        {
            Fortschrittswert.Kilometer => "km",
            Fortschrittswert.Anzahl => "Einheiten",
            Fortschrittswert.Hoehenmeter => "hm",
            _ => "Stunden",
        };
        _flaeche.Points.Clear();
        _flaeche.Points.AddRange(fortschritt.Punkte.Select(p => new DataPoint(p.Jahr, p.Summe)));

        // Ziellinie: je Jahr mit Ziel ein waagrechtes Stück, getrennt durch Lücken (ungültige Punkte)
        _ziellinie.Points.Clear();
        foreach (Jahrespunkt punkt in fortschritt.Punkte.Where(p => p.Ziel is not null))
        {
            _ziellinie.Points.Add(new DataPoint(punkt.Jahr - HalbeZielbreite, punkt.Ziel!.Value));
            _ziellinie.Points.Add(new DataPoint(punkt.Jahr + HalbeZielbreite, punkt.Ziel!.Value));
            _ziellinie.Points.Add(DataPoint.Undefined);
        }

        // Soll-Stand zum heutigen Tag im laufenden Jahr
        _soll.Points.Clear();
        if (fortschritt.Kennzahlen.Soll is double soll)
        {
            _soll.Points.Add(new ScatterPoint(fortschritt.Kennzahlen.Jahr, soll));
        }

        Diagramm.InvalidatePlot(true);
    }
}

using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Tourdetail“ mit der Karteikarte „Höhenprofil“.
/// </summary>
/// <remarks>
/// Das Diagramm ist ein OxyPlot-Datenmodell (<see cref="Diagramm"/>, siehe <c>docs/adr/0001-diagrammbibliothek.md</c>);
/// die Ansicht bindet es nur an. Es wird bei jeder Änderung von Achse oder Häkchen neu aufgebaut:
/// <list type="bullet">
/// <item>waagrecht die Distanz in km oder die Fahrzeit ohne Pausen,</item>
/// <item>links die Höhe in m als Fläche,</item>
/// <item>rechts je eingeblendetem Zusatzwert (Puls, Geschwindigkeit, Trittfrequenz, Temperatur) eine eigene Skala
/// mit einer Linie.</item>
/// </list>
/// Die Maussteuerung (<see cref="Steuerung"/>): Überfahren zeigt die Werte an dieser Stelle, mit der linken Taste
/// einen Bereich aufziehen vergrößert ihn, ein Doppelklick zeigt wieder die ganze Tour. Die Anzeige ist der Stand
/// beim Öffnen; spätere Änderungen an der Einheit wirken sich nicht aus.
/// </remarks>
public partial class TourdetailViewModel : ObservableObject
{
    /// <summary>Hinweis, wenn die Aufzeichnung keine Höhendaten enthält.</summary>
    public const string KeineHoehendaten = "Die Aufzeichnung enthält keine Höhendaten.";

    /// <summary>Deutsche Kultur für Zahlen im Diagramm und im Hinweis beim Überfahren.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Farbe der Höhenfläche.</summary>
    private static readonly OxyColor Hoehenfarbe = OxyColor.FromRgb(0x2E, 0x6D, 0xB4);

    /// <summary>Farbe, Achsentitel und Einheit je Zusatzwert.</summary>
    private static readonly Dictionary<Verlaufswert, (OxyColor Farbe, string Titel, string Einheit)> Darstellung = new()
    {
        [Verlaufswert.Puls] = (OxyColor.FromRgb(0xC0, 0x39, 0x2B), "Puls", "bpm"),
        [Verlaufswert.Geschwindigkeit] = (OxyColor.FromRgb(0x27, 0xAE, 0x60), "Geschwindigkeit", "km/h"),
        [Verlaufswert.Trittfrequenz] = (OxyColor.FromRgb(0x8E, 0x44, 0xAD), "Trittfrequenz", "U/min"),
        [Verlaufswert.Temperatur] = (OxyColor.FromRgb(0xE6, 0x7E, 0x22), "Temperatur", "°C"),
    };

    /// <summary>Die Messpunkte der Tour.</summary>
    private readonly IReadOnlyList<Verlaufspunkt> _verlauf;

    /// <summary>
    /// Ein Punkt einer Diagrammlinie mit dem fertigen Hinweistext für das Überfahren.
    /// </summary>
    /// <remarks>
    /// Alle Linien tragen an derselben Stelle denselben Text; so zeigt der Hinweis immer Distanz, Fahrzeit, Höhe
    /// und alle eingeblendeten Werte, egal an welcher Linie die Maus gerade steht.
    /// </remarks>
    /// <param name="X">Waagrechter Wert (km oder Fahrzeit).</param>
    /// <param name="Y">Senkrechter Wert der Linie.</param>
    /// <param name="Hinweis">Text beim Überfahren.</param>
    private sealed record Diagrammpunkt(double X, double Y, string Hinweis);

    /// <summary>
    /// Erzeugt das ViewModel und baut das Diagramm auf.
    /// </summary>
    /// <param name="einheit">Die Einheit (für die Kopfzeile).</param>
    /// <param name="verlauf">Die gelesenen Messpunkte ihrer Aufzeichnung.</param>
    public TourdetailViewModel(Trainingseinheit einheit, IReadOnlyList<Verlaufspunkt> verlauf)
    {
        _verlauf = verlauf;
        Kopfzeile = BeschreibeTourdetail.Ausfuehren(einheit);
        (HatHoehe, IReadOnlyList<Verlaufswert> vorhanden) = ErmittleVerlaufswerte.Ausfuehren(verlauf);
        KannPuls = vorhanden.Contains(Verlaufswert.Puls);
        KannGeschwindigkeit = vorhanden.Contains(Verlaufswert.Geschwindigkeit);
        KannTrittfrequenz = vorhanden.Contains(Verlaufswert.Trittfrequenz);
        KannTemperatur = vorhanden.Contains(Verlaufswert.Temperatur);

        Diagramm = new PlotModel { Culture = Deutsch, PlotAreaBorderColor = OxyColors.LightGray };
        Steuerung = ErstelleSteuerung();
        BaueDiagramm();
    }

    /// <summary>Datum, Name, Distanz, Höhenmeter und höchster Punkt der Einheit.</summary>
    public string Kopfzeile { get; }

    /// <summary><c>true</c>, wenn die Aufzeichnung Höhendaten enthält; sonst erscheint statt des Diagramms ein Hinweis.</summary>
    public bool HatHoehe { get; }

    /// <summary><c>true</c>, wenn statt des Diagramms der Hinweis <see cref="KeineHoehendaten"/> erscheint.</summary>
    public bool OhneHoehe => !HatHoehe;

    /// <summary>Das Diagramm (Datenmodell für die Ansicht).</summary>
    public PlotModel Diagramm { get; }

    /// <summary>Die Maussteuerung des Diagramms.</summary>
    public PlotController Steuerung { get; }

    /// <summary>Waagrechte Achse zeigt die Distanz (Gegenstück zu <see cref="NachFahrzeit"/>).</summary>
    public bool NachDistanz
    {
        get => !NachFahrzeit;
        set => NachFahrzeit = !value;
    }

    /// <summary>Waagrechte Achse zeigt die Fahrzeit ohne Pausen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NachDistanz))]
    private bool _nachFahrzeit;

    /// <summary>Puls als Linie einblenden.</summary>
    [ObservableProperty]
    private bool _zeigePuls;

    /// <summary>Geschwindigkeit als Linie einblenden.</summary>
    [ObservableProperty]
    private bool _zeigeGeschwindigkeit;

    /// <summary>Trittfrequenz als Linie einblenden.</summary>
    [ObservableProperty]
    private bool _zeigeTrittfrequenz;

    /// <summary>Temperatur als Linie einblenden.</summary>
    [ObservableProperty]
    private bool _zeigeTemperatur;

    /// <summary><c>true</c>, wenn die Aufzeichnung Puls enthält (Häkchen wählbar).</summary>
    public bool KannPuls { get; }

    /// <summary><c>true</c>, wenn die Aufzeichnung Geschwindigkeit enthält (Häkchen wählbar).</summary>
    public bool KannGeschwindigkeit { get; }

    /// <summary><c>true</c>, wenn die Aufzeichnung Trittfrequenz enthält (Häkchen wählbar).</summary>
    public bool KannTrittfrequenz { get; }

    /// <summary><c>true</c>, wenn die Aufzeichnung Temperatur enthält (Häkchen wählbar).</summary>
    public bool KannTemperatur { get; }

    /// <summary>Baut das Diagramm neu auf, wenn sich Achse oder ein Häkchen ändert.</summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(NachFahrzeit) or nameof(ZeigePuls) or nameof(ZeigeGeschwindigkeit)
            or nameof(ZeigeTrittfrequenz) or nameof(ZeigeTemperatur))
        {
            BaueDiagramm();
        }
    }

    /// <summary>Die eingeblendeten Zusatzwerte in fester Reihenfolge (nur vorhandene).</summary>
    private List<Verlaufswert> EingeblendeteWerte()
    {
        List<Verlaufswert> werte = [];
        if (ZeigePuls && KannPuls)
        {
            werte.Add(Verlaufswert.Puls);
        }

        if (ZeigeGeschwindigkeit && KannGeschwindigkeit)
        {
            werte.Add(Verlaufswert.Geschwindigkeit);
        }

        if (ZeigeTrittfrequenz && KannTrittfrequenz)
        {
            werte.Add(Verlaufswert.Trittfrequenz);
        }

        if (ZeigeTemperatur && KannTemperatur)
        {
            werte.Add(Verlaufswert.Temperatur);
        }

        return werte;
    }

    /// <summary>
    /// Baut Achsen und Linien nach Achsenwahl und Häkchen neu auf.
    /// </summary>
    private void BaueDiagramm()
    {
        Diagramm.Axes.Clear();
        Diagramm.Series.Clear();
        if (!HatHoehe)
        {
            Diagramm.InvalidatePlot(true);
            return;
        }

        // Waagrecht: Distanz oder Fahrzeit; nur diese Achse und die Höhe lassen sich vergrößern
        Diagramm.Axes.Add(NachFahrzeit
            ? new TimeSpanAxis { Position = AxisPosition.Bottom, Title = "Fahrzeit", StringFormat = "h:mm" }
            : new LinearAxis { Position = AxisPosition.Bottom, Title = "km", StringFormat = "#,0.#" });
        Diagramm.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Key = "Hoehe",
            Title = "m",
            StringFormat = "#,0",
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColors.LightGray,
        });

        List<Verlaufswert> eingeblendet = EingeblendeteWerte();
        List<string> hinweise = _verlauf.Select(p => Hinweis(p, eingeblendet)).ToList();

        // Höhe als Fläche (Punkte ohne Höhe fehlen); sie reicht bis knapp unter den tiefsten Punkt, nicht bis 0 m
        double tiefsterPunkt = _verlauf.Where(p => p.Hoehe is not null).Min(p => p.Hoehe!.Value);
        Diagramm.Series.Add(new AreaSeries
        {
            YAxisKey = "Hoehe",
            Color = Hoehenfarbe,
            Fill = OxyColor.FromAColor(70, Hoehenfarbe),
            StrokeThickness = 1.5,
            ConstantY2 = Math.Floor(tiefsterPunkt / 50) * 50,
            ItemsSource = Punkte(p => p.Hoehe, hinweise),
            DataFieldX = nameof(Diagrammpunkt.X),
            DataFieldY = nameof(Diagrammpunkt.Y),
            TrackerFormatString = "{" + nameof(Diagrammpunkt.Hinweis) + "}",
        });

        // Je Zusatzwert eine eigene Skala rechts und eine Linie
        for (int i = 0; i < eingeblendet.Count; i++)
        {
            Verlaufswert wert = eingeblendet[i];
            (OxyColor farbe, string titel, string einheit) = Darstellung[wert];
            Diagramm.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                PositionTier = i,
                Key = wert.ToString(),
                Title = $"{titel} ({einheit})",
                TitleColor = farbe,
                TextColor = farbe,
                IsZoomEnabled = false,
                IsPanEnabled = false,
            });
            Diagramm.Series.Add(new LineSeries
            {
                YAxisKey = wert.ToString(),
                Color = farbe,
                StrokeThickness = 1,
                ItemsSource = Punkte(p => p.Wert(wert), hinweise),
                DataFieldX = nameof(Diagrammpunkt.X),
                DataFieldY = nameof(Diagrammpunkt.Y),
                TrackerFormatString = "{" + nameof(Diagrammpunkt.Hinweis) + "}",
            });
        }

        Diagramm.InvalidatePlot(true);
    }

    /// <summary>
    /// Liefert die Punkte einer Linie; Messpunkte ohne den Wert fehlen.
    /// </summary>
    /// <param name="wert">Liest den senkrechten Wert eines Messpunkts.</param>
    /// <param name="hinweise">Der Hinweistext je Messpunkt (gleiche Reihenfolge wie der Verlauf).</param>
    private List<Diagrammpunkt> Punkte(Func<Verlaufspunkt, double?> wert, List<string> hinweise)
    {
        List<Diagrammpunkt> punkte = [];
        for (int i = 0; i < _verlauf.Count; i++)
        {
            if (wert(_verlauf[i]) is double y)
            {
                punkte.Add(new Diagrammpunkt(X(_verlauf[i]), y, hinweise[i]));
            }
        }

        return punkte;
    }

    /// <summary>Waagrechter Wert eines Messpunkts je nach Achsenwahl.</summary>
    private double X(Verlaufspunkt punkt) => NachFahrzeit ? TimeSpanAxis.ToDouble(punkt.Fahrzeit) : punkt.DistanzKm;

    /// <summary>
    /// Text beim Überfahren: Distanz, Fahrzeit, Höhe und die eingeblendeten Zusatzwerte, je eine Zeile.
    /// </summary>
    private static string Hinweis(Verlaufspunkt punkt, List<Verlaufswert> eingeblendet)
    {
        List<string> zeilen =
        [
            string.Format(Deutsch, "{0:N2} km", punkt.DistanzKm),
            "Fahrzeit " + FormatiereDauer.Ausfuehren(punkt.Fahrzeit),
            punkt.Hoehe is double hoehe ? string.Format(Deutsch, "Höhe {0:N0} m", hoehe) : "Höhe –",
        ];
        foreach (Verlaufswert wert in eingeblendet)
        {
            (_, string titel, string einheit) = Darstellung[wert];
            zeilen.Add(punkt.Wert(wert) is double zahl
                ? string.Format(Deutsch, "{0} {1:#,0.#} {2}", titel, zahl, einheit)
                : $"{titel} –");
        }

        return string.Join("\n", zeilen);
    }

    /// <summary>
    /// Erstellt die Maussteuerung: Überfahren zeigt die Werte, linke Taste ziehen vergrößert einen Bereich,
    /// Doppelklick zeigt wieder alles. Mausrad und Verschieben sind abgeschaltet, damit die Bedienung eindeutig bleibt.
    /// </summary>
    private static PlotController ErstelleSteuerung()
    {
        var steuerung = new PlotController();
        steuerung.UnbindAll();
        steuerung.BindMouseEnter(PlotCommands.HoverSnapTrack);
        steuerung.BindMouseDown(OxyMouseButton.Left, PlotCommands.ZoomRectangle);
        steuerung.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, 2, new DelegatePlotCommand<OxyMouseDownEventArgs>((ansicht, _, _) =>
        {
            ansicht.ActualModel.ResetAllAxes();
            ansicht.InvalidatePlot(false);
        }));
        return steuerung;
    }
}

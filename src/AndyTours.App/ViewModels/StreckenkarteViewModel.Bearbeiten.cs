using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Streckenkarten-ViewModels: die Leiste „Track bearbeiten“ (kürzen, umkehren, glätten).
/// </summary>
/// <remarks>
/// Nur beim Tourdetail einer einzelnen Einheit mit GPS-Daten. Grundlage sind die Messpunkte der Aufzeichnung mit
/// Position; die Regler „Start“ und „Ziel“ wählen je einen davon. Jede Änderung berechnet die Strecke mit
/// <see cref="BearbeiteTrack"/> neu, ersetzt <see cref="Strecken"/> samt Rechteck und meldet
/// <see cref="StreckeGeaendert"/>, damit das Fenster die Karte neu zeichnet. Die Bearbeitung lebt nur in diesem
/// ViewModel: Einheit, Aufzeichnung und Höhenprofil bleiben unberührt, und beim nächsten Öffnen ist sie verworfen.
/// </remarks>
public partial class StreckenkarteViewModel
{
    /// <summary>Die Messpunkte der Aufzeichnung mit Position (leer, wenn nicht bearbeitet werden kann).</summary>
    private List<Verlaufspunkt> _punkte = [];

    /// <summary>Die Einheit der bearbeiteten Strecke (für den Export).</summary>
    private Trainingseinheit? _einheit;

    /// <summary>Die bearbeiteten Messpunkte, wie sie die Karte zeigt.</summary>
    private List<Verlaufspunkt> _bearbeitet = [];

    private int _startpunkt;
    private int _zielpunkt;
    private bool _umkehren;
    private Glaettung _glaettung = Glaettung.Aus;

    /// <summary>
    /// Wird ausgelöst, wenn sich die bearbeitete Strecke geändert hat; das Fenster zeichnet dann Linie und Zeichen neu.
    /// </summary>
    public event EventHandler? StreckeGeaendert;

    /// <summary><c>true</c>, wenn die Leiste „Track bearbeiten“ erscheint (eine Einheit, mindestens zwei Punkte mit Position).</summary>
    public bool KannBearbeiten { get; private set; }

    /// <summary>Nummer des letzten Punkts (Höchstwert der Regler).</summary>
    public int LetzterPunkt => Math.Max(_punkte.Count - 1, 0);

    /// <summary>
    /// Regler „Start“: Nummer des Startpunkts; höchstens ein Punkt vor „Ziel“, damit zwei Punkte bleiben.
    /// </summary>
    public int Startpunkt
    {
        get => _startpunkt;
        set => SetzeGrenze(ref _startpunkt, Math.Clamp(value, 0, _zielpunkt - 1), nameof(Startpunkt));
    }

    /// <summary>Regler „Ziel“: Nummer des Zielpunkts; mindestens ein Punkt nach „Start“.</summary>
    public int Zielpunkt
    {
        get => _zielpunkt;
        set => SetzeGrenze(ref _zielpunkt, Math.Clamp(value, _startpunkt + 1, LetzterPunkt), nameof(Zielpunkt));
    }

    /// <summary>Stelle von „Start“ in km ab dem Start der Aufzeichnung.</summary>
    public double StartKm => _punkte.Count > 0 ? _punkte[_startpunkt].DistanzKm : 0;

    /// <summary>Stelle von „Ziel“ in km ab dem Start der Aufzeichnung.</summary>
    public double ZielKm => _punkte.Count > 0 ? _punkte[_zielpunkt].DistanzKm : 0;

    /// <summary>
    /// Länge der bearbeiteten Strecke: Distanz laut Aufzeichnung zwischen Start- und Zielpunkt. Glätten und
    /// Umkehren ändern sie nicht; ungekürzt entspricht sie der Distanz der Aufzeichnung.
    /// </summary>
    public double LaengeKm => ZielKm - StartKm;

    /// <summary>„Umkehren“: Die Strecke läuft vom Ziel zum Start.</summary>
    public bool Umkehren
    {
        get => _umkehren;
        set
        {
            if (SetProperty(ref _umkehren, value))
            {
                BerechneBearbeitung();
            }
        }
    }

    /// <summary>Gewählte Stufe von „Glätten“.</summary>
    public Glaettung Glaettung
    {
        get => _glaettung;
        set
        {
            if (SetProperty(ref _glaettung, value))
            {
                OnPropertyChanged(nameof(IstGlaettungAus));
                OnPropertyChanged(nameof(IstGlaettungSchwach));
                OnPropertyChanged(nameof(IstGlaettungStark));
                BerechneBearbeitung();
            }
        }
    }

    /// <summary>Umschalter „aus“ von „Glätten“.</summary>
    public bool IstGlaettungAus
    {
        get => Glaettung == Glaettung.Aus;
        set
        {
            if (value)
            {
                Glaettung = Glaettung.Aus;
            }
        }
    }

    /// <summary>Umschalter „schwach“ von „Glätten“.</summary>
    public bool IstGlaettungSchwach
    {
        get => Glaettung == Glaettung.Schwach;
        set
        {
            if (value)
            {
                Glaettung = Glaettung.Schwach;
            }
        }
    }

    /// <summary>Umschalter „stark“ von „Glätten“.</summary>
    public bool IstGlaettungStark
    {
        get => Glaettung == Glaettung.Stark;
        set
        {
            if (value)
            {
                Glaettung = Glaettung.Stark;
            }
        }
    }

    /// <summary><c>true</c>, wenn die Strecke vom Ausgangszustand abweicht (gekürzt, umgekehrt oder geglättet).</summary>
    public bool IstBearbeitet =>
        KannBearbeiten && (_startpunkt != 0 || _zielpunkt != LetzterPunkt || _umkehren || _glaettung != Glaettung.Aus);

    /// <summary>
    /// Befehl „Zurücksetzen“: Start und Ziel an Anfang und Ende, „Umkehren“ aus, „Glätten“ aus.
    /// </summary>
    [RelayCommand]
    private void Zuruecksetzen()
    {
        _startpunkt = 0;
        _zielpunkt = LetzterPunkt;
        _umkehren = false;
        _glaettung = Glaettung.Aus;
        OnPropertyChanged(nameof(Startpunkt));
        OnPropertyChanged(nameof(Zielpunkt));
        OnPropertyChanged(nameof(Umkehren));
        OnPropertyChanged(nameof(Glaettung));
        OnPropertyChanged(nameof(IstGlaettungAus));
        OnPropertyChanged(nameof(IstGlaettungSchwach));
        OnPropertyChanged(nameof(IstGlaettungStark));
        BerechneBearbeitung();
    }

    /// <summary>
    /// Merkt sich beim Erzeugen die Punkte der einzigen Etappe; bei mehreren Etappen oder ohne GPS-Daten bleibt die
    /// Leiste aus.
    /// </summary>
    /// <param name="etappen">Die Etappen mit Verlauf.</param>
    /// <param name="mehrere"><c>true</c> bei einer Mehrtagestour.</param>
    private void BereiteBearbeitungVor(IReadOnlyList<Etappe> etappen, bool mehrere)
    {
        if (mehrere || etappen.Count != 1)
        {
            return;
        }

        _punkte = (etappen[0].Verlauf ?? []).Where(p => p.Position is not null).ToList();
        _einheit = etappen[0].Einheit;
        _bearbeitet = _punkte;
        _zielpunkt = LetzterPunkt;
        KannBearbeiten = _punkte.Count >= 2;
    }

    /// <summary>
    /// Etappen für den GPX-Export: bei einer bearbeiteten Strecke eine Etappe mit den bearbeiteten Punkten, sonst
    /// alle Etappen des Tourdetails.
    /// </summary>
    private IReadOnlyList<Etappe> ExportEtappen() =>
        IstBearbeitet && _einheit is not null ? [new Etappe(_einheit, _bearbeitet)] : _alleEtappen;

    /// <summary>
    /// Setzt einen Regler auf den begrenzten Wert und berechnet neu. Die Anzeige wird immer benachrichtigt, damit
    /// ein über die Grenze gezogener Regler wieder den gültigen Wert zeigt.
    /// </summary>
    /// <param name="feld">Das Feld des Reglers.</param>
    /// <param name="wert">Der schon begrenzte Wert.</param>
    /// <param name="name">Name der Eigenschaft des Reglers.</param>
    private void SetzeGrenze(ref int feld, int wert, string name)
    {
        bool geaendert = feld != wert;
        feld = wert;
        OnPropertyChanged(name);
        if (geaendert)
        {
            BerechneBearbeitung();
        }
    }

    /// <summary>Berechnet die bearbeitete Strecke neu und meldet sie der Karte.</summary>
    private void BerechneBearbeitung()
    {
        if (!KannBearbeiten)
        {
            return;
        }

        _bearbeitet = BearbeiteTrack.Ausfuehren(_punkte, _startpunkt, _zielpunkt, _umkehren, _glaettung);
        Strecke strecke = ErmittleStrecke.Ausfuehren(_bearbeitet)!;
        Strecken = [strecke];
        (SuedWest, NordOst) = UmschliesseStrecken.Ausfuehren(Strecken);

        OnPropertyChanged(nameof(StartKm));
        OnPropertyChanged(nameof(ZielKm));
        OnPropertyChanged(nameof(LaengeKm));
        StreckeGeaendert?.Invoke(this, EventArgs.Empty);
    }
}

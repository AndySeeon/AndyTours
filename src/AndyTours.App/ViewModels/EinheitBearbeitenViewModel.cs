using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Bearbeitungsfensters einer Trainingseinheit (Neu, Bearbeiten, Kopieren).
/// </summary>
/// <remarks>
/// Hält alle Felder als Text bzw. gewählten Listeneintrag. Jede Änderung prüft die Eingaben über
/// <see cref="EinheitEingaben.AlsEinheit"/>: Fehlerhafte Felder meldet der Indexer (<c>this[feld]</c>,
/// in XAML <c>{Binding [Distanz]}</c>), und „Speichern“ ist nur ohne Fehler wählbar.
/// <para>
/// Gespeichert wird nicht hier, sondern über die beim Erzeugen übergebene Funktion des Hauptfensters.
/// Gelingt es, meldet das ViewModel <see cref="SchliessenAngefordert"/>; sonst bleibt das Fenster mit den
/// Eingaben offen.
/// </para>
/// </remarks>
public partial class EinheitBearbeitenViewModel : ObservableObject
{
    /// <summary>Die bearbeitete Einheit (liefert Nummer, Sportler und Anhänge).</summary>
    private readonly Trainingseinheit _basis;

    /// <summary>Speichert die fertige Einheit; liefert <c>true</c> bei Erfolg.</summary>
    private readonly Func<Trainingseinheit, bool> _speichern;

    /// <summary>Die Eingaben beim Öffnen; daran wird erkannt, ob etwas geändert wurde.</summary>
    private readonly Einheiteneingaben _anfang;

    /// <summary>Die zurzeit fehlerhaften Felder.</summary>
    private IReadOnlySet<Eingabefeld> _fehler = new HashSet<Eingabefeld>();

    /// <summary>
    /// Erzeugt das ViewModel und füllt alle Felder aus der Einheit.
    /// </summary>
    /// <param name="einheit">Die zu bearbeitende Einheit (bei „Neu“ und „Kopieren“ die vorbereitete neue).</param>
    /// <param name="listen">Einträge der Auswahllisten Aktivität, Equipment und Trainingsbereich.</param>
    /// <param name="kategorien">Alle Kategorien des Datenbestands.</param>
    /// <param name="titel">Fenstertitel, z. B. „Einheit bearbeiten“.</param>
    /// <param name="speichern">Speichert die Einheit; <c>true</c> bei Erfolg.</param>
    public EinheitBearbeitenViewModel(
        Trainingseinheit einheit,
        Eingabelisten listen,
        IEnumerable<Kategorie> kategorien,
        string titel,
        Func<Trainingseinheit, bool> speichern)
    {
        _basis = einheit;
        _speichern = speichern;
        Titel = titel;
        Aktivitaeten = listen.Aktivitaeten;
        Equipments = listen.Equipments;
        Trainingsbereiche = listen.Trainingsbereiche;

        Einheiteneingaben e = EinheitEingaben.AlsEingaben(einheit);
        _datum = e.Datum;
        _startzeit = e.Startzeit;
        _name = e.Name;
        _aktivitaet = Aktivitaeten.FirstOrDefault(a => a.Id == e.AktivitaetId);
        _equipment = Equipments.FirstOrDefault(q => q.Id == e.EquipmentId) ?? (Equipments.Count > 0 ? Equipments[0] : null);
        _distanz = e.Distanz;
        _hoehenmeter = e.Hoehenmeter;
        _fahrzeit = e.Fahrzeit;
        _maxKmh = e.MaxKmh;
        _pulsDurchschnitt = e.PulsDurchschnitt;
        _pulsMax = e.PulsMax;
        _pulsMin = e.PulsMin;
        _trittfrequenz = e.Trittfrequenz;
        _temperatur = e.Temperatur;
        _leistung = e.Leistung;
        _koerpergewicht = e.Koerpergewicht;
        _erholungspuls = e.Erholungspuls;
        _startHoehe = e.StartHoehe;
        _maxHoehe = e.MaxHoehe;
        _trainingsbereich = e.Trainingsbereich;
        _kommentar = e.Kommentar;

        // Kategorien als Liste mit Häkchen, alphabetisch; jedes Häkchen löst die Prüfung aus
        Kategorien = kategorien
            .OrderBy(k => k.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(k => new Kategorieauswahl(k.Id, k.Name, e.KategorieIds.Contains(k.Id)))
            .ToList();
        foreach (Kategorieauswahl kategorie in Kategorien)
        {
            kategorie.PropertyChanged += (_, _) => Pruefen();
        }

        _anfang = AktuelleEingaben();
        Pruefen();
    }

    /// <summary>
    /// Wird ausgelöst, wenn das Fenster nach erfolgreichem Speichern schließen soll. („Abbrechen“ schließt
    /// das Fenster selbst; die Rückfrage bei Änderungen stellt das Fenster.)
    /// </summary>
    public event EventHandler? SchliessenAngefordert;

    /// <summary>Fenstertitel.</summary>
    public string Titel { get; }

    /// <summary>Einträge der Auswahlliste „Aktivität“.</summary>
    public IReadOnlyList<Auswahleintrag> Aktivitaeten { get; }

    /// <summary>Einträge der Auswahlliste „Equipment“ (beginnend mit „(kein)“).</summary>
    public IReadOnlyList<Auswahleintrag> Equipments { get; }

    /// <summary>Einträge der Auswahlliste „Trainingsbereich“ (beginnend mit dem leeren Eintrag).</summary>
    public IReadOnlyList<string> Trainingsbereiche { get; }

    /// <summary>Alle Kategorien mit Häkchen.</summary>
    public IReadOnlyList<Kategorieauswahl> Kategorien { get; }

    /// <summary><c>true</c>, nachdem erfolgreich gespeichert wurde (das Schließen fragt dann nicht nach).</summary>
    public bool IstGespeichert { get; private set; }

    /// <summary><c>true</c>, wenn die Eingaben vom Stand beim Öffnen abweichen.</summary>
    public bool IstGeaendert => AktuelleEingaben() != _anfang;

    /// <summary><c>true</c>, wenn das Feld rot markiert werden muss (in XAML: <c>{Binding [Distanz]}</c>).</summary>
    /// <param name="feld">Das Feld.</param>
    public bool this[Eingabefeld feld] => _fehler.Contains(feld);

    /// <summary>Ø km/h aus Distanz und Fahrzeit, nur zur Ansicht; leer, solange eines davon fehlt oder ungültig ist.</summary>
    public string DurchschnittKmhText
    {
        get
        {
            if (Eingabetext.LeseZahl(Distanz, out double? km) && km is double distanz
                && Eingabetext.LeseDauer(Fahrzeit, out TimeSpan? dauer) && dauer is TimeSpan zeit
                && BerechneDurchschnittsgeschwindigkeit.Ausfuehren(distanz, zeit) is double kmh)
            {
                return kmh.ToString("N1", CultureInfo.GetCultureInfo("de-DE"));
            }

            return string.Empty;
        }
    }

    /// <summary>Datum, z. B. „24.10.2026“ (Pflicht).</summary>
    [ObservableProperty]
    private string _datum;

    /// <summary>Startzeit, z. B. „9:30“.</summary>
    [ObservableProperty]
    private string _startzeit;

    /// <summary>Name der Tour.</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>Gewählte Aktivität (Pflicht).</summary>
    [ObservableProperty]
    private Auswahleintrag? _aktivitaet;

    /// <summary>Gewähltes Equipment; „(kein)“ = keins.</summary>
    [ObservableProperty]
    private Auswahleintrag? _equipment;

    /// <summary>Distanz in km.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurchschnittKmhText))]
    private string _distanz;

    /// <summary>Höhenmeter.</summary>
    [ObservableProperty]
    private string _hoehenmeter;

    /// <summary>Fahrzeit „h:mm“ oder „h:mm:ss“ (Sekunden freiwillig).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurchschnittKmhText))]
    private string _fahrzeit;

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    [ObservableProperty]
    private string _maxKmh;

    /// <summary>Durchschnittlicher Puls.</summary>
    [ObservableProperty]
    private string _pulsDurchschnitt;

    /// <summary>Höchster Puls.</summary>
    [ObservableProperty]
    private string _pulsMax;

    /// <summary>Niedrigster Puls.</summary>
    [ObservableProperty]
    private string _pulsMin;

    /// <summary>Durchschnittliche Trittfrequenz.</summary>
    [ObservableProperty]
    private string _trittfrequenz;

    /// <summary>Durchschnittstemperatur in °C (darf negativ sein).</summary>
    [ObservableProperty]
    private string _temperatur;

    /// <summary>Durchschnittliche Leistung in Watt.</summary>
    [ObservableProperty]
    private string _leistung;

    /// <summary>Körpergewicht in kg.</summary>
    [ObservableProperty]
    private string _koerpergewicht;

    /// <summary>Erholungspuls.</summary>
    [ObservableProperty]
    private string _erholungspuls;

    /// <summary>Höhe des Startpunkts in Metern.</summary>
    [ObservableProperty]
    private string _startHoehe;

    /// <summary>Höchster Punkt der Tour in Metern.</summary>
    [ObservableProperty]
    private string _maxHoehe;

    /// <summary>Gewählter Trainingsbereich; leer = keiner.</summary>
    [ObservableProperty]
    private string _trainingsbereich;

    /// <summary>Freier, mehrzeiliger Kommentar.</summary>
    [ObservableProperty]
    private string _kommentar;

    /// <summary>
    /// Befehl „Speichern“: nur ohne fehlerhafte Felder wählbar. Gelingt das Speichern, schließt sich das
    /// Fenster; sonst bleibt es mit den Eingaben offen (die Fehlermeldung zeigt das Hauptfenster).
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannSpeichern))]
    private void Speichern()
    {
        if (EinheitEingaben.AlsEinheit(AktuelleEingaben(), _basis).Einheit is Trainingseinheit einheit && _speichern(einheit))
        {
            IstGespeichert = true;
            SchliessenAngefordert?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Wählbarkeit von „Speichern“: keine fehlerhaften Felder.</summary>
    private bool KannSpeichern() => _fehler.Count == 0;

    /// <summary>
    /// Prüft nach jeder Änderung eines Feldes alle Eingaben (vom CommunityToolkit über
    /// <see cref="OnPropertyChanged(PropertyChangedEventArgs)"/> aufgerufen).
    /// </summary>
    /// <param name="e">Name der geänderten Eigenschaft.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is not (nameof(DurchschnittKmhText) or "Item[]") && _anfang is not null)
        {
            Pruefen();
        }
    }

    /// <summary>Prüft die Eingaben, aktualisiert die Fehlermarkierung und die Wählbarkeit von „Speichern“.</summary>
    private void Pruefen()
    {
        _fehler = EinheitEingaben.AlsEinheit(AktuelleEingaben(), _basis).Fehler;
        OnPropertyChanged("Item[]"); // alle Indexer-Bindungen neu lesen
        SpeichernCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Fasst die Felder zu Eingaben zusammen.</summary>
    private Einheiteneingaben AktuelleEingaben() => new()
    {
        Datum = Datum,
        Startzeit = Startzeit,
        Name = Name,
        AktivitaetId = Aktivitaet?.Id,
        KategorieIds = Kategorien?.Where(k => k.IstGewaehlt).Select(k => k.Id).ToList() ?? [],
        EquipmentId = Equipment?.Id,
        Distanz = Distanz,
        Hoehenmeter = Hoehenmeter,
        Fahrzeit = Fahrzeit,
        MaxKmh = MaxKmh,
        PulsDurchschnitt = PulsDurchschnitt,
        PulsMax = PulsMax,
        PulsMin = PulsMin,
        Trittfrequenz = Trittfrequenz,
        Temperatur = Temperatur,
        Leistung = Leistung,
        Koerpergewicht = Koerpergewicht,
        Erholungspuls = Erholungspuls,
        StartHoehe = StartHoehe,
        MaxHoehe = MaxHoehe,
        Trainingsbereich = Trainingsbereich ?? string.Empty,
        Kommentar = Kommentar,
    };
}

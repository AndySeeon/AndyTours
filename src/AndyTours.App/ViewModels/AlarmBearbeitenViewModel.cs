using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Alarmfensters („Neuer Alarm…“, „Bearbeiten…“): Rad, Tätigkeit, Intervall und „Alarm eingeschaltet“.
/// </summary>
/// <remarks>
/// Hält die Felder als Text bzw. gewählten Listeneintrag. Jede Änderung prüft die Eingaben über
/// <see cref="AlarmEingaben.AlsAlarm"/>: Fehlerhafte Felder meldet der Indexer (<c>this[feld]</c>, in XAML
/// <c>{Binding [Intervall]}</c>), ein Hinweis nennt z. B. einen schon vorhandenen Alarm, und „OK“ ist nur ohne
/// Fehler wählbar.
/// <para>
/// Die Tätigkeit wird wie im Wartungsfenster nur aus der Liste „Tätigkeit“ gewählt; ist die Liste leer, weist
/// das Fenster darauf hin. Eine bisherige Tätigkeit, die so nicht in der Liste steht, bleibt wählbar.
/// </para>
/// <para>
/// Gespeichert wird über die beim Erzeugen übergebene Funktion des Hauptfensters. Gelingt es, meldet das
/// ViewModel <see cref="SchliessenAngefordert"/>; sonst bleibt das Fenster mit den Eingaben offen.
/// </para>
/// </remarks>
public partial class AlarmBearbeitenViewModel : ObservableObject
{
    /// <summary>Der bearbeitete Alarm (liefert Nummer, Tag des Anlegens und Kommentar).</summary>
    private readonly Wartung _basis;

    /// <summary>Der Datenbestand mit den übrigen Alarmen (für die Doppelprüfung; nur gelesen).</summary>
    private readonly Datenbestand _bestand;

    /// <summary>Speichert den fertigen Alarm; liefert <c>true</c> bei Erfolg.</summary>
    private readonly Func<Wartung, bool> _speichern;

    /// <summary><c>true</c>, wenn die Liste „Tätigkeit“ beim Öffnen leer war.</summary>
    private readonly bool _ohneTaetigkeiten;

    /// <summary>Die Eingaben beim Öffnen; daran wird erkannt, ob etwas geändert wurde.</summary>
    private readonly Alarmeingaben _anfang;

    /// <summary>Die zurzeit fehlerhaften Felder.</summary>
    private IReadOnlySet<Alarmfeld> _fehler = new HashSet<Alarmfeld>();

    /// <summary>
    /// Erzeugt das ViewModel und füllt die Felder aus dem Alarm.
    /// </summary>
    /// <param name="alarm">Der zu bearbeitende Alarm (bei „Neu“ der vorbereitete neue mit heutigem Datum und vorgewähltem Rad).</param>
    /// <param name="raeder">Die wählbaren Räder (siehe <see cref="ErmittleWartungsraeder"/>).</param>
    /// <param name="taetigkeiten">Die Einträge der Liste „Tätigkeit“, häufigste zuerst (siehe <see cref="OrdneTaetigkeitenNachHaeufigkeit"/>).</param>
    /// <param name="titel">Fenstertitel, z. B. „Alarm bearbeiten“.</param>
    /// <param name="speichern">Speichert den Alarm; <c>true</c> bei Erfolg.</param>
    /// <param name="bestand">Der Datenbestand (nur gelesen) für die Doppelprüfung.</param>
    public AlarmBearbeitenViewModel(
        Wartung alarm,
        IReadOnlyList<Auswahleintrag> raeder,
        IReadOnlyList<string> taetigkeiten,
        string titel,
        Func<Wartung, bool> speichern,
        Datenbestand bestand)
    {
        _basis = alarm;
        _bestand = bestand;
        _speichern = speichern;
        _ohneTaetigkeiten = taetigkeiten.Count == 0;
        Titel = titel;
        Raeder = raeder;

        Alarmeingaben e = AlarmEingaben.AlsEingaben(alarm);
        _rad = raeder.FirstOrDefault(r => r.Id == e.EquipmentId);
        _intervall = e.Intervall;
        _eingeschaltet = e.Eingeschaltet;

        // Bisherige Tätigkeit: den gleich geschriebenen Listeneintrag wählen, sonst sie selbst voranstellen
        string bisher = e.Taetigkeit;
        string? passend = taetigkeiten.FirstOrDefault(t => string.Equals(t.Trim(), bisher, StringComparison.OrdinalIgnoreCase));
        Taetigkeiten = passend is null && bisher.Length > 0 ? [bisher, .. taetigkeiten] : taetigkeiten;
        _taetigkeit = passend ?? (bisher.Length > 0 ? bisher : null);

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

    /// <summary>Die wählbaren Räder: im Einsatz alphabetisch, ein ausgemustertes am Ende mit „(ausgemustert)“.</summary>
    public IReadOnlyList<Auswahleintrag> Raeder { get; }

    /// <summary>Die wählbaren Tätigkeiten, die häufigsten zuerst.</summary>
    public IReadOnlyList<string> Taetigkeiten { get; }

    /// <summary><c>true</c>, nachdem erfolgreich gespeichert wurde (das Schließen fragt dann nicht nach).</summary>
    public bool IstGespeichert { get; private set; }

    /// <summary><c>true</c>, wenn die Eingaben vom Stand beim Öffnen abweichen.</summary>
    public bool IstGeaendert => AktuelleEingaben() != _anfang;

    /// <summary><c>true</c>, wenn das Feld rot markiert werden muss (in XAML: <c>{Binding [Intervall]}</c>).</summary>
    /// <param name="feld">Das Feld.</param>
    public bool this[Alarmfeld feld] => _fehler.Contains(feld);

    /// <summary>Das gewählte Rad (Pflicht).</summary>
    [ObservableProperty]
    private Auswahleintrag? _rad;

    /// <summary>Die gewählte Tätigkeit (Pflicht).</summary>
    [ObservableProperty]
    private string? _taetigkeit;

    /// <summary>Intervall in km, z. B. „3000“ (Pflicht, ganze Zahl größer als 0).</summary>
    [ObservableProperty]
    private string _intervall;

    /// <summary>„Alarm eingeschaltet“: ausgeschaltet wird der Alarm nie fällig.</summary>
    [ObservableProperty]
    private bool _eingeschaltet;

    /// <summary>Hinweis unter den Feldern: leere Liste „Tätigkeit“ oder schon vorhandener Alarm; leer = kein Hinweis.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>
    /// Befehl „OK“: nur ohne fehlerhafte Felder wählbar. Gelingt das Speichern, schließt sich das Fenster;
    /// sonst bleibt es mit den Eingaben offen (die Fehlermeldung zeigt das Hauptfenster).
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannSpeichern))]
    private void Speichern()
    {
        if (AlarmEingaben.AlsAlarm(AktuelleEingaben(), _basis, _bestand).Alarm is Wartung alarm && _speichern(alarm))
        {
            IstGespeichert = true;
            SchliessenAngefordert?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Wählbarkeit von „OK“: keine fehlerhaften Felder.</summary>
    private bool KannSpeichern() => _fehler.Count == 0;

    /// <summary>
    /// Prüft nach jeder Änderung eines Feldes alle Eingaben.
    /// </summary>
    /// <param name="e">Name der geänderten Eigenschaft.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is (nameof(Rad) or nameof(Taetigkeit) or nameof(Intervall) or nameof(Eingeschaltet)) && _anfang is not null)
        {
            Pruefen();
        }
    }

    /// <summary>Prüft die Eingaben, aktualisiert Fehlermarkierung, Hinweis und die Wählbarkeit von „OK“.</summary>
    private void Pruefen()
    {
        Alarmergebnis ergebnis = AlarmEingaben.AlsAlarm(AktuelleEingaben(), _basis, _bestand);
        _fehler = ergebnis.Fehler;
        Hinweis = _ohneTaetigkeiten ? WartungBearbeitenViewModel.KeineTaetigkeiten : ergebnis.Hinweis ?? string.Empty;
        OnPropertyChanged("Item[]"); // alle Indexer-Bindungen neu lesen
        SpeichernCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Fasst die Felder zu Eingaben zusammen.</summary>
    private Alarmeingaben AktuelleEingaben() => new()
    {
        EquipmentId = Rad?.Id,
        Taetigkeit = Taetigkeit ?? string.Empty,
        Intervall = Intervall,
        Eingeschaltet = Eingeschaltet,
    };
}

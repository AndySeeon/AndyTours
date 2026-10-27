using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Wartungsfensters („Neue Wartung…“, „Bearbeiten…“): Datum, Rad, Tätigkeit und Kommentar.
/// </summary>
/// <remarks>
/// Hält die Felder als Text bzw. gewählten Listeneintrag. Jede Änderung prüft die Eingaben über
/// <see cref="WartungEingaben.AlsWartung"/>: Fehlerhafte Felder meldet der Indexer (<c>this[feld]</c>, in XAML
/// <c>{Binding [Datum]}</c>), ein Hinweis nennt z. B. ein Datum in der Zukunft, und „OK“ ist nur ohne Fehler
/// wählbar.
/// <para>
/// Die Tätigkeit wird nur aus der Liste „Tätigkeit“ gewählt, die häufigsten Tätigkeiten zuerst; eine freie Eingabe gibt
/// es nicht. Ist die Liste leer, weist das Fenster darauf hin, dass zuerst in den Einstellungen Tätigkeiten
/// angelegt werden müssen. Steht in einer vorhandenen Wartung eine Tätigkeit, die so nicht in der Liste
/// steht, wird der Listeneintrag gleicher Schreibweise (ohne Groß-/Kleinschreibung) gewählt; gibt es keinen,
/// bleibt die bisherige Tätigkeit am Anfang der Auswahl wählbar – wie ein ausgemustertes Rad.
/// </para>
/// <para>
/// Gespeichert wird nicht hier, sondern über die beim Erzeugen übergebene Funktion des Hauptfensters.
/// Gelingt es, meldet das ViewModel <see cref="SchliessenAngefordert"/>; sonst bleibt das Fenster mit den
/// Eingaben offen.
/// </para>
/// </remarks>
public partial class WartungBearbeitenViewModel : ObservableObject
{
    /// <summary>Hinweis, wenn die Liste „Tätigkeit“ leer ist.</summary>
    public const string KeineTaetigkeiten =
        "Es sind noch keine Tätigkeiten angelegt. Bitte zuerst unter „Extras → Einstellungen… → Auswahllisten“ die Liste „Tätigkeit“ füllen.";

    /// <summary>Die bearbeitete Wartung (liefert Nummer, Typ, Alarm-km und Aktiv-Kennzeichen).</summary>
    private readonly Wartung _basis;

    /// <summary>Der Datenbestand mit den übrigen Wartungen (für die Doppelprüfung; nur gelesen).</summary>
    private readonly Datenbestand _bestand;

    /// <summary>Speichert die fertige Wartung; liefert <c>true</c> bei Erfolg.</summary>
    private readonly Func<Wartung, bool> _speichern;

    /// <summary>Der heutige Tag; ein späteres Datum ist nicht erlaubt.</summary>
    private readonly DateOnly _heute;

    /// <summary><c>true</c>, wenn die Liste „Tätigkeit“ beim Öffnen leer war.</summary>
    private readonly bool _ohneTaetigkeiten;

    /// <summary>Die Eingaben beim Öffnen; daran wird erkannt, ob etwas geändert wurde.</summary>
    private readonly Wartungseingaben _anfang;

    /// <summary>Die zurzeit fehlerhaften Felder.</summary>
    private IReadOnlySet<Wartungsfeld> _fehler = new HashSet<Wartungsfeld>();

    /// <summary>
    /// Erzeugt das ViewModel und füllt die Felder aus der Wartung.
    /// </summary>
    /// <param name="wartung">Die zu bearbeitende Wartung (bei „Neu“ die vorbereitete neue mit heutigem Datum und vorgewähltem Rad).</param>
    /// <param name="raeder">Die wählbaren Räder (siehe <see cref="ErmittleWartungsraeder"/>).</param>
    /// <param name="taetigkeiten">Die Einträge der Liste „Tätigkeit“, häufigste zuerst (siehe <see cref="OrdneTaetigkeitenNachHaeufigkeit"/>).</param>
    /// <param name="titel">Fenstertitel, z. B. „Wartung bearbeiten“.</param>
    /// <param name="speichern">Speichert die Wartung; <c>true</c> bei Erfolg.</param>
    /// <param name="heute">Der heutige Tag.</param>
    /// <param name="bestand">Der Datenbestand (nur gelesen) für die Doppelprüfung.</param>
    public WartungBearbeitenViewModel(
        Wartung wartung,
        IReadOnlyList<Auswahleintrag> raeder,
        IReadOnlyList<string> taetigkeiten,
        string titel,
        Func<Wartung, bool> speichern,
        DateOnly heute,
        Datenbestand bestand)
    {
        _basis = wartung;
        _bestand = bestand;
        _speichern = speichern;
        _heute = heute;
        _ohneTaetigkeiten = taetigkeiten.Count == 0;
        Titel = titel;
        Raeder = raeder;

        Wartungseingaben e = WartungEingaben.AlsEingaben(wartung);
        _datum = e.Datum;
        _rad = raeder.FirstOrDefault(r => r.Id == e.EquipmentId);
        _kommentar = e.Kommentar;

        // Bisherige Tätigkeit: den gleich geschriebenen Listeneintrag wählen, sonst sie selbst voranstellen
        string bisher = e.Taetigkeit.Trim();
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

    /// <summary><c>true</c>, wenn das Feld rot markiert werden muss (in XAML: <c>{Binding [Datum]}</c>).</summary>
    /// <param name="feld">Das Feld.</param>
    public bool this[Wartungsfeld feld] => _fehler.Contains(feld);

    /// <summary>Datum, z. B. „06.09.2026“ (Pflicht, nicht in der Zukunft).</summary>
    [ObservableProperty]
    private string _datum;

    /// <summary>Das gewählte Rad (Pflicht).</summary>
    [ObservableProperty]
    private Auswahleintrag? _rad;

    /// <summary>Die gewählte Tätigkeit (Pflicht).</summary>
    [ObservableProperty]
    private string? _taetigkeit;

    /// <summary>Kommentar, z. B. „Kettenverschleiß 0,5 %“.</summary>
    [ObservableProperty]
    private string _kommentar;

    /// <summary>Hinweis unter den Feldern: leere Liste „Tätigkeit“, Datum in der Zukunft oder schon vorhandene gleichartige Wartung an diesem Tag; leer = kein Hinweis.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>
    /// Befehl „OK“: nur ohne fehlerhafte Felder wählbar. Gelingt das Speichern, schließt sich das Fenster;
    /// sonst bleibt es mit den Eingaben offen (die Fehlermeldung zeigt das Hauptfenster).
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannSpeichern))]
    private void Speichern()
    {
        if (WartungEingaben.AlsWartung(AktuelleEingaben(), _basis, _bestand, _heute).Wartung is Wartung wartung && _speichern(wartung))
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
        if (e.PropertyName is (nameof(Datum) or nameof(Rad) or nameof(Taetigkeit) or nameof(Kommentar)) && _anfang is not null)
        {
            Pruefen();
        }
    }

    /// <summary>Prüft die Eingaben, aktualisiert Fehlermarkierung, Hinweis und die Wählbarkeit von „OK“.</summary>
    private void Pruefen()
    {
        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(AktuelleEingaben(), _basis, _bestand, _heute);
        _fehler = ergebnis.Fehler;
        Hinweis = _ohneTaetigkeiten ? KeineTaetigkeiten : ergebnis.Hinweis ?? string.Empty;
        OnPropertyChanged("Item[]"); // alle Indexer-Bindungen neu lesen
        SpeichernCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Fasst die Felder zu Eingaben zusammen.</summary>
    private Wartungseingaben AktuelleEingaben() => new()
    {
        Datum = Datum,
        EquipmentId = Rad?.Id,
        Taetigkeit = Taetigkeit ?? string.Empty,
        Kommentar = Kommentar,
    };
}

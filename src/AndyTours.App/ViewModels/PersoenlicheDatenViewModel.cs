using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Persönliche Daten“: Vorname, Nachname, Geburtstag, Größe, Gewicht, Ruhe- und
/// Maximalpuls sowie Heimatort des Sportlers.
/// </summary>
/// <remarks>
/// Das Fenster arbeitet nur auf Texten; der Datenbestand ändert sich erst, wenn „OK“ erfolgreich
/// gespeichert hat. „Abbrechen“ lässt deshalb alles unverändert. Jede Eingabe wird sofort geprüft; der
/// Hinweis nennt den ersten Fehler, und „OK“ ist gesperrt, solange eine Eingabe ungültig ist.
/// </remarks>
public partial class PersoenlicheDatenViewModel : ObservableObject
{
    /// <summary>Prüft die Eingaben und liefert den Hinweis oder <c>null</c>.</summary>
    private readonly Func<Fahrereingaben, string?> _pruefen;

    /// <summary>Speichert die Eingaben; <c>false</c>, wenn das Speichern fehlgeschlagen ist.</summary>
    private readonly Func<Fahrereingaben, bool> _speichern;

    /// <summary>
    /// Erzeugt das ViewModel und füllt die Felder mit den gespeicherten Werten.
    /// </summary>
    /// <param name="eingaben">Die bisherigen persönlichen Daten (unbekannte Werte leer).</param>
    /// <param name="pruefen">Prüft die Eingaben (Hinweis oder <c>null</c>).</param>
    /// <param name="speichern">Speichert die Eingaben.</param>
    public PersoenlicheDatenViewModel(Fahrereingaben eingaben, Func<Fahrereingaben, string?> pruefen, Func<Fahrereingaben, bool> speichern)
    {
        _pruefen = pruefen;
        _speichern = speichern;
        _vorname = eingaben.Vorname;
        _nachname = eingaben.Nachname;
        _geburtstag = eingaben.Geburtstag;
        _groesse = eingaben.Groesse;
        _gewicht = eingaben.Gewicht;
        _ruhepuls = eingaben.Ruhepuls;
        _maxpuls = eingaben.Maxpuls;
        _heimatort = eingaben.Heimatort;

        // Übernommene Altwerte außerhalb der Grenzen zeigen den Hinweis schon beim Öffnen
        Pruefen();
    }

    /// <summary>Wird ausgelöst, sobald gespeichert wurde; das Fenster schließt sich dann.</summary>
    public event EventHandler? Gespeichert;

    /// <summary>Vorname.</summary>
    [ObservableProperty]
    private string _vorname;

    /// <summary>Nachname.</summary>
    [ObservableProperty]
    private string _nachname;

    /// <summary>Geburtstag als Text.</summary>
    [ObservableProperty]
    private string _geburtstag;

    /// <summary>Größe in cm als Text.</summary>
    [ObservableProperty]
    private string _groesse;

    /// <summary>Gewicht in kg als Text.</summary>
    [ObservableProperty]
    private string _gewicht;

    /// <summary>Ruhepuls als Text.</summary>
    [ObservableProperty]
    private string _ruhepuls;

    /// <summary>Maximalpuls als Text.</summary>
    [ObservableProperty]
    private string _maxpuls;

    /// <summary>Name des Heimatorts.</summary>
    [ObservableProperty]
    private string _heimatort;

    /// <summary>Hinweis zur ersten ungültigen Eingabe; leer, wenn alles gültig ist.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>Befehl „OK“: speichert und schließt das Fenster; bei einem Fehler bleibt es offen.</summary>
    [RelayCommand(CanExecute = nameof(IstGueltig))]
    private void Ok()
    {
        if (_speichern(AktuelleEingaben()))
        {
            Gespeichert?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Wählbarkeit von „OK“: keine ungültige Eingabe.</summary>
    private bool IstGueltig() => Hinweis.Length == 0;

    /// <summary>Prüft nach jeder Änderung eines Feldes neu.</summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(Hinweis))
        {
            Pruefen();
        }
    }

    /// <summary>Prüft die Eingaben und aktualisiert Hinweis und „OK“.</summary>
    private void Pruefen()
    {
        Hinweis = _pruefen(AktuelleEingaben()) ?? string.Empty;
        OkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Fasst die Felder zu Eingaben zusammen.</summary>
    private Fahrereingaben AktuelleEingaben() => new()
    {
        Vorname = Vorname,
        Nachname = Nachname,
        Geburtstag = Geburtstag,
        Groesse = Groesse,
        Gewicht = Gewicht,
        Ruhepuls = Ruhepuls,
        Maxpuls = Maxpuls,
        Heimatort = Heimatort,
    };
}

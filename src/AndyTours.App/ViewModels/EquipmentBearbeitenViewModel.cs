using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Equipment“: Name, Aktivität, Standardkategorie, Anschaffungsdatum,
/// Anfangskilometer, Standard und ausgemustert eines neuen oder vorhandenen Equipments sowie die beim
/// Garmin-Import gemerkten Rad-Kennungen und Sensoren (Gerätezuordnung), von denen sich einzelne entfernen lassen.
/// </summary>
/// <remarks>
/// Das Fenster arbeitet nur auf Texten; der Datenbestand ändert sich erst, wenn „OK“ erfolgreich
/// gespeichert hat. „Abbrechen“ lässt deshalb alles unverändert. Jede Eingabe wird sofort geprüft; der
/// Hinweis nennt den Grund, und „OK“ ist gesperrt, solange eine Eingabe ungültig ist.
/// </remarks>
public partial class EquipmentBearbeitenViewModel : ObservableObject
{
    /// <summary>Anzeige für „keine Aktivität“ bzw. „keine Standardkategorie“.</summary>
    public const string Keine = "(keine)";

    /// <summary>Prüft die Eingaben und liefert den Hinweis oder <c>null</c>.</summary>
    private readonly Func<Equipmenteingaben, string?> _pruefen;

    /// <summary>Speichert die Eingaben; <c>false</c>, wenn das Speichern fehlgeschlagen ist.</summary>
    private readonly Func<Equipmenteingaben, bool> _speichern;

    /// <summary>
    /// Erzeugt das ViewModel und füllt die Felder mit den Eingaben des Equipments.
    /// </summary>
    /// <param name="eingaben">Die Eingaben des Equipments (für ein neues leer).</param>
    /// <param name="aktivitaeten">Die Aktivitäten; „(keine)“ wird vorne ergänzt.</param>
    /// <param name="kategorien">Die Namen der Kategorien; „(keine)“ wird vorne ergänzt.</param>
    /// <param name="titel">Titel des Fensters, z. B. „Neues Equipment“.</param>
    /// <param name="pruefen">Prüft die Eingaben (Hinweis oder <c>null</c>).</param>
    /// <param name="speichern">Speichert die Eingaben.</param>
    public EquipmentBearbeitenViewModel(
        Equipmenteingaben eingaben,
        IEnumerable<Auswahleintrag> aktivitaeten,
        IEnumerable<string> kategorien,
        string titel,
        Func<Equipmenteingaben, string?> pruefen,
        Func<Equipmenteingaben, bool> speichern)
    {
        _pruefen = pruefen;
        _speichern = speichern;
        Titel = titel;
        Aktivitaeten = [new Auswahleintrag(null, Keine), .. aktivitaeten];
        Kategorien = [Keine, .. kategorien];

        _name = eingaben.Name;
        _aktivitaet = Aktivitaeten.FirstOrDefault(a => a.Id == eingaben.AktivitaetId) ?? Aktivitaeten[0];

        // Eine eingetragene, aber nicht mehr vorhandene Kategorie bleibt wählbar, damit sie nicht stillschweigend verschwindet
        string kategorie = eingaben.StandardKategorie.Trim();
        string? vorhanden = Kategorien.Skip(1).FirstOrDefault(k => string.Equals(k, kategorie, StringComparison.OrdinalIgnoreCase));
        if (kategorie.Length > 0 && vorhanden is null)
        {
            Kategorien = [.. Kategorien, kategorie];
            vorhanden = kategorie;
        }

        _standardKategorie = vorhanden ?? Keine;
        _anschaffungsdatum = eingaben.Anschaffungsdatum;
        _anfangskilometer = eingaben.Anfangskilometer;
        _ausgemustert = eingaben.Ausgemustert;
        _istStandard = eingaben.IstStandard && KannStandard;
        Radkennungen = [.. eingaben.Radkennungen];
        Pruefen();
    }

    /// <summary>Wird ausgelöst, sobald gespeichert wurde; das Fenster schließt sich dann.</summary>
    public event EventHandler? Gespeichert;

    /// <summary>Titel des Fensters.</summary>
    public string Titel { get; }

    /// <summary>Wählbare Aktivitäten, beginnend mit „(keine)“.</summary>
    public IReadOnlyList<Auswahleintrag> Aktivitaeten { get; }

    /// <summary>Wählbare Standardkategorien, beginnend mit „(keine)“.</summary>
    public IReadOnlyList<string> Kategorien { get; }

    /// <summary>Name des Equipments.</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>Gewählte Aktivität; „(keine)“ hat die Nummer <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KannStandard))]
    private Auswahleintrag _aktivitaet;

    /// <summary>Gewählte Standardkategorie oder „(keine)“.</summary>
    [ObservableProperty]
    private string _standardKategorie;

    /// <summary>Anschaffungsdatum als Text.</summary>
    [ObservableProperty]
    private string _anschaffungsdatum;

    /// <summary>Anfangskilometer als Text.</summary>
    [ObservableProperty]
    private string _anfangskilometer;

    /// <summary>Standard-Equipment seiner Aktivität.</summary>
    [ObservableProperty]
    private bool _istStandard;

    /// <summary>Ausgemustert.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KannStandard))]
    private bool _ausgemustert;

    /// <summary>Die gemerkten Rad-Kennungen und Sensoren; „Entfernen“ nimmt die markierte heraus.</summary>
    public ObservableCollection<Radkennung> Radkennungen { get; }

    /// <summary>Die in der Liste der Gerätezuordnung markierte Kennung.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(KennungEntfernenCommand))]
    private Radkennung? _markierteKennung;

    /// <summary>Hinweis zur ersten ungültigen Eingabe; leer, wenn alles gültig ist.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>„Standard“ ist nur mit Aktivität und ohne „ausgemustert“ wählbar.</summary>
    public bool KannStandard => Aktivitaet?.Id is not null && !Ausgemustert;

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

    /// <summary>
    /// Befehl „Entfernen“ der Gerätezuordnung: nimmt die markierte Kennung aus der Liste.
    /// </summary>
    /// <remarks>
    /// Gespeichert wird erst mit „OK“. Eine entfernte Kennung lernt AndyTours beim nächsten Import wieder,
    /// dann für das dort gewählte Equipment.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(IstKennungMarkiert))]
    private void KennungEntfernen()
    {
        if (MarkierteKennung is Radkennung kennung)
        {
            Radkennungen.Remove(kennung);
        }
    }

    /// <summary>Wählbarkeit von „Entfernen“: eine Kennung ist markiert.</summary>
    private bool IstKennungMarkiert() => MarkierteKennung is not null;

    /// <summary>Prüft nach jeder Änderung eines Feldes neu; ohne Möglichkeit für Standard wird er abgeschaltet.</summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Hinweis) or nameof(KannStandard))
        {
            return;
        }

        if (!KannStandard && IstStandard)
        {
            IstStandard = false;
        }

        Pruefen();
    }

    /// <summary>Prüft die Eingaben und aktualisiert Hinweis und „OK“.</summary>
    private void Pruefen()
    {
        Hinweis = _pruefen(AktuelleEingaben()) ?? string.Empty;
        OkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Fasst die Felder zu Eingaben zusammen.</summary>
    private Equipmenteingaben AktuelleEingaben() => new()
    {
        Name = Name,
        AktivitaetId = Aktivitaet?.Id,
        StandardKategorie = StandardKategorie == Keine ? string.Empty : StandardKategorie ?? string.Empty,
        Anschaffungsdatum = Anschaffungsdatum,
        Anfangskilometer = Anfangskilometer,
        IstStandard = IstStandard && KannStandard,
        Ausgemustert = Ausgemustert,
        Radkennungen = [.. Radkennungen],
    };
}

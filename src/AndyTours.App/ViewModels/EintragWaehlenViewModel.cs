using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Eintrag wählen“: Auswahl einer Aktivität bzw. eines Equipments für alle
/// markierten Einheiten.
/// </summary>
/// <remarks>
/// Das Fenster wählt nur aus. Ergebnis ist der gewählte <see cref="Auswahleintrag"/>; „(kein)“ beim
/// Equipment hat die Nummer <c>null</c>. Rückfrage und Speichern folgen nach dem Schließen im Hauptfenster.
/// </remarks>
public partial class EintragWaehlenViewModel : ObservableObject
{
    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="titel">Titel des Fensters, z. B. „Aktivität ändern“.</param>
    /// <param name="erklaerung">Text über der Liste, z. B. „Neue Aktivität für 3 Einheiten:“.</param>
    /// <param name="eintraege">Die wählbaren Einträge.</param>
    /// <param name="vorgewaehlt">Vorgewählter Eintrag oder <c>null</c> für keine Vorauswahl.</param>
    public EintragWaehlenViewModel(string titel, string erklaerung, IReadOnlyList<Auswahleintrag> eintraege, Auswahleintrag? vorgewaehlt)
    {
        Titel = titel;
        Erklaerung = erklaerung;
        Eintraege = eintraege;
        _ausgewaehlt = vorgewaehlt;
    }

    /// <summary>Wird ausgelöst, sobald der Benutzer einen Eintrag bestätigt hat; das Fenster schließt sich dann.</summary>
    public event EventHandler? Gewaehlt;

    /// <summary>Titel des Fensters.</summary>
    public string Titel { get; }

    /// <summary>Text über der Liste.</summary>
    public string Erklaerung { get; }

    /// <summary>Die wählbaren Einträge.</summary>
    public IReadOnlyList<Auswahleintrag> Eintraege { get; }

    /// <summary>Der markierte Eintrag oder <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private Auswahleintrag? _ausgewaehlt;

    /// <summary>Der bestätigte Eintrag; <c>null</c>, solange nicht bestätigt.</summary>
    public Auswahleintrag? Ergebnis { get; private set; }

    /// <summary>Befehl „OK“: übernimmt den markierten Eintrag.</summary>
    [RelayCommand(CanExecute = nameof(IstAusgewaehlt))]
    private void Ok()
    {
        Ergebnis = Ausgewaehlt;
        Gewaehlt?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Wählbarkeit von „OK“: ein Eintrag ist markiert.</summary>
    private bool IstAusgewaehlt() => Ausgewaehlt is not null;
}

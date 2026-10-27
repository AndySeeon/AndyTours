using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Zusammenführen mit …“: Wahl des Eintrags, in dem der markierte Eintrag aufgeht.
/// </summary>
/// <remarks>
/// Das Fenster wählt nur aus. Die Rückfrage mit der Anzahl der Einheiten und das Zusammenführen selbst
/// folgen erst nach dem Schließen im <see cref="AuswahllistenViewModel"/>.
/// </remarks>
public partial class ZusammenfuehrenViewModel : ObservableObject
{
    /// <summary>
    /// Erzeugt das ViewModel; vorgewählt ist kein Ziel, damit nicht versehentlich zusammengeführt wird.
    /// </summary>
    /// <param name="ursprung">Der Eintrag, der entfernt werden soll.</param>
    /// <param name="ziele">Die übrigen Einträge derselben Liste.</param>
    public ZusammenfuehrenViewModel(string ursprung, IReadOnlyList<string> ziele)
    {
        Erklaerung = $"‚{ursprung}‘ zusammenführen mit:";
        Ziele = ziele;
    }

    /// <summary>Wird ausgelöst, sobald der Benutzer ein Ziel bestätigt hat; das Fenster schließt sich dann.</summary>
    public event EventHandler? Gewaehlt;

    /// <summary>Text über der Liste, z. B. „‚Trainieren‘ zusammenführen mit:“.</summary>
    public string Erklaerung { get; }

    /// <summary>Die wählbaren Ziele.</summary>
    public IReadOnlyList<string> Ziele { get; }

    /// <summary>Das markierte Ziel oder <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private string? _ausgewaehlt;

    /// <summary>Das bestätigte Ziel; <c>null</c>, solange nicht bestätigt.</summary>
    public string? Ergebnis { get; private set; }

    /// <summary>Befehl „OK“: übernimmt das markierte Ziel.</summary>
    [RelayCommand(CanExecute = nameof(IstAusgewaehlt))]
    private void Ok()
    {
        Ergebnis = Ausgewaehlt;
        Gewaehlt?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Wählbarkeit von „OK“: ein Ziel ist markiert.</summary>
    private bool IstAusgewaehlt() => Ausgewaehlt is not null;
}

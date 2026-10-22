using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Einstellungsfensters.
/// </summary>
/// <remarks>
/// Derzeit enthält es nur den Datenordner. Der eigentliche Wechsel (Ordnerwahl, Rückfragen,
/// Kopieren, Laden, Merken) wird vom Hauptfenster-ViewModel als Funktion übergeben, weil dieses
/// den Datenbestand hält und neu laden muss.
/// </remarks>
public partial class EinstellungenViewModel : ObservableObject
{
    /// <summary>Erklärung, warum der Datenordner in der Entwicklungsversion nicht änderbar ist.</summary>
    public const string Entwicklungshinweis =
        "Diese Entwicklungsversion verwendet immer den Ordner „AppDaten“ im Projektordner, damit Ihre " +
        "echten Daten unberührt bleiben. Der Datenordner lässt sich nur in der fertigen Programmversion ändern.";

    /// <summary>Führt den Ordnerwechsel durch; liefert den neuen Ordner oder <c>null</c>.</summary>
    private readonly Func<Task<string?>> _ordnerWechseln;

    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="datenordner">Der aktuelle Datenordner.</param>
    /// <param name="aendernMoeglich"><c>false</c> in der Entwicklungsversion.</param>
    /// <param name="ordnerWechseln">Ablauf des Ordnerwechsels (vom Hauptfenster-ViewModel).</param>
    public EinstellungenViewModel(string datenordner, bool aendernMoeglich, Func<Task<string?>> ordnerWechseln)
    {
        _datenordner = datenordner;
        AendernMoeglich = aendernMoeglich;
        _ordnerWechseln = ordnerWechseln;
    }

    /// <summary>Vollständiger Pfad des aktuellen Datenordners.</summary>
    [ObservableProperty]
    private string _datenordner;

    /// <summary><c>true</c>, wenn der Datenordner geändert werden darf (nur in der fertigen Programmversion).</summary>
    public bool AendernMoeglich { get; }

    /// <summary><c>true</c>, wenn der <see cref="Entwicklungshinweis"/> angezeigt werden soll.</summary>
    public bool HinweisSichtbar => !AendernMoeglich;

    /// <summary>Text des Hinweises (für die Datenbindung).</summary>
    public string Hinweis => Entwicklungshinweis;

    /// <summary>
    /// Befehl „Ändern…“: startet den Ordnerwechsel und zeigt danach den neuen Ordner an.
    /// </summary>
    /// <remarks>
    /// Nur ausführbar, wenn <see cref="AendernMoeglich"/> gilt – dadurch ist die Schaltfläche in
    /// der Entwicklungsversion gesperrt.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(AendernMoeglich))]
    private async Task Aendern()
    {
        string? neu = await _ordnerWechseln();
        if (neu is not null)
        {
            Datenordner = neu;
        }
    }
}

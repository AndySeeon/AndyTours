using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Garmin-Import“: die neuen Touren mit Häkchen und Equipment sowie der Befehl „Importieren“.
/// </summary>
/// <remarks>
/// Das Fenster ändert nichts am Datenbestand. Nach „Importieren“ liefert <see cref="Ergebnis"/> die angehakten
/// Zeilen; Ablegen, Speichern und Markieren übernimmt der Aufrufer (<c>HauptfensterViewModel.Import</c>).
/// </remarks>
public partial class GarminImportViewModel : ObservableObject
{
    /// <summary>Anzeige für „kein Equipment“.</summary>
    public const string Keines = "(keines)";

    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="zeilen">Die neuen Touren, alle angehakt.</param>
    /// <param name="equipments">Die wählbaren Equipments; „(keines)“ wird vorne ergänzt.</param>
    public GarminImportViewModel(IReadOnlyList<Importzeile> zeilen, IEnumerable<Auswahleintrag> equipments)
    {
        Zeilen = zeilen;
        Equipments = [new Auswahleintrag(null, Keines), .. equipments];
        MehrereGeraete = zeilen.Select(z => z.Geraet).Distinct().Count() > 1;
        foreach (Importzeile zeile in zeilen)
        {
            zeile.PropertyChanged += ZeileGeaendert;
        }
    }

    /// <summary>Wird ausgelöst, sobald „Importieren“ gewählt wurde; das Fenster schließt sich daraufhin.</summary>
    public event EventHandler? Bestaetigt;

    /// <summary>Die neuen Touren.</summary>
    public IReadOnlyList<Importzeile> Zeilen { get; }

    /// <summary>Wählbare Equipments, beginnend mit „(keines)“.</summary>
    public IReadOnlyList<Auswahleintrag> Equipments { get; }

    /// <summary><c>true</c>, wenn die Touren von mehreren Geräten stammen; dann ist die Spalte „Gerät“ sichtbar.</summary>
    public bool MehrereGeraete { get; }

    /// <summary>Die angehakten Zeilen nach „Importieren“; leer bei Abbruch.</summary>
    public IReadOnlyList<Importzeile> Ergebnis { get; private set; } = [];

    /// <summary>Befehl „Importieren“: übernimmt die angehakten Touren und schließt das Fenster.</summary>
    [RelayCommand(CanExecute = nameof(IstEineAngehakt))]
    private void Importieren()
    {
        Ergebnis = Zeilen.Where(z => z.Importieren).ToList();
        Bestaetigt?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>„Importieren“ ist nur wählbar, wenn mindestens eine Tour angehakt ist.</summary>
    private bool IstEineAngehakt() => Zeilen.Any(z => z.Importieren);

    /// <summary>Aktualisiert „Importieren“, wenn ein Häkchen gesetzt oder entfernt wird.</summary>
    private void ZeileGeaendert(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Importzeile.Importieren))
        {
            ImportierenCommand.NotifyCanExecuteChanged();
        }
    }
}

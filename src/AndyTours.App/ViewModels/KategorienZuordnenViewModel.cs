using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Kategorien zuordnen“: setzt oder entfernt Kategorien bei allen markierten Einheiten.
/// </summary>
/// <remarks>
/// Das Fenster wählt nur aus. Ergebnis sind die Kategorien, deren Häkchen sich gegenüber dem Anfang geändert
/// hat: auf „an“ gestellte werden bei allen Einheiten gesetzt, auf „aus“ gestellte bei allen entfernt.
/// Häkchen, die auf „teilweise“ stehen, ändern nichts. Rückfrage und Speichern folgen nach dem Schließen
/// im Hauptfenster.
/// </remarks>
public partial class KategorienZuordnenViewModel : ObservableObject
{
    /// <summary>
    /// Erzeugt das ViewModel mit den Zuständen der Kategorien.
    /// </summary>
    /// <param name="zustaende">Je Kategorie (in Anzeigereihenfolge): alle, keine oder teilweise.</param>
    /// <param name="anzahlEinheiten">Anzahl der markierten Einheiten (für die Erklärung).</param>
    public KategorienZuordnenViewModel(IEnumerable<(Kategorie Kategorie, bool? Zustand)> zustaende, int anzahlEinheiten)
    {
        Zeilen = zustaende.Select(z => new Kategoriezustand(z.Kategorie.Id, z.Kategorie.Name, z.Zustand)).ToList();
        Erklaerung = anzahlEinheiten == 1
            ? "Kategorien der markierten Einheit:"
            : string.Format(CultureInfo.GetCultureInfo("de-DE"),
                "Kategorien der {0:N0} markierten Einheiten (grau = nur bei manchen; bleibt unverändert):", anzahlEinheiten);
    }

    /// <summary>Wird ausgelöst, sobald der Benutzer „OK“ gewählt hat; das Fenster schließt sich dann.</summary>
    public event EventHandler? Bestaetigt;

    /// <summary>Text über der Liste.</summary>
    public string Erklaerung { get; }

    /// <summary>Die Kategorien mit ihren Häkchen.</summary>
    public IReadOnlyList<Kategoriezustand> Zeilen { get; }

    /// <summary>Kategorien, die nach „OK“ bei allen Einheiten gesetzt werden.</summary>
    public IReadOnlyList<long> Setzen { get; private set; } = [];

    /// <summary>Kategorien, die nach „OK“ bei allen Einheiten entfernt werden.</summary>
    public IReadOnlyList<long> Entfernen { get; private set; } = [];

    /// <summary><c>true</c>, wenn „OK“ etwas ändert.</summary>
    public bool AendertEtwas => Setzen.Count > 0 || Entfernen.Count > 0;

    /// <summary>Befehl „OK“: übernimmt die geänderten Häkchen und schließt das Fenster.</summary>
    [RelayCommand]
    private void Ok()
    {
        List<Kategoriezustand> geaendert = Zeilen.Where(z => z.IstGewaehlt is not null && z.IstGewaehlt != z.Anfang).ToList();
        Setzen = geaendert.Where(z => z.IstGewaehlt == true).Select(z => z.Id).ToList();
        Entfernen = geaendert.Where(z => z.IstGewaehlt == false).Select(z => z.Id).ToList();
        Bestaetigt?.Invoke(this, EventArgs.Empty);
    }
}

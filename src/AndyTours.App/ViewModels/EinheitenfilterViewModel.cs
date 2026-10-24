using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel der Filterleiste über der Tabelle „Trainingseinheiten“.
/// </summary>
/// <remarks>
/// Hält die Eingaben der Filterleiste als Text, wie sie in den Feldern stehen. Jede Änderung prüft die
/// Zahlenfelder (<see cref="LeseFilterbereich"/>), setzt die Fehlermerker und <see cref="IstGesetzt"/> und
/// meldet sich über <see cref="Geaendert"/>; darauf baut das Hauptfenster die Tabelle sofort neu auf.
/// <para>
/// Der Zustand wird bewusst nirgends gespeichert: Nach einem Neustart sind die Filter leer, damit nicht
/// unbemerkt Einheiten fehlen.
/// </para>
/// </remarks>
public partial class EinheitenfilterViewModel : ObservableObject
{
    /// <summary>
    /// Solange <c>true</c>, lösen Änderungen der Eingaben kein <see cref="Geaendert"/> aus
    /// (beim Zurücksetzen mehrerer Felder auf einmal).
    /// </summary>
    private bool _unterdrueckt;

    /// <summary>
    /// Wird ausgelöst, sobald sich ein Filter geändert hat; das Hauptfenster aktualisiert daraufhin die Tabelle.
    /// </summary>
    public event EventHandler? Geaendert;

    /// <summary>Einträge der Auswahlliste „Kategorie“ (beginnend mit „(alle)“).</summary>
    public ObservableCollection<string> Kategorien { get; } = new() { Filterwerte.Alle };

    /// <summary>Einträge der Auswahlliste „Equipment“ (beginnend mit „(alle)“).</summary>
    public ObservableCollection<string> Equipments { get; } = new() { Filterwerte.Alle };

    /// <summary>Einträge der Auswahlliste „Trainingsbereich“ (beginnend mit „(alle)“).</summary>
    public ObservableCollection<string> Trainingsbereiche { get; } = new() { Filterwerte.Alle };

    /// <summary>Gewählte Kategorie; „(alle)“ = kein Filter.</summary>
    [ObservableProperty]
    private string? _kategorie = Filterwerte.Alle;

    /// <summary>Gewähltes Equipment; „(alle)“ = kein Filter.</summary>
    [ObservableProperty]
    private string? _equipment = Filterwerte.Alle;

    /// <summary>Gewählter Trainingsbereich; „(alle)“ = kein Filter.</summary>
    [ObservableProperty]
    private string? _trainingsbereich = Filterwerte.Alle;

    /// <summary>Eingabe „Distanz von“ in km.</summary>
    [ObservableProperty]
    private string _distanzVon = string.Empty;

    /// <summary>Eingabe „Distanz bis“ in km.</summary>
    [ObservableProperty]
    private string _distanzBis = string.Empty;

    /// <summary>Eingabe „Höhenmeter von“.</summary>
    [ObservableProperty]
    private string _hoehenmeterVon = string.Empty;

    /// <summary>Eingabe „Höhenmeter bis“.</summary>
    [ObservableProperty]
    private string _hoehenmeterBis = string.Empty;

    /// <summary>Eingabe im Namensfeld.</summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary><c>true</c>, wenn das Feld „Distanz von“ rot markiert wird.</summary>
    [ObservableProperty]
    private bool _distanzVonFehlerhaft;

    /// <summary><c>true</c>, wenn das Feld „Distanz bis“ rot markiert wird.</summary>
    [ObservableProperty]
    private bool _distanzBisFehlerhaft;

    /// <summary><c>true</c>, wenn das Feld „Höhenmeter von“ rot markiert wird.</summary>
    [ObservableProperty]
    private bool _hoehenmeterVonFehlerhaft;

    /// <summary><c>true</c>, wenn das Feld „Höhenmeter bis“ rot markiert wird.</summary>
    [ObservableProperty]
    private bool _hoehenmeterBisFehlerhaft;

    /// <summary>
    /// <c>true</c>, wenn mindestens ein Filter gesetzt ist; die Schaltfläche „Filter zurücksetzen“ wird dann hervorgehoben.
    /// </summary>
    /// <remarks>
    /// Ein Zahlenfeld mit ungültigem Inhalt zählt mit, denn auch dann ist etwas eingetragen, das sich
    /// zurücksetzen lässt.
    /// </remarks>
    [ObservableProperty]
    private bool _istGesetzt;

    /// <summary>
    /// Liefert den aktuell gesetzten Filter für <see cref="FiltereEinheiten"/>.
    /// </summary>
    /// <returns>Der Filter; „(alle)“ und leere Felder bedeuten „keine Einschränkung“.</returns>
    public Einheitenfilter ErstelleFilter() => new(
        Kategorie: OhneAlle(Kategorie),
        Equipment: OhneAlle(Equipment),
        Trainingsbereich: OhneAlle(Trainingsbereich),
        Distanz: LeseFilterbereich.Ausfuehren(DistanzVon, DistanzBis),
        Hoehenmeter: LeseFilterbereich.Ausfuehren(HoehenmeterVon, HoehenmeterBis),
        Name: Name);

    /// <summary>
    /// Stellt eine gewählte Kategorie auf ihren neuen Namen um (nach Umbenennen oder Zusammenführen in den
    /// Einstellungen); ist eine andere Kategorie gewählt, ändert sich nichts.
    /// </summary>
    /// <remarks>
    /// Der neue Name kommt vor dem Umstellen in die Liste, damit die Auswahlliste der Oberfläche ihn findet
    /// und die Wahl nicht auf „nichts“ zurücksetzt. Neu berechnet wird hier nicht; der Aufrufer baut danach
    /// Listen und Tabelle neu auf, wobei der alte Name aus der Liste verschwindet.
    /// </remarks>
    /// <param name="alt">Bisheriger Name der Kategorie.</param>
    /// <param name="neu">Neuer Name bzw. Name der Zielkategorie.</param>
    public void ErsetzeKategorie(string alt, string neu)
    {
        if (Kategorie != alt)
        {
            return;
        }

        int stelle = Kategorien.IndexOf(alt);
        if (!Kategorien.Contains(neu))
        {
            Kategorien.Insert(stelle < 0 ? Kategorien.Count : stelle + 1, neu);
        }

        _unterdrueckt = true;
        try
        {
            Kategorie = neu;
        }
        finally
        {
            _unterdrueckt = false;
        }
    }

    /// <summary>
    /// Übernimmt neue Einträge für die drei Auswahllisten (nach einem Wechsel von Jahr, Aktivität oder Datenbestand).
    /// </summary>
    /// <remarks>
    /// Die Listen werden abgeglichen statt geleert: Würde die Liste kurz leer, setzte die Auswahlliste der
    /// Oberfläche die Wahl auf „nichts“ zurück. <see cref="ErmittleFilterwerte"/> nimmt die gewählte
    /// Eintragung immer mit auf, sie wird also nie entfernt.
    /// </remarks>
    /// <param name="werte">Die neuen Einträge.</param>
    public void UebernimmListen(Filterwerte werte)
    {
        Abgleichen(Kategorien, werte.Kategorien);
        Abgleichen(Equipments, werte.Equipments);
        Abgleichen(Trainingsbereiche, werte.Trainingsbereiche);
    }

    /// <summary>
    /// Befehl „Filter zurücksetzen“: leert alle Filter und meldet die Änderung einmal.
    /// </summary>
    [RelayCommand]
    private void FilterZuruecksetzen()
    {
        _unterdrueckt = true;
        try
        {
            Kategorie = Filterwerte.Alle;
            Equipment = Filterwerte.Alle;
            Trainingsbereich = Filterwerte.Alle;
            DistanzVon = string.Empty;
            DistanzBis = string.Empty;
            HoehenmeterVon = string.Empty;
            HoehenmeterBis = string.Empty;
            Name = string.Empty;
        }
        finally
        {
            _unterdrueckt = false;
        }

        Neuberechnen();
    }

    // Jede Eingabe löst die Neuberechnung aus (vom CommunityToolkit aufgerufen).
    partial void OnKategorieChanged(string? value) => Neuberechnen();

    partial void OnEquipmentChanged(string? value) => Neuberechnen();

    partial void OnTrainingsbereichChanged(string? value) => Neuberechnen();

    partial void OnDistanzVonChanged(string value) => Neuberechnen();

    partial void OnDistanzBisChanged(string value) => Neuberechnen();

    partial void OnHoehenmeterVonChanged(string value) => Neuberechnen();

    partial void OnHoehenmeterBisChanged(string value) => Neuberechnen();

    partial void OnNameChanged(string value) => Neuberechnen();

    /// <summary>
    /// Setzt Fehlermerker und <see cref="IstGesetzt"/> neu und meldet die Änderung.
    /// </summary>
    private void Neuberechnen()
    {
        if (_unterdrueckt)
        {
            return;
        }

        Einheitenfilter filter = ErstelleFilter();
        DistanzVonFehlerhaft = filter.Distanz.VonFehlerhaft;
        DistanzBisFehlerhaft = filter.Distanz.BisFehlerhaft;
        HoehenmeterVonFehlerhaft = filter.Hoehenmeter.VonFehlerhaft;
        HoehenmeterBisFehlerhaft = filter.Hoehenmeter.BisFehlerhaft;

        IstGesetzt = filter.Kategorie is not null
            || filter.Equipment is not null
            || filter.Trainingsbereich is not null
            || !string.IsNullOrWhiteSpace(DistanzVon) || !string.IsNullOrWhiteSpace(DistanzBis)
            || !string.IsNullOrWhiteSpace(HoehenmeterVon) || !string.IsNullOrWhiteSpace(HoehenmeterBis)
            || !string.IsNullOrWhiteSpace(Name);

        Geaendert?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>„(alle)“ oder keine Wahl → <c>null</c> (kein Filter).</summary>
    private static string? OhneAlle(string? wert) => wert is null || wert == Filterwerte.Alle ? null : wert;

    /// <summary>
    /// Bringt eine Liste auf den neuen Stand, ohne sie zwischendurch zu leeren.
    /// </summary>
    /// <remarks>
    /// Zuerst werden Einträge entfernt, die nicht mehr vorkommen; die übrigen stehen dann bereits in der
    /// richtigen Reihenfolge (beide Listen sind gleich sortiert), sodass nur noch fehlende eingefügt werden.
    /// </remarks>
    /// <param name="liste">Die angezeigte Liste.</param>
    /// <param name="neu">Die neuen Einträge in der gewünschten Reihenfolge.</param>
    private static void Abgleichen(ObservableCollection<string> liste, IReadOnlyList<string> neu)
    {
        var behalten = new HashSet<string>(neu, StringComparer.Ordinal);
        for (int i = liste.Count - 1; i >= 0; i--)
        {
            if (!behalten.Contains(liste[i]))
            {
                liste.RemoveAt(i);
            }
        }

        for (int i = 0; i < neu.Count; i++)
        {
            if (i >= liste.Count || liste[i] != neu[i])
            {
                liste.Insert(i, neu[i]);
            }
        }
    }
}

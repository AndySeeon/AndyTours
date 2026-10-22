using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RadlDB.App.ViewModels;

/// <summary>
/// ViewModel des Hauptfensters (Muster MVVM: Model – View – ViewModel).
/// </summary>
/// <remarks>
/// Das ViewModel enthält die Daten und Befehle, an die sich die Oberfläche
/// (<c>Views/Hauptfenster.xaml</c>) per Datenbindung anbindet. Es kennt das Fenster selbst
/// nicht, dadurch bleibt die Logik von der Darstellung getrennt.
/// <para>
/// <see cref="ObservableObject"/> aus dem CommunityToolkit benachrichtigt die Oberfläche
/// automatisch, sobald sich eine Eigenschaft ändert. Die Attribute
/// <c>[ObservableProperty]</c> und <c>[RelayCommand]</c> lassen den Compiler die
/// zugehörigen Eigenschaften bzw. Befehle selbst erzeugen.
/// </para>
/// </remarks>
public partial class HauptfensterViewModel : ObservableObject
{
    /// <summary>Eintrag in der Jahresauswahl, der alle Jahre zusammenfasst.</summary>
    public const string AlleJahre = "alle Jahre";

    /// <summary>Eintrag in der Aktivitätsauswahl, der alle Aktivitäten zusammenfasst.</summary>
    public const string AlleAktivitaeten = "alle Aktivitäten";

    /// <summary>
    /// Erzeugt das ViewModel und füllt die Auswahllisten mit Startwerten.
    /// </summary>
    public HauptfensterViewModel()
    {
        // Vorläufige Jahresliste: nur "alle Jahre" und das aktuelle Jahr.
        // Ab Phase 1 werden hier die Jahre eingetragen, für die Trainingseinheiten vorliegen.
        string aktuellesJahr = DateTime.Today.Year.ToString();
        Jahre = new ObservableCollection<string> { AlleJahre, aktuellesJahr };
        AusgewaehltesJahr = aktuellesJahr;

        // Vorläufige Aktivitätsliste. Ab Phase 1 kommen die Aktivitäten aus der Datenbank
        // (aus den Altdaten übernommen: "Bike" und "Laufen").
        Aktivitaeten = new ObservableCollection<string> { AlleAktivitaeten };
        AusgewaehlteAktivitaet = AlleAktivitaeten;

        Statustext = "Bereit – Grundgerüst (Phase 0). Es sind noch keine Daten geladen.";
    }

    /// <summary>Fenstertitel inklusive Programmversion.</summary>
    public string Titel => $"RadlDB {Programmversion}";

    /// <summary>
    /// Versionsnummer der Anwendung, gelesen aus den Assembly-Informationen.
    /// </summary>
    public static string Programmversion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";

    /// <summary>Auswählbare Jahre in der Werkzeugleiste.</summary>
    public ObservableCollection<string> Jahre { get; }

    /// <summary>Auswählbare Aktivitäten in der Werkzeugleiste.</summary>
    public ObservableCollection<string> Aktivitaeten { get; }

    /// <summary>Aktuell in der Werkzeugleiste gewähltes Jahr.</summary>
    [ObservableProperty]
    private string _ausgewaehltesJahr = string.Empty;

    /// <summary>Aktuell in der Werkzeugleiste gewählte Aktivität.</summary>
    [ObservableProperty]
    private string _ausgewaehlteAktivitaet = string.Empty;

    /// <summary>Text, der unten in der Statusleiste angezeigt wird.</summary>
    [ObservableProperty]
    private string _statustext = string.Empty;

    /// <summary>
    /// Befehl "Datei → Beenden": beendet die Anwendung.
    /// </summary>
    [RelayCommand]
    private void Beenden()
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Befehl "Hilfe → Über RadlDB": zeigt Programmname und Version an.
    /// </summary>
    [RelayCommand]
    private void Ueber()
    {
        MessageBox.Show(
            $"RadlDB {Programmversion}\n\nTourenverwaltung für Radltouren.\n" +
            "Neuentwicklung als Nachfolger von BikeXperience.",
            "Über RadlDB",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

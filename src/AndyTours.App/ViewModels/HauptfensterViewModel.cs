using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using AndyTours.App.Funktionen;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.ViewModels;

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

    /// <summary>Der geladene Datenbestand des aktiven Datenordners.</summary>
    private Datenbestand _bestand = new();

    /// <summary>
    /// Erzeugt das ViewModel und lädt den Datenbestand aus dem Datenordner.
    /// </summary>
    /// <remarks>
    /// Der Datenordner wurde beim Programmstart bereits ermittelt und geprüft (siehe <c>App</c>).
    /// </remarks>
    /// <param name="datenordner">Der aktive Datenordner.</param>
    /// <exception cref="SqliteException">Die Datenbank im Ordner ist beschädigt oder nicht lesbar.</exception>
    public HauptfensterViewModel(string datenordner)
    {
        Jahre = new ObservableCollection<string> { AlleJahre };
        Aktivitaeten = new ObservableCollection<string> { AlleAktivitaeten };
        AusgewaehltesJahr = AlleJahre;
        AusgewaehlteAktivitaet = AlleAktivitaeten;

        LadeDatenbank(datenordner);
    }

    /// <summary>Fenstertitel inklusive Programmversion.</summary>
    public string Titel => $"AndyTours {Programmversion}";

    /// <summary>
    /// Versionsnummer der Anwendung, gelesen aus den Assembly-Informationen.
    /// </summary>
    public static string Programmversion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";

    /// <summary>Der aktive Datenordner; wird rechts in der Statusleiste angezeigt.</summary>
    [ObservableProperty]
    private string _datenordner = string.Empty;

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

    /// <summary>Text, der links in der Statusleiste angezeigt wird (Gesamtzahl der Einheiten).</summary>
    [ObservableProperty]
    private string _statustext = string.Empty;

    /// <summary>
    /// Summen der aktuellen Auswahl (Anzahl, km, hm) bzw. der Hinweis, dass noch keine Touren
    /// vorhanden sind; wird in der Werkzeugleiste neben der Auswahl angezeigt.
    /// </summary>
    [ObservableProperty]
    private string _summentext = string.Empty;

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Jahresauswahl ändert.</summary>
    partial void OnAusgewaehltesJahrChanged(string value) => AktualisiereSummen();

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Aktivitätsauswahl ändert.</summary>
    partial void OnAusgewaehlteAktivitaetChanged(string value) => AktualisiereSummen();

    /// <summary>
    /// Öffnet die Datenbank im angegebenen Datenordner, übernimmt ihn als aktiven Ordner und füllt die Auswahllisten.
    /// </summary>
    /// <remarks>
    /// Enthält der Ordner noch keine Datenbank, wird eine leere angelegt – die App beginnt dann
    /// mit einem leeren Datenbestand. Schlägt das Laden fehl, bleibt der bisherige Zustand erhalten,
    /// weil der neue Bestand erst nach dem erfolgreichen Laden übernommen wird.
    /// </remarks>
    /// <param name="ordner">Datenordner (erreichbar und beschreibbar).</param>
    private void LadeDatenbank(string ordner)
    {
        Datenbestand geladen;
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(ordner, DatenordnerInhalt.Datenbankdatei)))
        {
            geladen = LadeDatenbestand.Ausfuehren(verbindung);
        }

        _bestand = geladen;
        Datenordner = ordner;

        // Jahresliste: „alle Jahre“, dann alle Jahre mit Einheiten und immer das aktuelle Jahr, das neueste zuerst
        int aktuellesJahr = DateTime.Today.Year;
        Jahre.Clear();
        Jahre.Add(AlleJahre);
        foreach (int jahr in ErmittleAuswahljahre.Ausfuehren(_bestand.Trainingseinheiten, aktuellesJahr))
        {
            Jahre.Add(jahr.ToString(CultureInfo.InvariantCulture));
        }

        Aktivitaeten.Clear();
        Aktivitaeten.Add(AlleAktivitaeten);
        foreach (Aktivitaet aktivitaet in _bestand.Aktivitaeten)
        {
            Aktivitaeten.Add(aktivitaet.Name);
        }

        // Vorauswahl: aktuelles Jahr (steht immer in der Liste) und Hauptaktivität; ohne Hauptaktivität alle Aktivitäten
        AusgewaehltesJahr = aktuellesJahr.ToString(CultureInfo.InvariantCulture);
        AusgewaehlteAktivitaet = _bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Name ?? AlleAktivitaeten;

        Statustext = string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0:N0} Trainingseinheiten in der Datenbank", _bestand.Trainingseinheiten.Count);
        AktualisiereSummen();
    }

    /// <summary>
    /// Berechnet die Summen der aktuellen Jahres- und Aktivitätsauswahl und zeigt sie neben der Auswahl an.
    /// </summary>
    /// <remarks>
    /// Wird bei jeder Änderung der Auswahl aufgerufen. Die Einträge „alle Jahre“ und „alle Aktivitäten“
    /// werden als „keine Einschränkung“ (<c>null</c>) an <see cref="FiltereAuswahl"/> übergeben.
    /// </remarks>
    private void AktualisiereSummen()
    {
        if (_bestand.Trainingseinheiten.Count == 0)
        {
            Summentext = "Noch keine Touren vorhanden.";
            return;
        }

        // Auswahl aus der Werkzeugleiste in Filterwerte übersetzen
        int? jahr = int.TryParse(AusgewaehltesJahr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int j) ? j : null;
        long? aktivitaetId = _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet)?.Id;

        Auswahlsumme summe = BerechneAuswahlsummen.Ausfuehren(FiltereAuswahl.Ausfuehren(_bestand.Trainingseinheiten, jahr, aktivitaetId));
        Summentext = string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0:N0} Einheiten · {1:N2} km · {2:N0} hm",
            summe.Anzahl, summe.DistanzKm, summe.Hoehenmeter);
    }

    /// <summary>
    /// Befehl „Extras → Einstellungen…“: öffnet das Einstellungsfenster.
    /// </summary>
    [RelayCommand]
    private void EinstellungenOeffnen()
    {
        var viewModel = new EinstellungenViewModel(Datenordner, !Programmpfade.IstEntwicklung, WechsleZuNeuemDatenordner);
        var fenster = new EinstellungenFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Befehl „Extras → Sicherung wiederherstellen…“: Sicherung wählen, nachfragen, zurückspielen und
    /// danach den Datenbestand neu laden, sodass Jahre, Aktivitäten und Summen sofort den Stand der
    /// Sicherung zeigen.
    /// </summary>
    [RelayCommand]
    private async Task SicherungWiederherstellen()
    {
        await FuehreWiederherstellungDurch.Ausfuehren(Datenordner, () =>
        {
            try
            {
                LadeDatenbank(Datenordner);
            }
            catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
            {
                // Sehr unwahrscheinlich, weil die Sicherung vorher geprüft wurde; ein Neustart lädt den Stand ebenfalls
                MessageBox.Show($"Die wiederhergestellten Daten konnten nicht geladen werden:\n\n{fehler.Message}\n\n" +
                    "Bitte starten Sie AndyTours neu.", "Sicherung wiederherstellen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    /// <summary>
    /// Ablauf „Datenordner ändern“ aus den Einstellungen: Ordner wählen und prüfen lassen
    /// (inkl. Rückfragen und ggf. Kopieren), dann die Daten des neuen Ordners laden und ihn merken.
    /// </summary>
    /// <returns>Der neue Datenordner oder <c>null</c>, wenn der bisherige aktiv bleibt.</returns>
    private async Task<string?> WechsleZuNeuemDatenordner()
    {
        string? neu = await WechsleDatenordner.Ausfuehren(Datenordner, kopierenAnbieten: true);
        if (neu is null)
        {
            return null;
        }

        try
        {
            LadeDatenbank(neu);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Daten im Ordner\n{neu}\nkonnten nicht geladen werden:\n\n{fehler.Message}\n\nDer bisherige Datenordner bleibt aktiv.",
                "Datenordner ändern", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        // Erst nach erfolgreichem Laden merken – sonst würde die App beim nächsten Start mit einem defekten Ordner beginnen
        MerkeDatenordner.Ausfuehren(Programmpfade.Merkdatei, neu);
        return neu;
    }

    /// <summary>
    /// Befehl "Datei → Beenden": beendet die Anwendung.
    /// </summary>
    [RelayCommand]
    private void Beenden()
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Befehl "Hilfe → Über AndyTours": zeigt Programmname und Version an.
    /// </summary>
    [RelayCommand]
    private void Ueber()
    {
        MessageBox.Show(
            $"AndyTours {Programmversion}\n\nTourenverwaltung für Radltouren.\n" +
            "Neuentwicklung als Nachfolger von BikeXperience.",
            "Über AndyTours",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

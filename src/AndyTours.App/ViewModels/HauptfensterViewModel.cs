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

    /// <summary>Text, der links in der Statusleiste angezeigt wird.</summary>
    [ObservableProperty]
    private string _statustext = string.Empty;

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Jahresauswahl ändert.</summary>
    partial void OnAusgewaehltesJahrChanged(string value) => AktualisiereStatus();

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Aktivitätsauswahl ändert.</summary>
    partial void OnAusgewaehlteAktivitaetChanged(string value) => AktualisiereStatus();

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

        // Jahresliste: alle Jahre mit Einheiten, das neueste zuerst
        Jahre.Clear();
        Jahre.Add(AlleJahre);
        foreach (int jahr in _bestand.Trainingseinheiten.Select(e => e.Datum.Year).Distinct().OrderDescending())
        {
            Jahre.Add(jahr.ToString(CultureInfo.InvariantCulture));
        }

        Aktivitaeten.Clear();
        Aktivitaeten.Add(AlleAktivitaeten);
        foreach (Aktivitaet aktivitaet in _bestand.Aktivitaeten)
        {
            Aktivitaeten.Add(aktivitaet.Name);
        }

        // Vorauswahl wie im Original: neuestes Jahr und Hauptaktivität
        AusgewaehltesJahr = Jahre.Count > 1 ? Jahre[1] : AlleJahre;
        AusgewaehlteAktivitaet = _bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Name ?? AlleAktivitaeten;
        AktualisiereStatus();
    }

    /// <summary>
    /// Zeigt in der Statusleiste die Summen für die aktuelle Jahres- und Aktivitätsauswahl.
    /// </summary>
    private void AktualisiereStatus()
    {
        if (_bestand.Trainingseinheiten.Count == 0)
        {
            Statustext = "Keine Trainingseinheiten im Datenordner.";
            return;
        }

        // Einheiten nach der Auswahl in der Werkzeugleiste filtern
        IEnumerable<Trainingseinheit> auswahl = _bestand.Trainingseinheiten;
        Aktivitaet? aktivitaet = _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet);
        if (aktivitaet is not null)
        {
            auswahl = auswahl.Where(e => e.AktivitaetId == aktivitaet.Id);
        }

        if (int.TryParse(AusgewaehltesJahr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int jahr))
        {
            auswahl = auswahl.Where(e => e.Datum.Year == jahr);
        }

        List<Jahressumme> summen = BerechneJahressummen.Ausfuehren(auswahl);
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        Statustext = string.Format(de, "{0} Trainingseinheiten in der Datenbank · Auswahl: {1} Einheiten, {2:N2} km, {3:N0} hm",
            _bestand.Trainingseinheiten.Count, summen.Sum(s => s.Anzahl), summen.Sum(s => s.DistanzKm), summen.Sum(s => s.Hoehenmeter));
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

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using AndyTours.App.Funktionen;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Import.Altdaten;

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

    /// <summary>Der geladene Datenbestand; <c>null</c>, solange keine Datenbank geöffnet ist.</summary>
    private Datenbestand? _bestand;

    /// <summary>
    /// Erzeugt das ViewModel und öffnet – falls bekannt – die Datenbank des zuletzt verwendeten Datenordners.
    /// </summary>
    public HauptfensterViewModel()
    {
        Jahre = new ObservableCollection<string> { AlleJahre };
        Aktivitaeten = new ObservableCollection<string> { AlleAktivitaeten };
        AusgewaehltesJahr = AlleJahre;
        AusgewaehlteAktivitaet = AlleAktivitaeten;
        Statustext = "Keine Datenbank geöffnet – bitte „Datei → Altdaten aus BikeXperience übernehmen…“ wählen.";

        string? ordner = LeseGemerktenDatenordner.Ausfuehren();
        if (ordner is not null && File.Exists(Path.Combine(ordner, FuehreAltdatenuebernahmeDurch.Datenbankdatei)))
        {
            try
            {
                LadeDatenbank(ordner);
            }
            catch (Exception fehler)
            {
                // Beim Start keinen Dialog zeigen (das Fenster ist noch nicht sichtbar), nur Statusmeldung
                Statustext = $"Datenbank konnte nicht geöffnet werden: {fehler.Message}";
            }
        }
    }

    /// <summary>Fenstertitel inklusive Programmversion.</summary>
    public string Titel => $"AndyTours {Programmversion}";

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

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Jahresauswahl ändert.</summary>
    partial void OnAusgewaehltesJahrChanged(string value) => AktualisiereStatus();

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Aktivitätsauswahl ändert.</summary>
    partial void OnAusgewaehlteAktivitaetChanged(string value) => AktualisiereStatus();

    /// <summary>
    /// Öffnet die Datenbank im angegebenen Datenordner und füllt die Auswahllisten.
    /// </summary>
    /// <param name="ordner">Datenordner mit der Datei <c>andytours.db</c>.</param>
    private void LadeDatenbank(string ordner)
    {
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(ordner, FuehreAltdatenuebernahmeDurch.Datenbankdatei)))
        {
            _bestand = LadeDatenbestand.Ausfuehren(verbindung);
        }

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
        if (_bestand is null)
        {
            return;
        }

        // Einheiten nach der Auswahl in der Werkzeugleiste filtern
        IEnumerable<Trainingseinheit> auswahl = _bestand.Trainingseinheiten;
        Aktivitaet? aktivitaet = _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet);
        if (aktivitaet is not null)
        {
            auswahl = auswahl.Where(e => e.AktivitaetId == aktivitaet.Id);
        }

        if (int.TryParse(AusgewaehltesJahr, out int jahr))
        {
            auswahl = auswahl.Where(e => e.Datum.Year == jahr);
        }

        List<Jahressumme> summen = BerechneJahressummen.Ausfuehren(auswahl);
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        Statustext = string.Format(de, "{0} Trainingseinheiten in der Datenbank · Auswahl: {1} Einheiten, {2:N2} km, {3:N0} hm",
            _bestand.Trainingseinheiten.Count, summen.Sum(s => s.Anzahl), summen.Sum(s => s.DistanzKm), summen.Sum(s => s.Hoehenmeter));
    }

    /// <summary>
    /// Befehl „Datei → Altdaten aus BikeXperience übernehmen…": liest den gewählten
    /// BikeXperience-Datenordner ein und legt daraus die AndyTours-Datenbank an.
    /// </summary>
    [RelayCommand]
    private void AltdatenUebernehmen()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "BikeXperience-Datenordner wählen (mit workouts.csv und bikexperience.ini)",
            InitialDirectory = LeseGemerktenDatenordner.Ausfuehren() ?? string.Empty,
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        string ordner = dialog.FolderName;
        if (!File.Exists(Path.Combine(ordner, "workouts.csv")))
        {
            MessageBox.Show("Im gewählten Ordner gibt es keine workouts.csv.", "Altdaten übernehmen",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Eine vorhandene Datenbank würde ersetzt – das muss ausdrücklich bestätigt werden.
        if (File.Exists(Path.Combine(ordner, FuehreAltdatenuebernahmeDurch.Datenbankdatei)))
        {
            MessageBoxResult antwort = MessageBox.Show(
                $"Im Ordner gibt es bereits eine AndyTours-Datenbank ({FuehreAltdatenuebernahmeDurch.Datenbankdatei}).\n\n" +
                "Soll sie durch eine neue Übernahme der Altdaten ersetzt werden? Änderungen, die seitdem in AndyTours gemacht wurden, gehen dabei verloren.",
                "Altdaten übernehmen", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (antwort != MessageBoxResult.Yes)
            {
                return;
            }
        }

        try
        {
            Migrationsergebnis ergebnis = FuehreAltdatenuebernahmeDurch.Ausfuehren(ordner);
            MerkeDatenordner.Ausfuehren(ordner);
            LadeDatenbank(ordner);

            Migrationsbericht bericht = ergebnis.Bericht;
            MessageBox.Show(
                $"Übernommen: {ergebnis.Bestand.Trainingseinheiten.Count} Trainingseinheiten, " +
                $"{ergebnis.Bestand.Equipments.Count} Equipments, {ergebnis.Bestand.Wartungen.Count} Wartungen.\n\n" +
                $"Hinweise: {bericht.Hinweise.Count}\nWarnungen: {bericht.Warnungen.Count}\n" +
                $"Nicht reparierte Umlaute: {bericht.NichtReparierteTexte.Count}\n\n" +
                $"Der vollständige Bericht steht in:\n{Path.Combine(ordner, FuehreAltdatenuebernahmeDurch.Berichtsdatei)}",
                "Altdaten übernommen", MessageBoxButton.OK,
                bericht.Warnungen.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception fehler)
        {
            MessageBox.Show($"Die Übernahme ist fehlgeschlagen:\n\n{fehler.Message}", "Altdaten übernehmen",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

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

        // Jede Änderung der Filterleiste baut nur die Tabelle neu auf; die Auswahllisten bleiben, wie sie sind
        Filter.Geaendert += (_, _) => AktualisiereSummen(listenNeu: false);

        LadeDatenbank(datenordner);
    }

    /// <summary>Die Filterleiste über der Tabelle „Trainingseinheiten“.</summary>
    public EinheitenfilterViewModel Filter { get; } = new();

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

    /// <summary>Zeilen der Tabelle „Trainingseinheiten“: die Einheiten der aktuellen Auswahl.</summary>
    /// <remarks>
    /// Die Sammlung bleibt dieselbe und wird nur neu gefüllt – so behält die Tabelle eine per Klick
    /// gewählte Sortierung beim Wechsel der Auswahl.
    /// </remarks>
    public ObservableCollection<Einheitszeile> Einheitszeilen { get; } = new();

    /// <summary>
    /// <c>true</c>, wenn die Tabelle nach Auswahl und Filter Einheiten enthält; sonst zeigt die Karteikarte
    /// den <see cref="Hinweistext"/>.
    /// </summary>
    [ObservableProperty]
    private bool _hatEinheiten;

    /// <summary>
    /// Hinweis bei leerer Tabelle: „Keine Einheiten in dieser Auswahl.“, wenn schon Jahr und Aktivität nichts
    /// liefern, sonst „Keine Einheiten passen zum Filter.“.
    /// </summary>
    [ObservableProperty]
    private string _hinweistext = string.Empty;

    /// <summary>Summenzeile: Anzahl der angezeigten Einheiten (unter der Spalte „Name“).</summary>
    [ObservableProperty]
    private string _summeAnzahl = string.Empty;

    /// <summary>Summenzeile: Summe der Distanzen in km.</summary>
    [ObservableProperty]
    private string _summeDistanz = string.Empty;

    /// <summary>Summenzeile: Summe der Höhenmeter.</summary>
    [ObservableProperty]
    private string _summeHoehenmeter = string.Empty;

    /// <summary>Summenzeile: Summe der Fahrzeiten („h:mm:ss“, auch über 24 Stunden).</summary>
    [ObservableProperty]
    private string _summeFahrzeit = string.Empty;

    /// <summary>
    /// In der Tabelle ausgeblendete Spalten; gespeichert in der Datenbank des Datenordners.
    /// </summary>
    /// <remarks>
    /// Die Tabelle beobachtet diese Eigenschaft und blendet ihre Spalten entsprechend ein oder aus.
    /// Bei jeder Änderung wird eine neue Menge zugewiesen, damit die Änderungsmeldung ausgelöst wird.
    /// </remarks>
    [ObservableProperty]
    private IReadOnlySet<Einheitenspalte> _ausgeblendeteSpalten = new HashSet<Einheitenspalte>();

    /// <summary>
    /// Reihenfolge und Breiten der Tabellenspalten, wie sie beim Laden des Datenbestands gelesen wurden.
    /// </summary>
    /// <remarks>
    /// Die Tabelle wendet die Anordnung an, sobald sich diese Eigenschaft ändert (Programmstart,
    /// Datenordnerwechsel). Änderungen durch den Benutzer meldet die Tabelle über
    /// <see cref="MerkeSpaltenanordnung"/> zurück.
    /// </remarks>
    [ObservableProperty]
    private IReadOnlyList<Spalteneinstellung> _spaltenanordnung = VervollstaendigeSpaltenanordnung.Ausfuehren([]);

    /// <summary>
    /// Beim Start gelesene Größe, Position und Zustand des Hauptfensters; <c>null</c>, wenn noch nichts gemerkt ist.
    /// </summary>
    public Fensterposition? GemerkteFensterposition { get; private set; }

    /// <summary>
    /// Beim Start gelesene Nummer der zuletzt gewählten Karteikarte; <c>null</c>, wenn noch nichts gemerkt ist.
    /// </summary>
    public int? GemerkteKarteikarte { get; private set; }

    /// <summary>
    /// Speichert Reihenfolge und Breiten der Tabellenspalten in der Datenbank des Datenordners.
    /// </summary>
    /// <remarks>
    /// Wird von der Tabelle nach dem Verschieben oder Verbreitern einer Spalte und beim Beenden aufgerufen.
    /// Die Eigenschaft <see cref="Spaltenanordnung"/> bleibt dabei unverändert, damit die Tabelle ihre
    /// gerade gezeigte Anordnung nicht erneut anwendet.
    /// </remarks>
    /// <param name="anordnung">Die Spalten in Anzeigereihenfolge.</param>
    public void MerkeSpaltenanordnung(IReadOnlyList<Spalteneinstellung> anordnung) =>
        SpeichereStill(verbindung => SpaltenanordnungEinstellung.Speichern(verbindung, anordnung));

    /// <summary>Speichert Größe, Position und Zustand des Hauptfensters (beim Schließen aufgerufen).</summary>
    /// <param name="position">Die zu merkende Position.</param>
    public void MerkeFensterposition(Fensterposition position) =>
        SpeichereStill(verbindung => FensterpositionEinstellung.Speichern(verbindung, position));

    /// <summary>Speichert die gewählte Karteikarte des Hauptfensters (beim Schließen aufgerufen).</summary>
    /// <param name="nummer">Index der Karteikarte, ab 0.</param>
    public void MerkeKarteikarte(int nummer) =>
        SpeichereStill(verbindung => KarteikarteEinstellung.Speichern(verbindung, nummer));

    /// <summary>
    /// Schreibt einen Komfortwert in die Datenbank des Datenordners; Fehler werden ohne Meldung übergangen.
    /// </summary>
    /// <remarks>
    /// Spaltenanordnung und Fensterposition sind reine Bequemlichkeit. Eine Fehlermeldung, etwa beim
    /// Beenden mit ausgeschaltetem NAS, würde nur stören – schlimmstenfalls gilt beim nächsten Start
    /// die vorherige Anordnung.
    /// </remarks>
    private void SpeichereStill(Action<SqliteConnection> speichern)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei));
            speichern(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            // bewusst ohne Meldung, siehe oben
        }
    }

    /// <summary>
    /// Befehl aus dem Kontextmenü der Spaltenüberschriften: blendet eine Spalte ein oder aus und speichert die Wahl sofort.
    /// </summary>
    /// <remarks>
    /// Die letzte sichtbare Spalte bleibt sichtbar (<see cref="SchalteSpalteUm"/>). Lässt sich die Wahl
    /// nicht speichern, erscheint eine Meldung und die Tabelle bleibt, wie sie war.
    /// </remarks>
    /// <param name="spalte">Die umzuschaltende Spalte.</param>
    [RelayCommand]
    private void SpalteUmschalten(Einheitenspalte spalte)
    {
        HashSet<Einheitenspalte> neu = SchalteSpalteUm.Ausfuehren(AusgeblendeteSpalten, spalte);
        if (neu.SetEquals(AusgeblendeteSpalten))
        {
            return; // letzte sichtbare Spalte – nichts zu tun
        }

        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei));
            AusgeblendeteSpaltenEinstellung.Speichern(verbindung, neu);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Spaltenauswahl konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Trainingseinheiten", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        AusgeblendeteSpalten = neu;
    }

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Jahresauswahl ändert.</summary>
    /// <remarks>Während die Jahresliste neu gefüllt wird, wird nichts neu aufgebaut (siehe <see cref="AktualisiereJahre"/>).</remarks>
    partial void OnAusgewaehltesJahrChanged(string value)
    {
        if (!_jahreWerdenGefuellt)
        {
            AktualisiereSummen();
        }
    }

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald sich die Aktivitätsauswahl ändert.</summary>
    /// <remarks>Während die Aktivitätsliste neu gefüllt wird, wird nichts neu aufgebaut (siehe <see cref="FuelleAktivitaeten"/>).</remarks>
    partial void OnAusgewaehlteAktivitaetChanged(string value)
    {
        if (!_aktivitaetenWerdenGefuellt)
        {
            AktualisiereSummen();
        }
    }

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
        HashSet<Einheitenspalte> ausgeblendet;
        List<Spalteneinstellung> anordnung;
        Fensterposition? fenster;
        int? karteikarte;
        List<string> trainingsbereiche;
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(ordner, DatenordnerInhalt.Datenbankdatei)))
        {
            geladen = DatenbestandTabellen.Laden(verbindung);
            geladen.Aktivitaeten = OrdneAktivitaeten.Ausfuehren(geladen.Aktivitaeten, AktivitaetenReihenfolgeEinstellung.Laden(verbindung));
            trainingsbereiche = LadeTrainingsbereiche(verbindung, geladen);
            ausgeblendet = AusgeblendeteSpaltenEinstellung.Laden(verbindung);
            anordnung = SpaltenanordnungEinstellung.Laden(verbindung);
            fenster = FensterpositionEinstellung.Laden(verbindung);
            karteikarte = KarteikarteEinstellung.Laden(verbindung);
        }

        _bestand = geladen;
        _trainingsbereiche = trainingsbereiche;
        Datenordner = ordner;
        Spaltenanordnung = anordnung;
        AusgeblendeteSpalten = ausgeblendet;
        GemerkteFensterposition = fenster;
        GemerkteKarteikarte = karteikarte;

        // Jahresliste: „alle Jahre“, dann alle Jahre mit Einheiten und immer das aktuelle Jahr, das neueste zuerst
        int aktuellesJahr = DateTime.Today.Year;
        Jahre.Clear();
        Jahre.Add(AlleJahre);
        foreach (int jahr in ErmittleAuswahljahre.Ausfuehren(_bestand.Trainingseinheiten, aktuellesJahr))
        {
            Jahre.Add(jahr.ToString(CultureInfo.InvariantCulture));
        }

        // Vorauswahl: aktuelles Jahr (steht immer in der Liste) und Hauptaktivität; ohne Hauptaktivität alle Aktivitäten
        AusgewaehltesJahr = aktuellesJahr.ToString(CultureInfo.InvariantCulture);
        FuelleAktivitaeten(_bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id);

        AktualisiereStatustext();
        AktualisiereSummen();
    }

    /// <summary>
    /// Berechnet die Summen der aktuellen Jahres- und Aktivitätsauswahl und zeigt sie neben der Auswahl an;
    /// füllt außerdem die Tabelle „Trainingseinheiten“ und ihre Summenzeile.
    /// </summary>
    /// <remarks>
    /// Wird bei jeder Änderung der Auswahl und nach jedem Laden des Datenbestands aufgerufen
    /// (Programmstart, Datenordnerwechsel, Wiederherstellen einer Sicherung). Die Einträge „alle Jahre“
    /// und „alle Aktivitäten“ werden als „keine Einschränkung“ (<c>null</c>) an
    /// <see cref="FiltereAuswahl"/> übergeben.
    /// <para>
    /// Danach wirken die Filter der Filterleiste (<see cref="FiltereEinheiten"/>): Tabelle, Bestwerte und
    /// Summenzeile zeigen nur die gefilterten Einheiten. Die Summe in der Werkzeugleiste bleibt bei der
    /// ungefilterten Auswahl, damit beide Zahlen nebeneinander sichtbar sind. Die Filter bleiben bei einem
    /// Wechsel der Auswahl erhalten und wirken auf die neue Auswahl.
    /// </para>
    /// </remarks>
    /// <param name="listenNeu">
    /// <c>true</c> (Auswahl oder Datenbestand geändert): die Auswahllisten der Filterleiste neu füllen;
    /// <c>false</c> (nur ein Filter geändert): die Listen bleiben, wie sie sind.
    /// </param>
    private void AktualisiereSummen(bool listenNeu = true)
    {
        // Eine Meldung wie „Gespeichert – liegt außerhalb …“ gilt nur bis zur nächsten Änderung der Anzeige
        Statusmeldung = string.Empty;

        // Auswahl aus der Werkzeugleiste in Filterwerte übersetzen
        int? jahr = int.TryParse(AusgewaehltesJahr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int j) ? j : null;
        long? aktivitaetId = _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet)?.Id;
        List<Trainingseinheit> auswahl = FiltereAuswahl.Ausfuehren(_bestand.Trainingseinheiten, jahr, aktivitaetId);

        // Auswahllisten der Filterleiste: nur Werte der Auswahl, die gewählte Eintragung bleibt erhalten
        if (listenNeu)
        {
            Filter.UebernimmListen(ErmittleFilterwerte.Ausfuehren(auswahl, _bestand, Filter.ErstelleFilter()));
        }

        // Filter der Filterleiste anwenden
        List<Trainingseinheit> angezeigt = FiltereEinheiten.Ausfuehren(auswahl, Filter.ErstelleFilter(), _bestand);

        // Tabelle neu füllen (dieselbe Sammlung, damit die gewählte Sortierung erhalten bleibt);
        // die Bestwerte werden dabei aus den angezeigten Einheiten bestimmt
        Einheitszeilen.Clear();
        foreach (Einheitszeile zeile in ErstelleEinheitszeilen.Ausfuehren(angezeigt, _bestand))
        {
            Einheitszeilen.Add(zeile);
        }

        HatEinheiten = angezeigt.Count > 0;
        Hinweistext = auswahl.Count == 0 ? "Keine Einheiten in dieser Auswahl." : "Keine Einheiten passen zum Filter.";

        // Summenzeile der Tabelle: nur die angezeigten Einheiten
        CultureInfo deutsch = CultureInfo.GetCultureInfo("de-DE");
        Auswahlsumme gefiltert = BerechneAuswahlsummen.Ausfuehren(angezeigt);
        SummeAnzahl = string.Format(deutsch, "{0:N0} {1}", gefiltert.Anzahl, gefiltert.Anzahl == 1 ? "Einheit" : "Einheiten");
        SummeDistanz = gefiltert.DistanzKm.ToString("N2", deutsch);
        SummeHoehenmeter = gefiltert.Hoehenmeter.ToString("N0", deutsch);
        SummeFahrzeit = FormatiereDauer.Ausfuehren(gefiltert.Fahrzeit);

        // Werkzeugleiste: die ganze Jahres- und Aktivitätsauswahl, ohne Filter
        Auswahlsumme summe = BerechneAuswahlsummen.Ausfuehren(auswahl);
        Summentext = _bestand.Trainingseinheiten.Count == 0
            ? "Noch keine Touren vorhanden."
            : string.Format(deutsch, "{0:N0} Einheiten · {1:N2} km · {2:N0} hm", summe.Anzahl, summe.DistanzKm, summe.Hoehenmeter);
    }

    /// <summary>
    /// Befehl „Extras → Einstellungen…“: öffnet das Einstellungsfenster.
    /// </summary>
    [RelayCommand]
    private void EinstellungenOeffnen()
    {
        var viewModel = new EinstellungenViewModel(Datenordner, !Programmpfade.IstEntwicklung, WechsleZuNeuemDatenordner,
            ErstelleAuswahllistenViewModel());
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
        DatenordnerMerkdatei.Speichern(Programmpfade.Merkdatei, neu);
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

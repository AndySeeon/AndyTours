using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using AndyTours.App.Funktionen;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Daten.Datensicherung;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Einstellungsfensters.
/// </summary>
/// <remarks>
/// Es enthält fünf Bereiche:
/// <list type="bullet">
/// <item><b>Datenordner:</b> Der eigentliche Wechsel (Ordnerwahl, Rückfragen, Kopieren, Laden, Merken)
/// wird vom Hauptfenster-ViewModel als Funktion übergeben, weil dieses den Datenbestand hält und neu
/// laden muss.</item>
/// <item><b>Datensicherung:</b> Sicherungsordner, Intervall und Höchstzahl. Sie liegen in der Datenbank
/// des aktuellen Datenordners und werden sofort gespeichert, sobald ein gültiger Wert eingegeben bzw.
/// ein Ordner gewählt ist. Nach einem Wechsel des Datenordners werden die Werte des neuen Ordners
/// angezeigt.</item>
/// <item><b>Routing (Tourenplanung):</b> Routingdienst, BRouter-Adresse und Zugangsschlüssel für openrouteservice
/// (siehe <c>docs/adr/0004-routingdienst.md</c>). Auch sie liegen in der Datenbank des Datenordners und werden
/// sofort gespeichert, die Adresse nur, wenn sie gültig ist. Der Schlüssel steht in einem maskierten Feld, das das
/// Code-Behind füllt und meldet (ein Passwortfeld lässt sich nicht binden).</item>
/// <item><b>Overlays:</b> die gemeinsame Overlay-Farbe; sie wird sofort gespeichert und allen offenen Karten gemeldet
/// (<see cref="Overlaybestand"/>).</item>
/// <item><b>Auswahllisten:</b> eigenes ViewModel (<see cref="AuswahllistenViewModel"/>), das das
/// Hauptfenster-ViewModel erzeugt, weil Änderungen den Datenbestand betreffen.</item>
/// </list>
/// </remarks>
public partial class EinstellungenViewModel : ObservableObject
{
    /// <summary>Erklärung, warum der Datenordner in der Entwicklungsversion nicht änderbar ist.</summary>
    public const string Entwicklungshinweis =
        "Diese Entwicklungsversion verwendet immer den Ordner „AppDaten“ im Projektordner, damit Ihre " +
        "echten Daten unberührt bleiben. Der Datenordner lässt sich nur in der fertigen Programmversion ändern.";

    /// <summary>Hinweis bei einer ungültigen Eingabe für Intervall oder Höchstzahl.</summary>
    public static readonly string Bereichshinweis = string.Create(CultureInfo.InvariantCulture,
        $"Bitte eine ganze Zahl von {Sicherungseinstellungen.Minimalwert} bis {Sicherungseinstellungen.Maximalwert} eingeben.");

    /// <summary>Hinweis bei einer ungültigen Adresse des BRouter-Servers.</summary>
    public const string UngueltigeAdresse = "Bitte eine vollständige Adresse eingeben, z. B. https://brouter.de/brouter.";

    /// <summary>Hinweis, wenn openrouteservice ohne Zugangsschlüssel gewählt ist.</summary>
    public const string Schluesselhinweis =
        "Ohne Zugangsschlüssel kann openrouteservice keine Strecken ermitteln. Einen kostenlosen Schlüssel gibt es nach " +
        "einer Anmeldung auf openrouteservice.org.";

    /// <summary>Führt den Ordnerwechsel durch; liefert den neuen Ordner oder <c>null</c>.</summary>
    private readonly Func<Task<string?>> _ordnerWechseln;

    /// <summary><c>true</c>, solange gespeicherte Werte in die Felder geschrieben werden (dann nicht speichern).</summary>
    private bool _wirdAngezeigt;

    /// <summary>
    /// Erzeugt das ViewModel und liest die Sicherungseinstellungen des Datenordners.
    /// </summary>
    /// <param name="datenordner">Der aktuelle Datenordner.</param>
    /// <param name="aendernMoeglich"><c>false</c> in der Entwicklungsversion.</param>
    /// <param name="ordnerWechseln">Ablauf des Ordnerwechsels (vom Hauptfenster-ViewModel).</param>
    /// <param name="auswahllisten">ViewModel des Bereichs „Auswahllisten“ (vom Hauptfenster-ViewModel).</param>
    public EinstellungenViewModel(string datenordner, bool aendernMoeglich, Func<Task<string?>> ordnerWechseln,
        AuswahllistenViewModel auswahllisten)
    {
        _datenordner = datenordner;
        AendernMoeglich = aendernMoeglich;
        _ordnerWechseln = ordnerWechseln;
        Auswahllisten = auswahllisten;
        ZeigeSicherungseinstellungen();
        ZeigeRoutingeinstellungen();
        ZeigeOverlayfarbe();
        ZeigeOberflaechengroesse();
    }

    /// <summary>
    /// Wird ausgelöst, wenn der Zugangsschlüssel neu gelesen wurde (nach einem Wechsel des Datenordners), damit das
    /// Code-Behind das maskierte Feld neu füllt.
    /// </summary>
    public event EventHandler? SchluesselNeuGelesen;

    /// <summary>Bereich „Auswahllisten“: gepflegte Listen mit festen Werten (zunächst die Trainingsbereiche).</summary>
    public AuswahllistenViewModel Auswahllisten { get; }

    /// <summary>Vollständiger Pfad des aktuellen Datenordners.</summary>
    [ObservableProperty]
    private string _datenordner;

    /// <summary><c>true</c>, wenn der Datenordner geändert werden darf (nur in der fertigen Programmversion).</summary>
    public bool AendernMoeglich { get; }

    /// <summary><c>true</c>, wenn der <see cref="Entwicklungshinweis"/> angezeigt werden soll.</summary>
    public bool HinweisSichtbar => !AendernMoeglich;

    /// <summary>Text des Hinweises (für die Datenbindung).</summary>
    public string Hinweis => Entwicklungshinweis;

    /// <summary>Vollständiger Pfad des Sicherungsordners (gewählt oder Standard).</summary>
    [ObservableProperty]
    private string _sicherungsordner = string.Empty;

    /// <summary>Eingabefeld „Sichern nach … Programmstarts“.</summary>
    [ObservableProperty]
    private string _intervallText = string.Empty;

    /// <summary>Eingabefeld „Höchstens … Sicherungen aufbewahren“.</summary>
    [ObservableProperty]
    private string _hoechstzahlText = string.Empty;

    /// <summary>Hinweis unter dem Intervall-Feld; leer, solange die Eingabe gültig ist.</summary>
    [ObservableProperty]
    private string _intervallHinweis = string.Empty;

    /// <summary>Hinweis unter dem Höchstzahl-Feld; leer, solange die Eingabe gültig ist.</summary>
    [ObservableProperty]
    private string _hoechstzahlHinweis = string.Empty;

    /// <summary>
    /// Befehl „Ändern…“ beim Datenordner: startet den Ordnerwechsel und zeigt danach den neuen Ordner
    /// und dessen Sicherungseinstellungen an.
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
            ZeigeSicherungseinstellungen();
            ZeigeRoutingeinstellungen();
            ZeigeOverlayfarbe();
            ZeigeOberflaechengroesse();
            SchluesselNeuGelesen?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Befehl „Ändern…“ beim Sicherungsordner: Ordner wählen und sofort speichern.
    /// </summary>
    /// <remarks>
    /// Der Ordner wird hier nicht geprüft; ist er später nicht erreichbar oder nicht beschreibbar,
    /// meldet das die Sicherung beim Programmstart. Anders als der Datenordner ist der
    /// Sicherungsordner auch in der Entwicklungsversion änderbar.
    /// </remarks>
    [RelayCommand]
    private void SicherungsordnerAendern()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Ordner für die Datensicherung wählen",
            InitialDirectory = Directory.Exists(Sicherungsordner) ? Sicherungsordner : string.Empty,
        };
        Window? besitzer = Application.Current?.Windows.OfType<Window>().FirstOrDefault(f => f.IsActive);
        bool? gewaehlt = besitzer is null ? dialog.ShowDialog() : dialog.ShowDialog(besitzer);
        if (gewaehlt != true)
        {
            return; // Auswahl abgebrochen – bisheriger Ordner bleibt
        }

        string ordner = Path.GetFullPath(dialog.FolderName);
        if (Speichere(e => e with { Sicherungsordner = ordner }))
        {
            Sicherungsordner = ordner;
        }
    }

    /// <summary>Wird bei jeder Änderung im Intervall-Feld aufgerufen: gültige Werte sofort speichern.</summary>
    partial void OnIntervallTextChanged(string value)
    {
        int? wert = PruefeSicherungswert.Ausfuehren(value);
        IntervallHinweis = wert is null ? Bereichshinweis : string.Empty;
        if (!_wirdAngezeigt && wert is int intervall)
        {
            Speichere(e => e with { Intervall = intervall });
        }
    }

    /// <summary>Wird bei jeder Änderung im Höchstzahl-Feld aufgerufen: gültige Werte sofort speichern.</summary>
    partial void OnHoechstzahlTextChanged(string value)
    {
        int? wert = PruefeSicherungswert.Ausfuehren(value);
        HoechstzahlHinweis = wert is null ? Bereichshinweis : string.Empty;
        if (!_wirdAngezeigt && wert is int hoechstzahl)
        {
            Speichere(e => e with { Hoechstzahl = hoechstzahl });
        }
    }

    /// <summary>
    /// Liest die Sicherungseinstellungen des aktuellen Datenordners und zeigt sie an.
    /// </summary>
    /// <remarks>
    /// Während des Anzeigens ist <see cref="_wirdAngezeigt"/> gesetzt, damit das Füllen der Felder
    /// nicht gleich wieder ein Speichern auslöst. Ist die Datenbank nicht lesbar, werden die
    /// Standardwerte angezeigt.
    /// </remarks>
    private void ZeigeSicherungseinstellungen()
    {
        Sicherungseinstellungen einstellungen;
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            einstellungen = SicherungEinstellungen.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            einstellungen = new Sicherungseinstellungen();
        }

        _wirdAngezeigt = true;
        try
        {
            Sicherungsordner = ErmittleSicherungsordner.Ausfuehren(Datenordner, einstellungen);
            IntervallText = einstellungen.Intervall.ToString(CultureInfo.InvariantCulture);
            HoechstzahlText = einstellungen.Hoechstzahl.ToString(CultureInfo.InvariantCulture);
            IntervallHinweis = string.Empty;
            HoechstzahlHinweis = string.Empty;
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>
    /// Liest die aktuellen Einstellungen, ändert sie und speichert sie wieder.
    /// </summary>
    /// <remarks>
    /// Es wird jedes Mal frisch gelesen, damit der Startzähler unverändert bleibt.
    /// </remarks>
    /// <param name="aenderung">Die Änderung, z. B. <c>e =&gt; e with { Intervall = 5 }</c>.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool Speichere(Func<Sicherungseinstellungen, Sicherungseinstellungen> aenderung)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            SicherungEinstellungen.Speichern(verbindung, aenderung(SicherungEinstellungen.Laden(verbindung)));
            return true;
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Einstellung konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>Routingdienst „BRouter“ gewählt (Optionsfeld); sonst openrouteservice.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RoutingHinweis), nameof(IstOpenrouteservice))]
    private bool _istBRouter = true;

    /// <summary>Routingdienst „openrouteservice“ gewählt (zweites Optionsfeld).</summary>
    public bool IstOpenrouteservice
    {
        get => !IstBRouter;
        set => IstBRouter = !value;
    }

    /// <summary>Adresse des BRouter-Servers (Eingabefeld).</summary>
    [ObservableProperty]
    private string _bRouterAdresse = Routingeinstellungen.StandardBRouterAdresse;

    /// <summary>Hinweis unter der Adresse; leer, solange sie gültig ist.</summary>
    [ObservableProperty]
    private string _adressHinweis = string.Empty;

    /// <summary>Gespeicherter Zugangsschlüssel für openrouteservice; nur für das maskierte Feld, nie anzeigen.</summary>
    public string Schluessel => _routing.OpenrouteserviceSchluessel ?? string.Empty;

    /// <summary>Hinweis unter dem Zugangsschlüssel, wenn openrouteservice ohne Schlüssel gewählt ist.</summary>
    public string RoutingHinweis =>
        !IstBRouter && string.IsNullOrWhiteSpace(_routing.OpenrouteserviceSchluessel) ? Schluesselhinweis : string.Empty;

    /// <summary>Die zuletzt gelesenen bzw. gespeicherten Routingeinstellungen.</summary>
    private Routingeinstellungen _routing = Routingeinstellungen.Standard;

    /// <summary>
    /// Übernimmt einen im maskierten Feld geänderten Zugangsschlüssel und speichert ihn sofort.
    /// </summary>
    /// <param name="schluessel">Der Inhalt des Felds.</param>
    public void SchluesselGeaendert(string schluessel)
    {
        if (!_wirdAngezeigt && schluessel != Schluessel)
        {
            SpeichereRouting(_routing with { OpenrouteserviceSchluessel = string.IsNullOrWhiteSpace(schluessel) ? null : schluessel.Trim() });
            OnPropertyChanged(nameof(RoutingHinweis));
        }
    }

    /// <summary>Wird bei der Wahl des Dienstes aufgerufen: sofort speichern.</summary>
    partial void OnIstBRouterChanged(bool value)
    {
        if (!_wirdAngezeigt)
        {
            SpeichereRouting(_routing with { Dienst = value ? Routingdienst.BRouter : Routingdienst.Openrouteservice });
        }
    }

    /// <summary>Wird bei jeder Änderung der Adresse aufgerufen: eine gültige Adresse sofort speichern.</summary>
    partial void OnBRouterAdresseChanged(string value)
    {
        bool gueltig = Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? adresse)
            && (adresse.Scheme == Uri.UriSchemeHttps || adresse.Scheme == Uri.UriSchemeHttp);
        AdressHinweis = gueltig ? string.Empty : UngueltigeAdresse;
        if (!_wirdAngezeigt && gueltig)
        {
            SpeichereRouting(_routing with { BRouterAdresse = value.Trim() });
        }
    }

    /// <summary>
    /// Liest die Routingeinstellungen des aktuellen Datenordners und zeigt sie an; ist die Datenbank nicht lesbar,
    /// gelten die Standardwerte.
    /// </summary>
    private void ZeigeRoutingeinstellungen()
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            _routing = RoutingEinstellung.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            _routing = Routingeinstellungen.Standard;
        }

        _wirdAngezeigt = true;
        try
        {
            IstBRouter = _routing.Dienst == Routingdienst.BRouter;
            BRouterAdresse = _routing.BRouterAdresse;
            AdressHinweis = string.Empty;
            OnPropertyChanged(nameof(RoutingHinweis));
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>
    /// Speichert geänderte Routingeinstellungen; die Fehlermeldung nennt den Zugangsschlüssel nicht.
    /// </summary>
    /// <param name="neu">Die neuen Einstellungen.</param>
    private void SpeichereRouting(Routingeinstellungen neu)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            RoutingEinstellung.Speichern(verbindung, neu);
            _routing = neu;
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Einstellung konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Die wählbaren gemeinsamen Overlay-Farben.</summary>
    public IReadOnlyList<Overlayfarbeintrag> Overlayfarben { get; } = Overlayfarbeintrag.Alle();

    /// <summary>Die gewählte gemeinsame Overlay-Farbe (Auswahlfeld der Gruppe „Overlays“).</summary>
    [ObservableProperty]
    private Overlayfarbeintrag? _overlayfarbe;

    /// <summary>
    /// Wird bei der Wahl der gemeinsamen Overlay-Farbe aufgerufen: sofort speichern und den offenen Karten melden.
    /// </summary>
    partial void OnOverlayfarbeChanged(Overlayfarbeintrag? value)
    {
        if (_wirdAngezeigt || value?.Farbe is not Overlayfarbe farbe)
        {
            return;
        }

        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            OverlayfarbeEinstellung.Speichern(verbindung, farbe);
            Overlaybestand.SetzeFarbe(farbe);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Einstellung konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Zeigt die gemeinsame Overlay-Farbe des aktuellen Datenordners an (sie ist beim Laden des Datenordners bereits im
    /// <see cref="Overlaybestand"/>).
    /// </summary>
    private void ZeigeOverlayfarbe()
    {
        _wirdAngezeigt = true;
        try
        {
            Overlayfarbe = Overlayfarben.First(e => e.Farbe == Overlaybestand.Farbe);
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>Die wählbaren Stufen der Oberflächengröße in Prozent (Anzeige „100 %“ … „200 %“).</summary>
    public IReadOnlyList<int> Oberflaechenstufen { get; } = OberflaechengroesseEinstellung.Stufen;

    /// <summary>Die gewählte Größe der Oberfläche in Prozent (Auswahlfeld der Gruppe „Darstellung“).</summary>
    [ObservableProperty]
    private int _oberflaechengroesse = OberflaechengroesseEinstellung.Standard;

    /// <summary>
    /// Wird bei der Wahl einer Stufe aufgerufen: sofort speichern und alle offenen Fenster vergrößern bzw.
    /// verkleinern (Spezifikation 035).
    /// </summary>
    partial void OnOberflaechengroesseChanged(int value)
    {
        if (_wirdAngezeigt)
        {
            return;
        }

        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            OberflaechengroesseEinstellung.Speichern(verbindung, value);
            Oberflaechenskalierung.Setze(value);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Einstellung konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Zeigt die Größe der Oberfläche des aktuellen Datenordners an (sie ist beim Laden des Datenordners bereits
    /// gesetzt, siehe <see cref="Oberflaechenskalierung"/>).
    /// </summary>
    private void ZeigeOberflaechengroesse()
    {
        _wirdAngezeigt = true;
        try
        {
            Oberflaechengroesse = (int)Math.Round(Oberflaechenskalierung.Faktor * 100);
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>Vollständiger Pfad der Datenbank im aktuellen Datenordner.</summary>
    private string Datenbankdatei => Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei);
}

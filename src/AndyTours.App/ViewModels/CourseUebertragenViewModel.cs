using System.Globalization;
// WPF-Projekte binden System.IO nicht automatisch ein
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Funktionen;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Import.Garmin;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Auf Garmin übertragen“ (Spec 032): Namen der Courses, Fahrprofil für die Abbiegungen einer
/// gefahrenen Strecke, Wahl des Geräts und die Übertragung selbst.
/// </summary>
/// <remarks>
/// <list type="number">
/// <item>Fehlt ein Name oder kommt ein Dateiname doppelt vor, erscheint ein Hinweis.</item>
/// <item>Wartet auf dem Gerät schon eine Datei gleichen Namens, fragt das Fenster, ob sie ersetzt werden soll; bei
/// „Nein“ entfällt dieser Course.</item>
/// <item>Bei gefahrenen Strecken ermittelt der Routingdienst die Abbiegungen (<see cref="Abbiegungsabruf"/>); gelingt
/// das nicht, wird ohne Abbiegungen übertragen und das Ergebnis nennt den Grund.</item>
/// <item>Jeder Course wird als FIT-Datei geschrieben (<see cref="SchreibeFitCourse"/>) und in
/// <c>Garmin\NewFiles</c> abgelegt.</item>
/// </list>
/// Die Zugriffe auf das Gerät laufen im Fenster-Thread, weil die Mediengeräte dort angelegt wurden; die Dateien sind
/// nur wenige Kilobyte groß. Danach schließt sich das Fenster, und der Aufrufer zeigt <see cref="Ergebnis"/>. Scheitert das Schreiben, bleibt
/// das Fenster offen und zeigt den Grund. Planung und Einheiten werden nur gelesen, nie verändert.
/// </remarks>
public partial class CourseUebertragenViewModel : ObservableObject
{
    /// <summary>Titel für Rückfragen und Meldungen.</summary>
    public const string Titel = "Auf Garmin übertragen";

    /// <summary>Hinweis, wenn kein Garmin-Gerät angeschlossen ist (wie beim Import).</summary>
    public const string KeinGeraet = HauptfensterViewModel.KeinGeraet;

    /// <summary>Liest die Routingeinstellungen.</summary>
    private readonly Func<Routingeinstellungen> _ladeRouting;

    /// <summary>Bricht eine laufende Übertragung ab, wenn das Fenster geschlossen wird; sonst <c>null</c>.</summary>
    private CancellationTokenSource? _abbruch;

    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="zeilen">Die Courses, die übertragen werden sollen.</param>
    /// <param name="ziele">Die angeschlossenen Geräte; mindestens eines.</param>
    /// <param name="profil">Vorgewähltes Fahrprofil für die Abbiegungen gefahrener Strecken.</param>
    /// <param name="ladeRouting">Liest die Routingeinstellungen.</param>
    public CourseUebertragenViewModel(IReadOnlyList<Coursezeile> zeilen, IReadOnlyList<Courseziel> ziele, Fahrprofil profil,
        Func<Routingeinstellungen> ladeRouting)
    {
        Zeilen = zeilen;
        Ziele = ziele;
        _ziel = ziele[0];
        _profil = profil;
        _ladeRouting = ladeRouting;
    }

    /// <summary>Wird ausgelöst, wenn die Übertragung beendet ist; das Fenster schließt sich dann.</summary>
    public event EventHandler? Fertig;

    /// <summary>Die Courses mit ihren Namen.</summary>
    public IReadOnlyList<Coursezeile> Zeilen { get; }

    /// <summary>Die angeschlossenen Geräte.</summary>
    public IReadOnlyList<Courseziel> Ziele { get; }

    /// <summary><c>true</c>, wenn mehrere Geräte angeschlossen sind (dann erscheint die Geräteauswahl).</summary>
    public bool HatMehrereZiele => Ziele.Count > 1;

    /// <summary><c>true</c>, wenn eine gefahrene Strecke dabei ist (dann erscheint die Wahl des Fahrprofils).</summary>
    public bool HatGefahreneStrecke => Zeilen.Any(z => z.Verlauf is not null);

    /// <summary>Die wählbaren Fahrprofile mit ihren Anzeigetexten (wie in der Tourenplanung).</summary>
    public static IReadOnlyList<KeyValuePair<Fahrprofil, string>> Fahrprofile => TourenplanungViewModel.Fahrprofile;

    /// <summary>Das gewählte Gerät.</summary>
    [ObservableProperty]
    private Courseziel _ziel;

    /// <summary>Das Fahrprofil, mit dem die Abbiegungen gefahrener Strecken ermittelt werden.</summary>
    [ObservableProperty]
    private Fahrprofil _profil;

    /// <summary>Roter Hinweis (fehlender Name, Fehler beim Schreiben); sonst leer.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>Grauer Zustandstext während der Übertragung, z. B. „Abbiegungen werden ermittelt …“.</summary>
    [ObservableProperty]
    private string _zustand = string.Empty;

    /// <summary><c>true</c>, solange übertragen wird.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstBereit))]
    [NotifyCanExecuteChangedFor(nameof(UebertragenCommand))]
    private bool _istBeschaeftigt;

    /// <summary><c>true</c>, solange nicht übertragen wird (dann lassen sich die Namen ändern).</summary>
    public bool IstBereit => !IstBeschaeftigt;

    /// <summary>Text für den Aufrufer nach einer Übertragung; <c>null</c>, wenn nichts übertragen wurde.</summary>
    public string? Ergebnis { get; private set; }

    /// <summary><c>true</c>, wenn das <see cref="Ergebnis"/> einen Grund für fehlende Abbiegehinweise enthält.</summary>
    public bool ErgebnisMitWarnung { get; private set; }

    /// <summary>
    /// Sucht die angeschlossenen Garmin-Geräte: Edge 800/1000 als Laufwerk, Edge 1040 als Mediengerät.
    /// </summary>
    /// <returns>Die Geräte; leer, wenn keines angeschlossen ist.</returns>
    public static List<Courseziel> FindeZiele() => [.. FindeLaufwerksziele.Ausfuehren(), .. FindeMtpZiele.Ausfuehren()];

    /// <summary>
    /// Sucht die Geräte, zeigt das Fenster und meldet das Ergebnis; ohne Gerät erscheint nur ein Hinweis.
    /// </summary>
    /// <param name="besitzer">Das aufrufende Fenster.</param>
    /// <param name="zeilen">Die Courses.</param>
    /// <param name="profil">Vorgewähltes Fahrprofil.</param>
    /// <param name="ladeRouting">Liest die Routingeinstellungen.</param>
    public static void Zeige(Window? besitzer, IReadOnlyList<Coursezeile> zeilen, Fahrprofil profil, Func<Routingeinstellungen> ladeRouting)
    {
        List<Courseziel> ziele = FindeZiele();
        if (ziele.Count == 0)
        {
            Meldung(besitzer, KeinGeraet, MessageBoxImage.Information);
            return;
        }

        var viewModel = new CourseUebertragenViewModel(zeilen, ziele, profil, ladeRouting);
        new CourseUebertragenFenster(viewModel) { Owner = besitzer }.ShowDialog();
        if (viewModel.Ergebnis is string ergebnis)
        {
            Meldung(besitzer, ergebnis, viewModel.ErgebnisMitWarnung ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Bricht eine laufende Übertragung ab (beim Schließen des Fensters): Die Ermittlung der Abbiegungen endet, und
    /// kein weiterer Course wird geschrieben.
    /// </summary>
    public void Abbrechen() => _abbruch?.Cancel();

    /// <summary>Befehl „Übertragen“.</summary>
    [RelayCommand(CanExecute = nameof(KannUebertragen))]
    private async Task Uebertragen()
    {
        Hinweis = string.Empty;
        if (Zeilen.Any(z => z.Name.Trim().Length == 0))
        {
            Hinweis = "Bitte für jeden Course einen Namen eingeben.";
            return;
        }

        if (Zeilen.GroupBy(z => BildeCoursename.Dateiname(z.Name), StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1) is { } doppelt)
        {
            Hinweis = $"Der Name „{doppelt.First().Name.Trim()}“ kommt mehrfach vor. Bitte verschiedene Namen eingeben.";
            return;
        }

        IstBeschaeftigt = true;
        var quelle = new CancellationTokenSource();
        _abbruch = quelle;
        CancellationToken abbruch = quelle.Token;
        try
        {
            List<Coursezeile>? auswahl = PruefeVorhandene();
            if (auswahl is null)
            {
                return;
            }

            List<string> uebertragen = [];
            string? grund = null;
            foreach (Coursezeile zeile in auswahl)
            {
                abbruch.ThrowIfCancellationRequested();
                string name = zeile.Name.Trim();
                Course course;
                if (zeile.Planung is Tourenplanung planung)
                {
                    course = BildeCourse.AusPlanung(planung, name);
                }
                else
                {
                    Zustand = $"Abbiegungen für „{name}“ werden ermittelt …";
                    IReadOnlyList<Geoposition> spur = BildeCourse.Spur(zeile.Verlauf!);
                    (IReadOnlyList<Courseabbiegung> abbiegungen, string? fehlt) =
                        await Abbiegungsabruf.Ermittle(spur, _ladeRouting(), Profil, abbruch);
                    grund ??= fehlt;
                    course = BildeCourse.AusVerlauf(zeile.Verlauf!, name, abbiegungen);
                }

                abbruch.ThrowIfCancellationRequested();
                Zustand = $"„{name}“ wird übertragen …";
                byte[] inhalt = SchreibeFitCourse.Ausfuehren(course, DateTime.UtcNow);
                if (!Schreibe(BildeCoursename.Dateiname(name), inhalt, name, uebertragen))
                {
                    return;
                }

                uebertragen.Add(name);
            }

            Ergebnis = Erfolgstext(uebertragen) + (grund is null ? string.Empty : "\n\n" + grund);
            ErgebnisMitWarnung = grund is not null;
            Fertig?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException)
        {
            // Das Fenster wurde während der Übertragung geschlossen
        }
        finally
        {
            _abbruch = null;
            quelle.Dispose();
            IstBeschaeftigt = false;
            Zustand = string.Empty;
        }
    }

    /// <summary>Wählbarkeit von „Übertragen“: nicht während einer laufenden Übertragung.</summary>
    private bool KannUebertragen() => IstBereit;

    /// <summary>
    /// Fragt bei jedem Course, dessen Datei auf dem Gerät schon wartet, ob sie ersetzt werden soll.
    /// </summary>
    /// <returns>Die zu übertragenden Zeilen; <c>null</c>, wenn das Gerät nicht erreichbar ist (Hinweis gesetzt).</returns>
    private List<Coursezeile>? PruefeVorhandene()
    {
        List<Coursezeile> auswahl = [];
        foreach (Coursezeile zeile in Zeilen)
        {
            string datei = BildeCoursename.Dateiname(zeile.Name);
            bool vorhanden;
            try
            {
                vorhanden = Ziel.DateiVorhanden(datei);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                Hinweis = $"Das Gerät ist nicht erreichbar: {fehler.Message}";
                return null;
            }

            if (!vorhanden || MessageBox.Show(
                    $"Auf „{Ziel.Geraet}“ wartet schon ein Course „{zeile.Name.Trim()}“, der noch nicht übernommen wurde. Soll er ersetzt werden?",
                    Titel, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                auswahl.Add(zeile);
            }
        }

        return auswahl;
    }

    /// <summary>
    /// Schreibt eine Datei auf das gewählte Gerät.
    /// </summary>
    /// <returns><c>true</c> bei Erfolg; sonst steht der Grund im <see cref="Hinweis"/>.</returns>
    private bool Schreibe(string datei, byte[] inhalt, string name, List<string> bisher)
    {
        try
        {
            Ziel.Schreibe(datei, inhalt);
            return true;
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            string schon = bisher.Count == 0 ? string.Empty : $" Bereits übertragen: {string.Join(", ", bisher.Select(n => $"„{n}“"))}.";
            Hinweis = $"Der Course „{name}“ konnte nicht übertragen werden: {fehler.Message}{schon}";
            return false;
        }
    }

    /// <summary>Text nach gelungener Übertragung, z. B. „Course „X“ übertragen. …“.</summary>
    private static string Erfolgstext(List<string> namen) => namen.Count switch
    {
        0 => "Es wurde kein Course übertragen.",
        1 => $"Course „{namen[0]}“ übertragen. Er erscheint auf dem Edge nach dem Trennen vom Computer unter „Courses“.",
        _ => string.Create(CultureInfo.GetCultureInfo("de-DE"),
            $"{namen.Count} Courses übertragen ({string.Join(", ", namen.Select(n => $"„{n}“"))}). Sie erscheinen auf dem Edge nach dem Trennen vom Computer unter „Courses“."),
    };

    /// <summary>Zeigt eine Meldung mit dem Titel des Fensters.</summary>
    private static void Meldung(Window? besitzer, string text, MessageBoxImage art)
    {
        if (besitzer is null)
        {
            MessageBox.Show(text, Titel, MessageBoxButton.OK, art);
        }
        else
        {
            MessageBox.Show(besitzer, text, Titel, MessageBoxButton.OK, art);
        }
    }
}

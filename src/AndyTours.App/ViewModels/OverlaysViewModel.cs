using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using AndyTours.App.Funktionen;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Overlays“ (Menü „Tourenplanung → Overlays…“): Ordnerbaum mit den Overlays, Ein- und
/// Ausschalten, GPX-Dateien hinzufügen, Ordner anlegen, umbenennen, verschieben, löschen und Farben wählen.
/// </summary>
/// <remarks>
/// Jede Änderung wird sofort in der Datenbank des Datenordners gespeichert. Danach liest das ViewModel Ordner und
/// Overlays neu, baut den Baum neu auf (aufgeklappte Ordner und Markierung bleiben erhalten) und meldet den Stand dem
/// <see cref="Overlaybestand"/>, der ihn an alle offenen Karten weitergibt. Lässt sich eine Änderung nicht speichern,
/// erscheint ein roter Hinweis und der Baum zeigt den gespeicherten Stand.
/// </remarks>
public partial class OverlaysViewModel : ObservableObject
{
    /// <summary>Titel für Rückfragen.</summary>
    private const string Titel = "Overlays";

    /// <summary>Name eines neu angelegten Ordners; ist er vergeben, kommt eine Nummer dazu („Neuer Ordner 2“).</summary>
    private const string NeuerOrdner = "Neuer Ordner";

    /// <summary>Hinweis, wenn noch keine Overlays vorhanden sind.</summary>
    public const string Leerhinweis =
        "Noch keine Overlays vorhanden. Mit „GPX hinzufügen…“ eine oder mehrere GPX-Dateien aufnehmen, z. B. eine " +
        "gespeicherte Planung aus „Tour planen“ oder eine als GPX exportierte Tour.";

    /// <summary>Vergleich für die alphabetische Reihenfolge im Baum (deutsch, ohne Rücksicht auf Groß-/Kleinschreibung).</summary>
    private static readonly StringComparer Alphabetisch = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true);

    /// <summary>Liefert den aktuellen Datenordner.</summary>
    private readonly Func<string> _datenordner;

    /// <summary>Alle Ordner, wie zuletzt gelesen.</summary>
    private List<Overlayordner> _ordner = [];

    /// <summary>Alle Overlays, wie zuletzt gelesen.</summary>
    private List<Overlay> _overlays = [];

    /// <summary><c>true</c>, solange die Auswahl in die Felder übertragen wird (dann keine Änderung speichern).</summary>
    private bool _wirdAngezeigt;

    /// <summary>
    /// Erzeugt das ViewModel und liest die Overlays des Datenordners.
    /// </summary>
    /// <param name="datenordner">Liefert den aktuellen Datenordner (er kann sich während der Anzeige ändern).</param>
    public OverlaysViewModel(Func<string> datenordner)
    {
        _datenordner = datenordner;
        Overlaybestand.Neugeladen += DatenordnerNeuGeladen;
        Overlaybestand.Geaendert += OverlaysGeaendert;
        Lade();
    }

    /// <summary>Wird ausgelöst, wenn ein neuer Ordner angelegt wurde und sein Name gleich bearbeitet werden soll.</summary>
    public event EventHandler? NameBearbeiten;

    /// <summary>Wird ausgelöst, wenn das Fenster sich schließen soll (nach „Übernehmen“).</summary>
    public event EventHandler? Schliessen;

    /// <summary>Die Knoten auf oberster Ebene: Ordner, dann Overlays, jeweils alphabetisch.</summary>
    public ObservableCollection<Overlayknoten> Wurzeln { get; } = [];

    /// <summary>Der markierte Knoten oder <c>null</c>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstOverlayAusgewaehlt), nameof(IstEtwasAusgewaehlt), nameof(Auswahlhinweis))]
    [NotifyCanExecuteChangedFor(nameof(NameUebernehmenCommand), nameof(VerschiebenCommand), nameof(LoeschenCommand))]
    private Overlayknoten? _ausgewaehlt;

    /// <summary><c>true</c>, wenn ein Overlay markiert ist (Farbauswahl sichtbar).</summary>
    public bool IstOverlayAusgewaehlt => Ausgewaehlt?.IstOverlay == true;

    /// <summary><c>true</c>, wenn ein Knoten markiert ist (Namensfeld sichtbar).</summary>
    public bool IstEtwasAusgewaehlt => Ausgewaehlt is not null;

    /// <summary>
    /// Grauer Hinweis unter Name und Farbe, solange sie gesperrt sind: ohne Markierung bzw. bei einem markierten Ordner
    /// (Ordner haben keine Farbe); sonst leer.
    /// </summary>
    public string Auswahlhinweis => Ausgewaehlt switch
    {
        null => "Zum Umbenennen oder für eine eigene Farbe einen Ordner bzw. ein Overlay im Baum markieren.",
        { IstOrdner: true } => "Eine eigene Farbe gibt es nur für Overlays.",
        _ => string.Empty,
    };

    /// <summary>Eingabefeld „Name“ des markierten Knotens.</summary>
    [ObservableProperty]
    private string _bearbeitungsname = string.Empty;

    /// <summary>Roter Hinweis (abgewiesene Eingabe, nicht aufgenommene Dateien, Speicherfehler); sonst leer.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary><c>true</c>, wenn es noch keine Overlays gibt (dann erscheint der <see cref="Leerhinweis"/>).</summary>
    [ObservableProperty]
    private bool _istLeer;

    /// <summary>Text des Hinweises für die Datenbindung.</summary>
    public string LeerhinweisText => Leerhinweis;

    /// <summary>Die Farbauswahl eines Overlays: „Gemeinsame Farbe (…)“ und die zwölf Farben.</summary>
    public ObservableCollection<Overlayfarbeintrag> Farbauswahl { get; } = [];

    /// <summary>Die gewählte Farbe des markierten Overlays.</summary>
    [ObservableProperty]
    private Overlayfarbeintrag? _gewaehlteFarbe;

    /// <summary>
    /// Meldet das Fenster von den Ereignissen des <see cref="Overlaybestand"/>s ab (beim Schließen).
    /// </summary>
    public void Beenden()
    {
        Overlaybestand.Neugeladen -= DatenordnerNeuGeladen;
        Overlaybestand.Geaendert -= OverlaysGeaendert;
    }

    /// <summary>
    /// Doppelklick auf einen Knoten: Bei einem Overlay zeigen die offenen Karten seine ganze Strecke; ein
    /// ausgeschaltetes Overlay wird dazu eingeschaltet, damit es auch zu sehen ist.
    /// </summary>
    /// <param name="knoten">Der doppelt angeklickte Knoten.</param>
    public void ZeigeGanz(Overlayknoten knoten)
    {
        if (knoten.Overlay is not Overlay overlay)
        {
            return;
        }

        if (!overlay.IstEingeschaltet)
        {
            Umschalten(knoten, true);
        }

        Overlaybestand.ZeigeGanz(overlay);
    }

    /// <summary>
    /// Befehl „Ordner anlegen“: legt „Neuer Ordner“ im markierten Ordner an (bei einem markierten Overlay in dessen
    /// Ordner, ohne Markierung auf oberster Ebene), markiert ihn und stellt den Namen zur Bearbeitung bereit.
    /// </summary>
    [RelayCommand]
    private void OrdnerAnlegen()
    {
        long? eltern = Zielordner();
        string name = NeuerOrdner;
        for (int nummer = 2; PruefeOrdnername.Ausfuehren(name, eltern, _ordner) is not null; nummer++)
        {
            name = $"{NeuerOrdner} {nummer}";
        }

        long id = 0;
        if (Speichere(verbindung => id = OverlayordnerTabelle.Einfuegen(verbindung, eltern, name)))
        {
            Lade(markieren: (true, id));
            NameBearbeiten?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Befehl „GPX hinzufügen…“: nimmt die gewählten GPX-Dateien als eingeschaltete Overlays in den markierten Ordner
    /// auf (bei einem markierten Overlay in dessen Ordner, ohne Markierung auf oberster Ebene).
    /// </summary>
    /// <remarks>
    /// Jede Datei wird für sich gelesen; Dateien ohne Strecke oder nicht lesbare Dateien werden übergangen und im
    /// Hinweis genannt, die übrigen trotzdem aufgenommen. Die Strecke wird in die Datenbank übernommen, die Datei selbst
    /// danach nicht mehr gebraucht.
    /// </remarks>
    [RelayCommand]
    private void GpxHinzufuegen()
    {
        var dialog = new OpenFileDialog
        {
            Title = "GPX-Dateien als Overlays hinzufügen",
            Filter = "GPX-Dateien (*.gpx)|*.gpx|Alle Dateien (*.*)|*.*",
            Multiselect = true,
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        long? ordner = Zielordner();
        var neu = new List<Overlay>();
        var fehlerhaft = new List<string>();
        foreach (string datei in dialog.FileNames)
        {
            IReadOnlyList<IReadOnlyList<Geoposition>>? abschnitte;
            try
            {
                abschnitte = LeseGpxStrecke.Ausfuehren(File.ReadAllText(datei));
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                abschnitte = null;
            }

            if (abschnitte is null)
            {
                fehlerhaft.Add(Path.GetFileName(datei));
            }
            else
            {
                neu.Add(new Overlay(0, ordner, Path.GetFileNameWithoutExtension(datei), null, true, abschnitte));
            }
        }

        if (neu.Count > 0 && Speichere(verbindung =>
            {
                foreach (Overlay overlay in neu)
                {
                    OverlaysTabelle.Einfuegen(verbindung, overlay);
                }
            }))
        {
            Lade(aufklappen: ordner);
        }

        if (fehlerhaft.Count > 0)
        {
            Hinweis = $"Nicht aufgenommen, weil keine Strecke enthalten oder die Datei nicht lesbar ist: {string.Join(", ", fehlerhaft)}";
        }
    }

    /// <summary>
    /// Befehl „Übernehmen“ beim Namen (auch mit der Eingabetaste): benennt den markierten Ordner bzw. das Overlay um
    /// und schließt das Fenster.
    /// </summary>
    /// <remarks>
    /// Ein leerer Name wird nicht übernommen, ebenso bei Ordnern ein Name, der im selben Ordner schon vorkommt
    /// (<see cref="PruefeOrdnername"/>); dann erscheint ein Hinweis und das Fenster bleibt offen. Ist der Name
    /// unverändert, schließt sich das Fenster ohne Speichern.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(IstEtwasAusgewaehlt))]
    private void NameUebernehmen()
    {
        if (Ausgewaehlt is not Overlayknoten knoten)
        {
            return;
        }

        string name = Bearbeitungsname.Trim();
        Hinweis = string.Empty;
        if (name != knoten.Name && !Umbenennen(knoten, name))
        {
            return;
        }

        Schliessen?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Benennt einen Ordner bzw. ein Overlay um.
    /// </summary>
    /// <returns><c>true</c>, wenn der Name gespeichert ist; sonst steht der Grund im <see cref="Hinweis"/>.</returns>
    private bool Umbenennen(Overlayknoten knoten, string name)
    {
        if (knoten.Ordner is Overlayordner ordner)
        {
            if (PruefeOrdnername.Ausfuehren(name, ordner.ElternId, _ordner, ordner.Id) is string ablehnung)
            {
                Hinweis = ablehnung;
                return false;
            }

            if (!Speichere(verbindung => OverlayordnerTabelle.Umbenennen(verbindung, ordner.Id, name)))
            {
                return false;
            }
        }
        else if (knoten.Overlay is Overlay overlay)
        {
            if (name.Length == 0)
            {
                Hinweis = "Bitte einen Namen für das Overlay eingeben.";
                return false;
            }

            if (!Speichere(verbindung => OverlaysTabelle.Umbenennen(verbindung, overlay.Id, name)))
            {
                return false;
            }
        }

        Lade();
        return true;
    }

    /// <summary>
    /// Befehl „Verschieben nach…“: legt den markierten Ordner bzw. das Overlay in einen anderen Ordner oder auf die
    /// oberste Ebene.
    /// </summary>
    /// <remarks>
    /// Beim Verschieben eines Ordners fehlen er selbst und seine Unterordner in der Auswahl (<see cref="BildeOrdnerpfade"/>);
    /// gibt es im Ziel schon einen Ordner gleichen Namens, wird nicht verschoben und ein Hinweis erscheint.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(IstEtwasAusgewaehlt))]
    private void Verschieben()
    {
        if (Ausgewaehlt is not Overlayknoten knoten)
        {
            return;
        }

        Hinweis = string.Empty;
        long? bisher = knoten.Ordner?.ElternId ?? knoten.Overlay!.OrdnerId;
        IReadOnlyList<Auswahleintrag> eintraege = BildeOrdnerpfade.Ausfuehren(_ordner, knoten.Ordner?.Id);
        var viewModel = new EintragWaehlenViewModel("Verschieben nach", $"Neuer Ordner für „{knoten.Name}“:", eintraege,
            eintraege.FirstOrDefault(e => e.Id == bisher));
        var fenster = new EintragWaehlenFenster(viewModel) { Owner = Application.Current.Windows.OfType<OverlaysFenster>().FirstOrDefault() };
        if (fenster.ShowDialog() != true || viewModel.Ergebnis is not Auswahleintrag ziel || ziel.Id == bisher)
        {
            return;
        }

        if (knoten.Ordner is Overlayordner ordner)
        {
            if (!PruefeVerschiebeziel.Ausfuehren(ordner.Id, ziel.Id, _ordner))
            {
                return;
            }

            if (PruefeOrdnername.Ausfuehren(ordner.Name, ziel.Id, _ordner, ordner.Id) is string ablehnung)
            {
                Hinweis = ablehnung;
                return;
            }

            if (Speichere(verbindung => OverlayordnerTabelle.Verschieben(verbindung, ordner.Id, ziel.Id)))
            {
                Lade(aufklappen: ziel.Id);
            }
        }
        else if (knoten.Overlay is Overlay overlay
            && Speichere(verbindung => OverlaysTabelle.Verschieben(verbindung, overlay.Id, ziel.Id)))
        {
            Lade(aufklappen: ziel.Id);
        }
    }

    /// <summary>
    /// Befehl „Löschen“: löscht nach Rückfrage das markierte Overlay bzw. den markierten Ordner mit seinem gesamten
    /// Inhalt; ein leerer Ordner wird ohne Rückfrage gelöscht.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IstEtwasAusgewaehlt))]
    private void Loeschen()
    {
        if (Ausgewaehlt is not Overlayknoten knoten)
        {
            return;
        }

        Hinweis = string.Empty;
        if (knoten.Ordner is Overlayordner ordner)
        {
            (IReadOnlySet<long> unterordner, IReadOnlyList<Overlay> overlays) = SammleOrdnerinhalt.Ausfuehren(ordner.Id, _ordner, _overlays);
            if ((unterordner.Count > 0 || overlays.Count > 0)
                && !Bestaetigt($"Den Ordner „{ordner.Name}“ mit {Anzahl(unterordner.Count, "Unterordner", "Unterordnern")} und " +
                    $"{Anzahl(overlays.Count, "Overlay", "Overlays")} löschen?"))
            {
                return;
            }

            if (Speichere(verbindung => OverlayordnerTabelle.Loeschen(verbindung, ordner.Id)))
            {
                Lade();
            }
        }
        else if (knoten.Overlay is Overlay overlay
            && Bestaetigt($"Das Overlay „{overlay.Name}“ löschen?")
            && Speichere(verbindung => OverlaysTabelle.Loeschen(verbindung, overlay.Id)))
        {
            Lade();
        }
    }

    /// <summary>Wird bei der Wahl einer Farbe aufgerufen: die eigene Farbe des markierten Overlays sofort speichern.</summary>
    partial void OnGewaehlteFarbeChanged(Overlayfarbeintrag? value)
    {
        if (_wirdAngezeigt || value is null || Ausgewaehlt?.Overlay is not Overlay overlay || value.Farbe == overlay.Farbe)
        {
            return;
        }

        Hinweis = string.Empty;
        if (Speichere(verbindung => OverlaysTabelle.SetzeFarbe(verbindung, overlay.Id, value.Farbe)))
        {
            LadeZustaende();
        }
    }

    /// <summary>Wird bei einer neuen Markierung aufgerufen: Name und Farbe des Knotens in die Felder übertragen.</summary>
    partial void OnAusgewaehltChanged(Overlayknoten? value) => ZeigeAuswahl();

    /// <summary>
    /// Ein Kontrollkästchen wurde angeklickt: das Overlay bzw. alle Overlays des Ordners (auf allen Ebenen) ein- oder
    /// ausschalten.
    /// </summary>
    private void Umschalten(Overlayknoten knoten, bool einschalten)
    {
        Hinweis = string.Empty;
        List<long> ids = knoten.Overlay is Overlay overlay
            ? [overlay.Id]
            : [.. SammleOrdnerinhalt.Ausfuehren(knoten.Ordner!.Id, _ordner, _overlays).Overlays.Select(o => o.Id)];
        if (ids.Count > 0)
        {
            Speichere(verbindung => OverlaysTabelle.SetzeEingeschaltet(verbindung, ids, einschalten));
        }

        // Kein Neuaufbau des Baums: Er bliebe sonst während des Klicks nicht stehen (Bildlauf, Fokus)
        LadeZustaende();
    }

    /// <summary>
    /// Führt eine Änderung in der Datenbank aus.
    /// </summary>
    /// <returns><c>true</c> bei Erfolg; sonst steht der Grund im <see cref="Hinweis"/>.</returns>
    private bool Speichere(Action<SqliteConnection> aenderung)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(_datenordner(), DatenordnerInhalt.Datenbankdatei));
            aenderung(verbindung);
            return true;
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            Hinweis = $"Die Änderung konnte nicht gespeichert werden: {fehler.Message}";
            return false;
        }
    }

    /// <summary>
    /// Liest Ordner und Overlays neu, baut den Baum auf und meldet den Stand an alle Karten.
    /// </summary>
    /// <param name="markieren">Knoten, der danach markiert sein soll (Ordner?, Nummer); sonst bleibt die Markierung.</param>
    /// <param name="aufklappen">Ordner, der danach (samt übergeordneten Ordnern) aufgeklappt sein soll.</param>
    private void Lade((bool IstOrdner, long Id)? markieren = null, long? aufklappen = null)
    {
        LeseDaten();
        BaueBaum(markieren ?? (Ausgewaehlt is { } knoten ? (knoten.IstOrdner, knoten.Ordner?.Id ?? knoten.Overlay!.Id) : null), aufklappen);
        Overlaybestand.Uebernimm(_overlays);
    }

    /// <summary>
    /// Liest die Overlays neu und überträgt Ein/Aus-Zustand und Farbe in die vorhandenen Knoten (nach dem Umschalten
    /// oder einer neuen Farbe, die am Aufbau des Baums nichts ändern); meldet den Stand an alle Karten.
    /// </summary>
    private void LadeZustaende()
    {
        LeseDaten();
        Dictionary<long, Overlay> nachId = _overlays.ToDictionary(o => o.Id);
        foreach (Overlayknoten knoten in Alle(Wurzeln).Where(k => k.IstOverlay))
        {
            if (nachId.TryGetValue(knoten.Overlay!.Id, out Overlay? overlay))
            {
                knoten.Overlay = overlay;
                knoten.Farbe = Overlayfarbeintrag.ErzeugePinsel(overlay.Farbe ?? Overlaybestand.Farbe);
            }
        }

        foreach (Overlayknoten knoten in Wurzeln)
        {
            SetzeZustand(knoten);
        }

        Overlaybestand.Uebernimm(_overlays);
    }

    /// <summary>Liest Ordner und Overlays aus der Datenbank; bei einem Fehler bleiben beide Listen leer.</summary>
    private void LeseDaten()
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(_datenordner(), DatenordnerInhalt.Datenbankdatei));
            _ordner = OverlayordnerTabelle.Laden(verbindung);
            _overlays = OverlaysTabelle.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _ordner = [];
            _overlays = [];
            Hinweis = $"Die Overlays konnten nicht gelesen werden: {fehler.Message}";
        }
    }

    /// <summary>
    /// Baut den Baum aus <see cref="_ordner"/> und <see cref="_overlays"/> neu auf und setzt die Zustände der
    /// Kontrollkästchen; bisher aufgeklappte Ordner bleiben aufgeklappt.
    /// </summary>
    private void BaueBaum((bool IstOrdner, long Id)? markieren, long? aufklappen)
    {
        HashSet<long> aufgeklappt = [.. Alle(Wurzeln).Where(k => k.IstAufgeklappt && k.IstOrdner).Select(k => k.Ordner!.Id)];
        Dictionary<long, long?> eltern = _ordner.ToDictionary(o => o.Id, o => o.ElternId);
        for (long? id = aufklappen; id is long ordnerId && aufgeklappt.Add(ordnerId); id = eltern.GetValueOrDefault(ordnerId))
        {
        }

        Ausgewaehlt = null;
        Wurzeln.Clear();
        foreach (Overlayknoten knoten in Kinder(null, aufgeklappt))
        {
            Wurzeln.Add(knoten);
        }

        foreach (Overlayknoten knoten in Wurzeln)
        {
            SetzeZustand(knoten);
        }

        IstLeer = _overlays.Count == 0;
        if (markieren is (bool istOrdner, long markierteId)
            && Alle(Wurzeln).FirstOrDefault(k => k.IstOrdner == istOrdner && (k.Ordner?.Id ?? k.Overlay!.Id) == markierteId) is Overlayknoten ziel)
        {
            ziel.IstAusgewaehlt = true;
        }
    }

    /// <summary>Die Knoten eines Ordners (bzw. der obersten Ebene): Unterordner, dann Overlays, jeweils alphabetisch.</summary>
    private List<Overlayknoten> Kinder(long? ordnerId, HashSet<long> aufgeklappt)
    {
        List<Overlayknoten> knoten = [];
        foreach (Overlayordner ordner in _ordner.Where(o => o.ElternId == ordnerId).OrderBy(o => o.Name, Alphabetisch))
        {
            var ordnerknoten = new Overlayknoten(ordner, Umschalten) { IstAufgeklappt = aufgeklappt.Contains(ordner.Id) };
            foreach (Overlayknoten kind in Kinder(ordner.Id, aufgeklappt))
            {
                ordnerknoten.Kinder.Add(kind);
            }

            knoten.Add(ordnerknoten);
        }

        knoten.AddRange(_overlays
            .Where(o => o.OrdnerId == ordnerId)
            .OrderBy(o => o.Name, Alphabetisch)
            .Select(o => new Overlayknoten(o, Overlayfarbeintrag.ErzeugePinsel(o.Farbe ?? Overlaybestand.Farbe), Umschalten)));
        foreach (Overlayknoten k in knoten)
        {
            k.Ausgewaehlt += (sender, _) => Ausgewaehlt = (Overlayknoten)sender!;
        }

        return knoten;
    }

    /// <summary>
    /// Setzt die Kontrollkästchen eines Teilbaums und liefert die Zustände seiner Overlays (eingeschaltet, ausgeschaltet).
    /// </summary>
    private static (int Ein, int Aus) SetzeZustand(Overlayknoten knoten)
    {
        if (knoten.Overlay is Overlay overlay)
        {
            knoten.ZeigeZustand(overlay.IstEingeschaltet);
            return overlay.IstEingeschaltet ? (1, 0) : (0, 1);
        }

        int ein = 0;
        int aus = 0;
        foreach (Overlayknoten kind in knoten.Kinder)
        {
            (int e, int a) = SetzeZustand(kind);
            ein += e;
            aus += a;
        }

        // Ein Ordner ohne Overlays gilt als ausgeschaltet
        knoten.ZeigeZustand(ein > 0 && aus > 0 ? null : ein > 0);
        return (ein, aus);
    }

    /// <summary>Überträgt Name und Farbe des markierten Knotens in die Felder.</summary>
    private void ZeigeAuswahl()
    {
        _wirdAngezeigt = true;
        try
        {
            Bearbeitungsname = Ausgewaehlt?.Name ?? string.Empty;
            FuelleFarbauswahl();
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>Füllt die Farbauswahl (der erste Eintrag zeigt die aktuelle gemeinsame Farbe) und wählt die Farbe des Overlays.</summary>
    private void FuelleFarbauswahl()
    {
        Farbauswahl.Clear();
        Farbauswahl.Add(Overlayfarbeintrag.Gemeinsam(Overlaybestand.Farbe));
        foreach (Overlayfarbeintrag eintrag in Overlayfarbeintrag.Alle())
        {
            Farbauswahl.Add(eintrag);
        }

        GewaehlteFarbe = Ausgewaehlt?.Overlay is Overlay overlay ? Farbauswahl.First(e => e.Farbe == overlay.Farbe) : null;
    }

    /// <summary>
    /// Die gemeinsame Farbe kann sich in den Einstellungen ändern: Farbfelder der Overlays ohne eigene Farbe und den
    /// ersten Eintrag der Farbauswahl anpassen.
    /// </summary>
    private void OverlaysGeaendert(object? sender, EventArgs e)
    {
        foreach (Overlayknoten knoten in Alle(Wurzeln).Where(k => k.Overlay is { Farbe: null }))
        {
            knoten.Farbe = Overlayfarbeintrag.ErzeugePinsel(Overlaybestand.Farbe);
        }

        if (Farbauswahl.Count > 0 && Farbauswahl[0].Name != Overlayfarbeintrag.Gemeinsam(Overlaybestand.Farbe).Name)
        {
            ZeigeAuswahl();
        }
    }

    /// <summary>Ein anderer Datenordner wurde geladen: dessen Overlays zeigen.</summary>
    private void DatenordnerNeuGeladen(object? sender, EventArgs e)
    {
        Hinweis = string.Empty;
        Lade();
    }

    /// <summary>
    /// Der Ordner für neue Ordner und Overlays: der markierte Ordner, bei einem markierten Overlay dessen Ordner, ohne
    /// Markierung die oberste Ebene (<c>null</c>).
    /// </summary>
    private long? Zielordner() => Ausgewaehlt?.Ordner?.Id ?? Ausgewaehlt?.Overlay?.OrdnerId;

    /// <summary>Alle Knoten eines Baums, auf allen Ebenen.</summary>
    private static IEnumerable<Overlayknoten> Alle(IEnumerable<Overlayknoten> knoten) =>
        knoten.SelectMany(k => Alle(k.Kinder).Prepend(k));

    /// <summary>Eine Anzahl mit passendem Wort, z. B. „1 Overlay“ oder „3 Overlays“.</summary>
    private static string Anzahl(int anzahl, string einzahl, string mehrzahl) =>
        string.Create(CultureInfo.GetCultureInfo("de-DE"), $"{anzahl} {(anzahl == 1 ? einzahl : mehrzahl)}");

    /// <summary>Stellt eine Ja/Nein-Rückfrage; „Nein“ ist vorgewählt.</summary>
    private static bool Bestaetigt(string frage) =>
        MessageBox.Show(frage, Titel, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
}

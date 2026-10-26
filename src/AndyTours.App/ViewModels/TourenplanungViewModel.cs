using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Funktionen;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Tour planen“.
/// </summary>
/// <remarks>
/// Es hält die <see cref="Tourenplanung"/> und ändert sie über <see cref="AenderePlanung"/>. Nach jeder Änderung werden
/// die offenen Teilstücke nacheinander beim Routingdienst ermittelt (<see cref="ErmittleOffeneTeilstuecke"/>); eine neue
/// Änderung bricht eine laufende Ermittlung ab und startet sie neu. Ein Teilstück, das der Dienst nicht liefert, wird
/// eine Luftlinie, und ein Hinweis erscheint. Die Karte zeichnet das Code-Behind (Mapsui kennt keine Bindungen); es
/// erfährt Änderungen über <see cref="PlanungGeaendert"/>. Speichern und Öffnen stehen im zweiten Teil der Klasse
/// (<c>TourenplanungViewModel.Datei.cs</c>).
/// </remarks>
public partial class TourenplanungViewModel : ObservableObject
{
    /// <summary>Titel der Rückfragen und Meldungen.</summary>
    private const string Titel = "Tour planen";

    /// <summary>Hinweis, wenn der Routingdienst ein Teilstück nicht geliefert hat.</summary>
    public const string DienstNichtErreichbar =
        "Der Routingdienst hat die Strecke nicht geliefert (keine Internetverbindung?). Betroffene Teilstücke sind als " +
        "gerade Linie gezeichnet.";

    /// <summary>Hinweis, wenn sich keine Anfrage bilden lässt, weil die Routingeinstellungen unvollständig sind.</summary>
    public const string EinstellungenUnvollstaendig =
        "Der Routingdienst ist nicht vollständig eingestellt (Serveradresse oder Zugangsschlüssel unter „Extras → " +
        "Einstellungen“). Die Strecke ist als gerade Linie gezeichnet.";

    /// <summary>Liest die aktuellen Routingeinstellungen (bei jeder Ermittlung neu, damit Änderungen sofort gelten).</summary>
    private readonly Func<Routingeinstellungen> _ladeRouting;

    /// <summary>Merkt die Planungseinstellungen (Fahrprofil, Ausschnitt, Ordner) in der Datenbank.</summary>
    private readonly Action<Planungseinstellungen> _speichereEinstellungen;

    /// <summary>Die zuletzt gemerkten Planungseinstellungen.</summary>
    private Planungseinstellungen _einstellungen;

    /// <summary>Abbruch der laufenden Ermittlung der Teilstücke.</summary>
    private CancellationTokenSource? _ermittlung;

    /// <summary><c>true</c>, solange eine geöffnete Planung übernommen wird (dann nicht neu ermitteln).</summary>
    private bool _wirdGeladen;

    /// <summary>
    /// Erzeugt das ViewModel mit einer leeren Planung.
    /// </summary>
    /// <param name="einstellungen">Die gemerkten Planungseinstellungen; das Fahrprofil wird vorgewählt.</param>
    /// <param name="suedWest">Südwestecke des Startausschnitts der Karte.</param>
    /// <param name="nordOst">Nordostecke des Startausschnitts der Karte.</param>
    /// <param name="ladeRouting">Liest die Routingeinstellungen.</param>
    /// <param name="speichereEinstellungen">Merkt die Planungseinstellungen.</param>
    public TourenplanungViewModel(Planungseinstellungen einstellungen, Geoposition suedWest, Geoposition nordOst,
        Func<Routingeinstellungen> ladeRouting, Action<Planungseinstellungen> speichereEinstellungen)
    {
        _einstellungen = einstellungen;
        _ladeRouting = ladeRouting;
        _speichereEinstellungen = speichereEinstellungen;
        StartSuedWest = suedWest;
        StartNordOst = nordOst;
        _profil = einstellungen.Profil;
        Planung = Tourenplanung.Leer(einstellungen.Profil);
        Werte = new Planungswerte(0, 0, 0);
    }

    /// <summary>Wird ausgelöst, wenn sich Wegpunkte oder Teilstücke geändert haben; die Karte zeichnet dann neu.</summary>
    public event EventHandler? PlanungGeaendert;

    /// <summary>Wird nach dem Öffnen einer Planung ausgelöst; die Karte zeigt dann die ganze Strecke.</summary>
    public event EventHandler? GanzePlanungAngefordert;

    /// <summary>Die wählbaren Fahrprofile mit ihren Anzeigetexten.</summary>
    public static IReadOnlyList<KeyValuePair<Fahrprofil, string>> Fahrprofile { get; } =
    [
        new(Fahrprofil.Rennrad, "Rennrad (nur Straße)"),
        new(Fahrprofil.Trekking, "Trekking/Tourenrad"),
        new(Fahrprofil.Mountainbike, "Mountainbike (bevorzugt offroad)"),
    ];

    /// <summary>Südwestecke des Kartenausschnitts beim Öffnen des Fensters.</summary>
    public Geoposition StartSuedWest { get; }

    /// <summary>Nordostecke des Kartenausschnitts beim Öffnen des Fensters.</summary>
    public Geoposition StartNordOst { get; }

    /// <summary>Die aktuelle Planung.</summary>
    public Tourenplanung Planung { get; private set; }

    /// <summary>Länge und Höhenmeter der aktuellen Planung.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaengeText), nameof(HoehenmeterText))]
    private Planungswerte _werte;

    /// <summary>Länge zur Anzeige, z. B. „42,7 km“.</summary>
    public string LaengeText => Werte.LaengeKm.ToString("N1", CultureInfo.CurrentCulture) + " km";

    /// <summary>Höhenmeter zur Anzeige, z. B. „↑ 380 hm  ↓ 375 hm“.</summary>
    public string HoehenmeterText => $"↑ {Werte.HoehenmeterBergauf:N0} hm  ↓ {Werte.HoehenmeterBergab:N0} hm";

    /// <summary>Das gewählte Fahrprofil; ein Wechsel ermittelt die ganze Strecke neu.</summary>
    [ObservableProperty]
    private Fahrprofil _profil;

    /// <summary>Hinweis unter der Karte, z. B. wenn der Routingdienst nicht erreichbar war; leer, wenn alles gut ging.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary><c>true</c>, solange Teilstücke beim Routingdienst ermittelt werden.</summary>
    [ObservableProperty]
    private bool _ermitteltGerade;

    /// <summary><c>true</c>, wenn es Änderungen gibt, die noch nicht gespeichert sind.</summary>
    [ObservableProperty]
    private bool _istGeaendert;

    /// <summary><c>true</c>, wenn die Planung mindestens einen Wegpunkt hat.</summary>
    public bool HatWegpunkte => Planung.Wegpunkte.Count > 0;

    /// <summary>
    /// Hängt einen Wegpunkt als neues Ziel an (Klick in die Karte neben die Strecke).
    /// </summary>
    /// <param name="position">Ort des Klicks.</param>
    public void Anhaengen(Geoposition position) => Aendere(AenderePlanung.Anhaengen(Planung, position));

    /// <summary>
    /// Fügt einen Wegpunkt in ein Teilstück ein (Linie weggezogen).
    /// </summary>
    /// <param name="teilstueck">Nummer des Teilstücks, dessen Linie gezogen wurde.</param>
    /// <param name="position">Ort, an dem die Linie losgelassen wurde.</param>
    public void Einfuegen(int teilstueck, Geoposition position) =>
        Aendere(AenderePlanung.Einfuegen(Planung, teilstueck + 1, position));

    /// <summary>
    /// Verschiebt einen Wegpunkt (Wegpunkt gezogen und losgelassen).
    /// </summary>
    /// <param name="nummer">Nummer des Wegpunkts.</param>
    /// <param name="position">Neuer Ort.</param>
    public void Verschieben(int nummer, Geoposition position) => Aendere(AenderePlanung.Verschieben(Planung, nummer, position));

    /// <summary>
    /// Entfernt einen Wegpunkt (Rechtsklick auf den Wegpunkt).
    /// </summary>
    /// <param name="nummer">Nummer des Wegpunkts.</param>
    public void Entfernen(int nummer) => Aendere(AenderePlanung.Entfernen(Planung, nummer));

    /// <summary>
    /// Merkt den beim Schließen gezeigten Kartenausschnitt für das nächste Öffnen.
    /// </summary>
    /// <param name="suedWest">Südwestecke.</param>
    /// <param name="nordOst">Nordostecke.</param>
    public void MerkeAusschnitt(Geoposition suedWest, Geoposition nordOst) =>
        Merke(_einstellungen with { SuedWest = suedWest, NordOst = nordOst });

    /// <summary>Befehl „Letzten Punkt entfernen“.</summary>
    [RelayCommand(CanExecute = nameof(HatWegpunkte))]
    private void LetztenPunktEntfernen() => Aendere(AenderePlanung.EntferneLetzten(Planung));

    /// <summary>Befehl „Alles löschen“: nach einer Rückfrage ist die Planung leer.</summary>
    [RelayCommand(CanExecute = nameof(HatWegpunkte))]
    private void AllesLoeschen()
    {
        MessageBoxResult antwort = MessageBox.Show("Alle Wegpunkte der Planung löschen?", Titel,
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        if (antwort == MessageBoxResult.Yes)
        {
            Aendere(Tourenplanung.Leer(Planung.Profil));
        }
    }

    /// <summary>Ein Profilwechsel ermittelt alle Teilstücke neu und wird gemerkt.</summary>
    partial void OnProfilChanged(Fahrprofil value)
    {
        Merke(_einstellungen with { Profil = value });
        if (!_wirdGeladen)
        {
            Aendere(AenderePlanung.WechsleProfil(Planung, value), geaendert: HatWegpunkte);
        }
    }

    /// <summary>
    /// Übernimmt eine geänderte Planung, zeichnet neu und ermittelt die offenen Teilstücke.
    /// </summary>
    /// <param name="neu">Die neue Planung.</param>
    /// <param name="geaendert">Ob dies eine ungespeicherte Änderung ist.</param>
    private void Aendere(Tourenplanung neu, bool geaendert = true)
    {
        Uebernimm(neu);
        if (geaendert)
        {
            IstGeaendert = true;
        }

        _ = ErmittleOffeneTeilstuecke();
    }

    /// <summary>Setzt die Planung, rechnet die Werte neu und meldet die Änderung.</summary>
    private void Uebernimm(Tourenplanung neu)
    {
        Planung = neu;
        Werte = BerechnePlanungswerte.Ausfuehren(neu);
        OnPropertyChanged(nameof(HatWegpunkte));
        LetztenPunktEntfernenCommand.NotifyCanExecuteChanged();
        AllesLoeschenCommand.NotifyCanExecuteChanged();
        SpeichernCommand.NotifyCanExecuteChanged();
        GarminUebertragenCommand.NotifyCanExecuteChanged();
        PlanungGeaendert?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ermittelt nacheinander alle noch offenen Teilstücke beim Routingdienst.
    /// </summary>
    /// <remarks>
    /// Eine laufende Ermittlung wird abgebrochen; sie gehört zu einer überholten Planung. Ein Ergebnis wird nur
    /// eingetragen, wenn das Teilstück noch offen ist und seine Wegpunkte sich nicht geändert haben. Liefert der Dienst
    /// nichts oder lässt sich keine Anfrage bilden, wird das Teilstück eine Luftlinie, und ein Hinweis erscheint.
    /// </remarks>
    private async Task ErmittleOffeneTeilstuecke()
    {
        _ermittlung?.Cancel();
        _ermittlung?.Dispose();
        var ermittlung = new CancellationTokenSource();
        _ermittlung = ermittlung;
        Hinweis = string.Empty;

        try
        {
            while (Planung.Teilstuecke.ToList().FindIndex(t => t is null) is int nummer and >= 0)
            {
                ErmitteltGerade = true;
                Geoposition von = Planung.Wegpunkte[nummer], nach = Planung.Wegpunkte[nummer + 1];
                Routinganfrage? anfrage = BildeRoutinganfrage.Ausfuehren(_ladeRouting(), Planung.Profil, von, nach);
                Teilstueck? teilstueck = anfrage is null ? null : await Routingabruf.Ermittle(anfrage, ermittlung.Token);
                ermittlung.Token.ThrowIfCancellationRequested();

                if (teilstueck is null)
                {
                    Hinweis = anfrage is null ? EinstellungenUnvollstaendig : DienstNichtErreichbar;
                    teilstueck = ErstelleLuftlinie.Ausfuehren(von, nach);
                }

                if (nummer < Planung.Teilstuecke.Count && Planung.Teilstuecke[nummer] is null
                    && Planung.Wegpunkte[nummer] == von && Planung.Wegpunkte[nummer + 1] == nach)
                {
                    Uebernimm(AenderePlanung.SetzeTeilstueck(Planung, nummer, teilstueck));
                }
                else
                {
                    // Kann nur nach einer Änderung ohne Abbruch passieren; dann nicht endlos weiterfragen
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Eine neuere Ermittlung hat übernommen
            return;
        }

        ErmitteltGerade = false;
    }

    /// <summary>Merkt geänderte Planungseinstellungen.</summary>
    private void Merke(Planungseinstellungen neu)
    {
        _einstellungen = neu;
        _speichereEinstellungen(neu);
    }
}

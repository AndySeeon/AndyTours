using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel der Karteikarte „Karte“ im Fenster „Tourdetail“: die gefahrene Strecke auf einer Landkarte, bei einer
/// Mehrtagestour die Strecken aller Etappen.
/// </summary>
/// <remarks>
/// Hält alles, was die Ansicht über die Karte wissen muss: die Strecken je Etappe und das Rechteck um alle, die
/// gewählte Kartenart, den passenden Lizenzhinweis und die Hinweise ohne GPS-Daten, ohne Aufzeichnung bzw. ohne
/// geladene Karte. Die Karte selbst (Mapsui, siehe <c>docs/adr/0003-kartenbibliothek.md</c>) baut das Code-Behind
/// des Fensters auf, weil Mapsui seine Ebenen nicht über Bindungen bekommt; es reagiert auf <see cref="Kartenart"/>
/// und <see cref="GanzeStreckeAngefordert"/> und meldet Ladefehler über <see cref="MeldeLadefehler"/>.
/// </remarks>
public partial class StreckenkarteViewModel : ObservableObject
{
    /// <summary>Hinweis, wenn die Aufzeichnung keine Positionen enthält (z. B. Fahrt auf der Rolle).</summary>
    public const string KeineGpsDaten = "Die Aufzeichnung enthält keine GPS-Daten.";

    /// <summary>Hinweis, wenn Kartenausschnitte weder aus dem Zwischenspeicher noch aus dem Internet kamen.</summary>
    public const string KarteNichtGeladen = "Die Karte konnte nicht geladen werden. Besteht eine Internetverbindung?";

    /// <summary>Lizenzhinweis von OpenStreetMap; gilt für beide Kartenarten, weil beide auf deren Daten beruhen.</summary>
    private const string HinweisOpenStreetMap = "© OpenStreetMap-Mitwirkende";

    /// <summary>Zusätzlicher Lizenzhinweis bei OpenTopoMap.</summary>
    private const string HinweisOpenTopoMap = "Kartendarstellung: © OpenTopoMap (CC-BY-SA)";

    /// <summary>Titel der Fehlermeldung beim Export.</summary>
    private const string Exporttitel = "Als GPX exportieren";

    /// <summary>Alle Etappen des Tourdetails, auch ohne Aufzeichnung (für Datei und Dateinamen).</summary>
    private readonly IReadOnlyList<Etappe> _alleEtappen;

    /// <summary>Liest den zuletzt für den Export verwendeten Ordner; <c>null</c> = noch keiner.</summary>
    private readonly Func<string?> _ladeGpxOrdner;

    /// <summary>Merkt sich den Ordner des letzten Exports.</summary>
    private readonly Action<string> _speichereGpxOrdner;

    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="etappen">Die Etappen mit Verlauf, in ihrer Reihenfolge.</param>
    /// <param name="mehrere"><c>true</c> bei einer Mehrtagestour (mehrere markierte Einheiten).</param>
    /// <param name="alleEtappen">Alle Etappen des Tourdetails, auch ohne Aufzeichnung, in ihrer Reihenfolge.</param>
    /// <param name="ladeGpxOrdner">Liest den zuletzt für „Als GPX exportieren…“ verwendeten Ordner.</param>
    /// <param name="speichereGpxOrdner">Merkt sich den Ordner nach einem Export.</param>
    public StreckenkarteViewModel(IReadOnlyList<Etappe> etappen, bool mehrere, IReadOnlyList<Etappe> alleEtappen,
        Func<string?> ladeGpxOrdner, Action<string> speichereGpxOrdner)
    {
        _alleEtappen = alleEtappen;
        _ladeGpxOrdner = ladeGpxOrdner;
        _speichereGpxOrdner = speichereGpxOrdner;

        List<Strecke> strecken = [];
        List<Trainingseinheit> ohneGps = [];
        foreach (Etappe etappe in etappen)
        {
            if (ErmittleStrecke.Ausfuehren(etappe.Verlauf ?? []) is Strecke strecke)
            {
                strecken.Add(strecke);
            }
            else
            {
                ohneGps.Add(etappe.Einheit);
            }
        }

        Strecken = strecken;
        if (strecken.Count > 0)
        {
            (SuedWest, NordOst) = UmschliesseStrecken.Ausfuehren(strecken);
        }

        BereiteBearbeitungVor(etappen, mehrere);

        HinweisOhneStrecke = mehrere && etappen.Count == 0 ? TourdetailViewModel.KeineAufzeichnung : KeineGpsDaten;
        if (mehrere && strecken.Count > 0 && ohneGps.Count > 0)
        {
            HinweisOhneGps = "Ohne GPS-Daten: " + BeschreibeEtappen.Ausfuehren(ohneGps);
        }
    }

    /// <summary>
    /// Wird ausgelöst, wenn die Karte wieder die ganze Strecke zeigen soll (Schaltfläche „Ganze Strecke“).
    /// </summary>
    public event EventHandler? GanzeStreckeAngefordert;

    /// <summary>
    /// Die Strecken der Etappen mit GPS-Daten, in ihrer Reihenfolge; leer ohne GPS-Daten. Nach „Track bearbeiten“
    /// die bearbeitete Strecke.
    /// </summary>
    public IReadOnlyList<Strecke> Strecken { get; private set; }

    /// <summary>Süd-West-Ecke des Rechtecks um alle Strecken (nur mit Strecke gesetzt).</summary>
    public Geoposition? SuedWest { get; private set; }

    /// <summary>Nord-Ost-Ecke des Rechtecks um alle Strecken (nur mit Strecke gesetzt).</summary>
    public Geoposition? NordOst { get; private set; }

    /// <summary><c>true</c>, wenn es eine Strecke gibt und die Karte erscheint.</summary>
    public bool HatStrecke => Strecken.Count > 0;

    /// <summary><c>true</c>, wenn statt der Karte der Hinweis <see cref="HinweisOhneStrecke"/> erscheint.</summary>
    public bool OhneStrecke => !HatStrecke;

    /// <summary><c>true</c> bei mehreren Strecken; dann erklärt die Zeichenerklärung auch den Etappenwechsel.</summary>
    public bool MehrereEtappen => Strecken.Count > 1;

    /// <summary>
    /// Hinweis statt der Karte: <see cref="TourdetailViewModel.KeineAufzeichnung"/>, wenn bei einer Mehrtagestour
    /// keine Etappe eine Aufzeichnung hat, sonst <see cref="KeineGpsDaten"/>.
    /// </summary>
    public string HinweisOhneStrecke { get; }

    /// <summary>Hinweis über der Karte, welche Etappen keine GPS-Daten haben; sonst <c>null</c>.</summary>
    public string? HinweisOhneGps { get; }

    /// <summary><c>true</c>, wenn <see cref="HinweisOhneGps"/> erscheint.</summary>
    public bool HatHinweisOhneGps => HinweisOhneGps is not null;

    /// <summary>Die gewählte Karte; zu Beginn die OpenStreetMap-Standardkarte.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstOpenStreetMap), nameof(IstOpenTopoMap), nameof(Lizenzhinweis))]
    private Kartenart _kartenart = Kartenart.OpenStreetMap;

    /// <summary>Umschalter „OpenStreetMap“ (Gegenstück zu <see cref="IstOpenTopoMap"/>).</summary>
    public bool IstOpenStreetMap
    {
        get => Kartenart == Kartenart.OpenStreetMap;
        set
        {
            if (value)
            {
                Kartenart = Kartenart.OpenStreetMap;
            }
        }
    }

    /// <summary>Umschalter „OpenTopoMap“.</summary>
    public bool IstOpenTopoMap
    {
        get => Kartenart == Kartenart.OpenTopoMap;
        set
        {
            if (value)
            {
                Kartenart = Kartenart.OpenTopoMap;
            }
        }
    }

    /// <summary>
    /// Lizenzhinweis in der Ecke der Karte: immer „© OpenStreetMap-Mitwirkende“, bei OpenTopoMap ergänzt um den
    /// Hinweis auf die Kartendarstellung.
    /// </summary>
    public string Lizenzhinweis => Kartenart == Kartenart.OpenTopoMap
        ? $"{HinweisOpenStreetMap} · {HinweisOpenTopoMap}"
        : HinweisOpenStreetMap;

    /// <summary><c>true</c>, wenn der Hinweis <see cref="KarteNichtGeladen"/> erscheint.</summary>
    [ObservableProperty]
    private bool _ladefehler;

    /// <summary>
    /// Meldet, dass ein Kartenausschnitt nicht geladen werden konnte; die Strecke bleibt trotzdem gezeichnet.
    /// </summary>
    /// <remarks>Der Hinweis bleibt stehen, bis der Benutzer die Kartenart wechselt.</remarks>
    public void MeldeLadefehler() => Ladefehler = true;

    /// <summary>Beim Wechsel der Kartenart beginnt das Laden neu; ein alter Hinweis gilt nicht mehr.</summary>
    partial void OnKartenartChanged(Kartenart value) => Ladefehler = false;

    /// <summary>Befehl „Ganze Strecke“: Die Karte zeigt wieder die ganze Strecke.</summary>
    [RelayCommand]
    private void GanzeStrecke() => GanzeStreckeAngefordert?.Invoke(this, EventArgs.Empty);

    /// <summary>Hinweis neben „Als GPX exportieren…“ nach einem gelungenen Export, z. B. „Track exportiert: …“.</summary>
    [ObservableProperty]
    private string _exporthinweis = string.Empty;

    /// <summary>
    /// Befehl „Als GPX exportieren…“: speichert die Strecke (bei einer Mehrtagestour alle Etappen) als GPX-Datei.
    /// </summary>
    /// <remarks>
    /// Der Speichern-Dialog schlägt den zuletzt verwendeten Ordner vor (gibt es ihn nicht mehr, „Dokumente“) und
    /// den Dateinamen aus <see cref="BildeGpxDateiname"/>; ob eine vorhandene Datei ersetzt wird, fragt der Dialog
    /// selbst. Wurde die Strecke mit „Track bearbeiten“ verändert, wird die bearbeitete Strecke exportiert, und der
    /// Dateiname trägt den Zusatz „(bearbeitet)“. Den Inhalt erzeugt <see cref="ErstelleGpx"/>. Bei Abbruch geschieht nichts; ein Schreibfehler
    /// erscheint als Meldung mit dem Grund. Nur wählbar, wenn es eine Strecke gibt.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(HatStrecke))]
    private void GpxExportieren()
    {
        if (ErstelleGpx.Ausfuehren(ExportEtappen()) is not string inhalt)
        {
            return;
        }

        string? ordner = _ladeGpxOrdner();
        var dialog = new SaveFileDialog
        {
            Title = Exporttitel,
            Filter = "GPX-Dateien (*.gpx)|*.gpx",
            DefaultExt = ".gpx",
            FileName = BildeGpxDateiname.Ausfuehren(_alleEtappen, IstBearbeitet),
            InitialDirectory = ordner is not null && Directory.Exists(ordner)
                ? ordner
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, inhalt, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            Exporthinweis = string.Empty;
            MessageBox.Show($"Der Track konnte nicht exportiert werden: {fehler.Message}", Exporttitel,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Exporthinweis = "Track exportiert: " + Path.GetFileName(dialog.FileName);
        if (Path.GetDirectoryName(dialog.FileName) is string verwendet)
        {
            _speichereGpxOrdner(verwendet);
        }
    }
}

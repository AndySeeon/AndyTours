using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Garmin;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: das Fenster „Tour planen“ öffnen.
/// </summary>
public partial class HauptfensterViewModel
{
    /// <summary>Südwestecke Deutschlands: Startausschnitt, wenn weder ein Ausschnitt gemerkt noch eine Tour aufgezeichnet ist.</summary>
    private static readonly Geoposition DeutschlandSuedWest = new(47.2, 5.8);

    /// <summary>Nordostecke Deutschlands.</summary>
    private static readonly Geoposition DeutschlandNordOst = new(55.1, 15.1);

    /// <summary>
    /// Befehl „Tourenplanung → Tour planen…“: öffnet das Planungsfenster (nicht modal) mit einer leeren Planung.
    /// </summary>
    /// <remarks>
    /// Die Karte zeigt den zuletzt in der Planung gezeigten Ausschnitt; beim ersten Mal das Gebiet der zuletzt gefahrenen
    /// Einheit mit Aufzeichnung, ohne eine solche Deutschland. Fahrprofil, Ausschnitt und Ordner werden still gemerkt;
    /// die Routingeinstellungen liest das Fenster bei jeder Ermittlung neu, damit Änderungen in den Einstellungen sofort
    /// gelten.
    /// </remarks>
    [RelayCommand]
    private void TourPlanen()
    {
        Planungseinstellungen einstellungen = LadeEinstellung(PlanungseinstellungenEinstellung.Laden) ?? Planungseinstellungen.Standard;
        (Geoposition suedWest, Geoposition nordOst) = einstellungen is { SuedWest: Geoposition sw, NordOst: Geoposition no }
            ? (sw, no)
            : GebietDerLetztenTour() ?? (DeutschlandSuedWest, DeutschlandNordOst);

        var viewModel = new TourenplanungViewModel(einstellungen, suedWest, nordOst,
            () => LadeEinstellung(RoutingEinstellung.Laden) ?? Routingeinstellungen.Standard,
            neu => SpeichereStill(verbindung => PlanungseinstellungenEinstellung.Speichern(verbindung, neu)));
        new TourenplanungFenster(viewModel) { Owner = Application.Current.MainWindow }.Show();
    }

    /// <summary>
    /// Liest eine Einstellung still aus der Datenbank; ist sie nicht erreichbar, kommt <c>null</c> zurück.
    /// </summary>
    private T? LadeEinstellung<T>(Func<SqliteConnection, T> laden) where T : class
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei));
            return laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// Das Gebiet der zuletzt gefahrenen Einheit, deren Aufzeichnung GPS-Daten enthält, oder <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Es werden höchstens die fünf jüngsten Einheiten mit Aufzeichnung gelesen, damit das Öffnen schnell bleibt
    /// (z. B. wenn die letzten Einheiten auf der Rolle ohne GPS gefahren wurden).
    /// </remarks>
    private (Geoposition SuedWest, Geoposition NordOst)? GebietDerLetztenTour()
    {
        IEnumerable<string> aufzeichnungen = _bestand.Trainingseinheiten
            .OrderByDescending(e => e.Datum).ThenByDescending(e => e.Startzeit)
            .Select(FindeAufzeichnung.Ausfuehren)
            .OfType<string>()
            .Select(relativ => Path.Combine(Datenordner, relativ))
            .Where(File.Exists)
            .Take(5);

        foreach (string pfad in aufzeichnungen)
        {
            (List<Verlaufspunkt>? verlauf, _) = LeseFitVerlauf.Ausfuehren(pfad);
            if (verlauf is not null && ErmittleStrecke.Ausfuehren(verlauf) is Strecke strecke)
            {
                return (strecke.SuedWest, strecke.NordOst);
            }
        }

        return null;
    }
}

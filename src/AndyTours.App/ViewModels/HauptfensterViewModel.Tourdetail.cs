using System.IO;
using System.Windows;
using System.Windows.Input;
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
/// Teil des Hauptfenster-ViewModels: das Fenster „Tourdetail“ zu den markierten Einheiten öffnen.
/// </summary>
/// <remarks>
/// Ablauf: Die markierten Einheiten werden zu Etappen geordnet (<see cref="OrdneEtappen"/>). Zu jeder wird die
/// Aufzeichnung bestimmt (<see cref="FindeAufzeichnung"/>), geprüft, ob die Datei im Datenordner liegt, und ihr
/// Verlauf gelesen (<see cref="LeseFitVerlauf"/>). Dann öffnet sich das Fenster nicht modal.
/// <list type="bullet">
/// <item>Eine Einheit: Fehlt die Datei oder ist sie nicht lesbar, erscheint eine Meldung, und das Fenster öffnet
/// sich nicht.</item>
/// <item>Mehrere Einheiten (Mehrtagestour): Fehlende oder unlesbare Dateien stehen in einer gemeinsamen Meldung;
/// danach öffnet sich das Fenster trotzdem, und diese Etappen zählen wie Etappen ohne Aufzeichnung.</item>
/// </list>
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Titel der Meldungen zum Tourdetail.</summary>
    private const string Tourdetailtitel = "Tourdetail";

    /// <summary>Befehl „Einheit → Tourdetail…“ (auch im Kontextmenü der Tabelle).</summary>
    [RelayCommand(CanExecute = nameof(KannTourdetailOeffnen))]
    private void TourdetailOeffnen()
    {
        List<Trainingseinheit> einheiten = OrdneEtappen.Ausfuehren(LeseMarkierteEinheiten());
        List<Etappe>? etappen = einheiten.Count switch
        {
            0 => null,
            1 => LeseEinzelneEtappe(einheiten[0]),
            _ => LeseEtappen(einheiten),
        };

        if (etappen is not null)
        {
            var viewModel = new TourdetailViewModel(etappen, LadeGpxOrdner,
                ordner => SpeichereStill(verbindung => GpxOrdnerEinstellung.Speichern(verbindung, ordner)),
                () => (LadeEinstellung(PlanungseinstellungenEinstellung.Laden) ?? Planungseinstellungen.Standard).Profil,
                () => LadeEinstellung(RoutingEinstellung.Laden) ?? Routingeinstellungen.Standard);
            new TourdetailFenster(viewModel) { Owner = Application.Current.MainWindow }.Show();
        }
    }

    /// <summary>
    /// Liest den zuletzt für „Als GPX exportieren…“ verwendeten Ordner.
    /// </summary>
    /// <remarks>
    /// Wie beim Speichern (<see cref="SpeichereStill"/>) ohne Fehlermeldung: Ist die Datenbank nicht erreichbar,
    /// schlägt der Dialog einfach „Dokumente“ vor.
    /// </remarks>
    /// <returns>Der Ordner oder <c>null</c>.</returns>
    private string? LadeGpxOrdner()
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei));
            return GpxOrdnerEinstellung.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// Wählbarkeit von „Tourdetail…“: mehrere Einheiten markiert (auch ohne Aufzeichnung) oder genau eine mit
    /// Aufzeichnung.
    /// </summary>
    private bool KannTourdetailOeffnen() =>
        MarkierteEinheiten.Count > 1
        || (MarkierteEinheiten.Count == 1
            && FindeEinheit(MarkierteEinheiten[0]) is Trainingseinheit einheit
            && FindeAufzeichnung.Ausfuehren(einheit) is not null);

    /// <summary>
    /// Liest die Aufzeichnung einer einzelnen Einheit; bei einem Fehler erscheint eine Meldung.
    /// </summary>
    /// <param name="einheit">Die markierte Einheit.</param>
    /// <returns>Die eine Etappe oder <c>null</c>, wenn das Fenster nicht geöffnet werden soll.</returns>
    private List<Etappe>? LeseEinzelneEtappe(Trainingseinheit einheit)
    {
        if (FindeAufzeichnung.Ausfuehren(einheit) is not string relativerPfad)
        {
            return null;
        }

        string pfad = Path.Combine(Datenordner, relativerPfad);
        if (!File.Exists(pfad))
        {
            MessageBox.Show(NichtGefunden(relativerPfad), Tourdetailtitel, MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        (List<Verlaufspunkt>? verlauf, string? fehler) = MitSanduhr(() => LeseFitVerlauf.Ausfuehren(pfad));
        if (verlauf is null)
        {
            MessageBox.Show($"Die Aufzeichnung konnte nicht gelesen werden.\n\n{fehler}", Tourdetailtitel,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return [new Etappe(einheit, verlauf)];
    }

    /// <summary>
    /// Liest die Aufzeichnungen aller Etappen einer Mehrtagestour; fehlende oder unlesbare Dateien stehen danach in
    /// einer gemeinsamen Meldung.
    /// </summary>
    /// <param name="einheiten">Die Einheiten in der Reihenfolge der Etappen.</param>
    /// <returns>Die Etappen; ohne lesbare Aufzeichnung ist ihr Verlauf <c>null</c>.</returns>
    private List<Etappe> LeseEtappen(List<Trainingseinheit> einheiten)
    {
        List<string> meldungen = [];
        List<Etappe> etappen = MitSanduhr(() => einheiten.Select(einheit =>
        {
            if (FindeAufzeichnung.Ausfuehren(einheit) is not string relativerPfad)
            {
                return new Etappe(einheit, null);
            }

            string pfad = Path.Combine(Datenordner, relativerPfad);
            if (!File.Exists(pfad))
            {
                meldungen.Add(NichtGefunden(relativerPfad));
                return new Etappe(einheit, null);
            }

            (List<Verlaufspunkt>? verlauf, string? fehler) = LeseFitVerlauf.Ausfuehren(pfad);
            if (verlauf is null)
            {
                meldungen.Add($"Die Aufzeichnung ‚{relativerPfad}‘ konnte nicht gelesen werden: {fehler}");
            }

            return new Etappe(einheit, verlauf);
        }).ToList());

        if (meldungen.Count > 0)
        {
            MessageBox.Show(string.Join("\n\n", meldungen), Tourdetailtitel, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        return etappen;
    }

    /// <summary>Hinweis, wenn die Datei einer Aufzeichnung im Datenordner fehlt.</summary>
    private static string NichtGefunden(string relativerPfad) =>
        $"Die Aufzeichnung ‚{relativerPfad}‘ wurde im Datenordner nicht gefunden.";

    /// <summary>
    /// Führt das Lesen mit Sanduhr aus; es dauert auch bei mehreren langen Touren nur einen Augenblick.
    /// </summary>
    private static T MitSanduhr<T>(Func<T> lesen)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            return lesen();
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }
}

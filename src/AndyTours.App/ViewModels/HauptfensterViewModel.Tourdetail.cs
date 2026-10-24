using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Import.Garmin;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: das Fenster „Tourdetail“ (Höhenprofil) zur markierten Einheit öffnen.
/// </summary>
/// <remarks>
/// Ablauf: Aufzeichnung der Einheit bestimmen (<see cref="FindeAufzeichnung"/>), prüfen, ob die Datei im Datenordner
/// liegt, ihren Verlauf lesen (<see cref="LeseFitVerlauf"/>) und das Fenster nicht modal öffnen. Fehlt die Datei oder
/// ist sie nicht lesbar, erscheint eine Meldung.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Titel der Meldungen zum Tourdetail.</summary>
    private const string Tourdetailtitel = "Tourdetail";

    /// <summary>Befehl „Einheit → Tourdetail…“ (auch im Kontextmenü der Tabelle).</summary>
    [RelayCommand(CanExecute = nameof(HatMarkierteAufzeichnung))]
    private void TourdetailOeffnen()
    {
        if (MarkierteEinheiten.Count != 1 || FindeEinheit(MarkierteEinheiten[0]) is not Trainingseinheit einheit
            || FindeAufzeichnung.Ausfuehren(einheit) is not string relativerPfad)
        {
            return;
        }

        string pfad = Path.Combine(Datenordner, relativerPfad);
        if (!File.Exists(pfad))
        {
            MessageBox.Show($"Die Aufzeichnung ‚{relativerPfad}‘ wurde im Datenordner nicht gefunden.", Tourdetailtitel,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Lesen dauert auch bei langen Touren nur einen Augenblick; währenddessen die Sanduhr zeigen
        List<Verlaufspunkt>? verlauf;
        string? fehler;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            (verlauf, fehler) = LeseFitVerlauf.Ausfuehren(pfad);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (verlauf is null)
        {
            MessageBox.Show($"Die Aufzeichnung konnte nicht gelesen werden.\n\n{fehler}", Tourdetailtitel,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        new TourdetailFenster(new TourdetailViewModel(einheit, verlauf)) { Owner = Application.Current.MainWindow }.Show();
    }

    /// <summary>Wählbarkeit von „Tourdetail…“: genau eine Einheit markiert, und sie hat eine Aufzeichnung.</summary>
    private bool HatMarkierteAufzeichnung() =>
        MarkierteEinheiten.Count == 1
        && FindeEinheit(MarkierteEinheiten[0]) is Trainingseinheit einheit
        && FindeAufzeichnung.Ausfuehren(einheit) is not null;
}

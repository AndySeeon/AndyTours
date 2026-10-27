using System.Windows;
using System.Windows.Input;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Einstellungsfensters.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Daten und Befehle liegen im <see cref="EinstellungenViewModel"/>.
/// „Schließen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt das
/// Fenster ohne eigenen Code. Hier wird nur der Doppelklick auf einen Eintrag der Auswahlliste an
/// „Bearbeiten…“ weitergegeben.
/// </remarks>
public partial class EinstellungenFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit dem aktuellen Datenordner.</param>
    public EinstellungenFenster(EinstellungenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;

        // Gespeicherten Zugangsschlüssel maskiert anzeigen; nach einem Ordnerwechsel den des neuen Datenordners
        SchluesselFeld.Password = viewModel.Schluessel;
        viewModel.SchluesselNeuGelesen += (_, _) => SchluesselFeld.Password = viewModel.Schluessel;
    }

    /// <summary>
    /// Meldet einen geänderten Zugangsschlüssel an das ViewModel, das ihn sofort speichert.
    /// </summary>
    /// <remarks>
    /// Ein Passwortfeld lässt sich (aus Sicherheitsgründen) nicht binden, deshalb dieser Umweg.
    /// </remarks>
    private void SchluesselFeld_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is EinstellungenViewModel viewModel)
        {
            viewModel.SchluesselGeaendert(SchluesselFeld.Password);
        }
    }

    /// <summary>Doppelklick auf einen Eintrag der Auswahlliste: bei Equipments wie „Bearbeiten…“.</summary>
    private void Eintrag_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is EinstellungenViewModel { Auswahllisten: var auswahllisten }
            && auswahllisten.BearbeitenCommand.CanExecute(null))
        {
            auswahllisten.BearbeitenCommand.Execute(null);
        }
    }
}

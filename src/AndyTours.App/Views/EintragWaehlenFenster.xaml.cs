using System.Windows;
using System.Windows.Input;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Eintrag wählen“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Liste, Auswahl und Befehl liegen im <see cref="EintragWaehlenViewModel"/>. Hier wird
/// nur das Fenster geschlossen, sobald das ViewModel einen Eintrag meldet, und der Doppelklick auf eine
/// Zeile an „OK“ weitergegeben. „Abbrechen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert.
/// </remarks>
public partial class EintragWaehlenFenster : Window
{
    /// <summary>Das ViewModel dieses Fensters.</summary>
    private readonly EintragWaehlenViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den wählbaren Einträgen.</param>
    public EintragWaehlenFenster(EintragWaehlenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.Gewaehlt += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus in die Liste, die Vorauswahl sichtbar
        Loaded += (_, _) =>
        {
            Liste.Focus();
            if (Liste.SelectedItem is not null)
            {
                Liste.ScrollIntoView(Liste.SelectedItem);
            }
        };
    }

    /// <summary>Doppelklick auf eine Zeile: wie „OK“.</summary>
    private void Zeile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.OkCommand.CanExecute(null))
        {
            _viewModel.OkCommand.Execute(null);
        }
    }
}

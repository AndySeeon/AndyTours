using System.Windows;
using System.Windows.Input;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Zusammenführen mit …“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Liste, Auswahl und Befehl liegen im <see cref="ZusammenfuehrenViewModel"/>.
/// Hier wird nur das Fenster geschlossen, sobald das ViewModel ein Ziel meldet, und der Doppelklick auf
/// eine Zeile an „OK“ weitergegeben. „Abbrechen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>)
/// markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class ZusammenfuehrenFenster : Window
{
    /// <summary>Das ViewModel dieses Fensters.</summary>
    private readonly ZusammenfuehrenViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den wählbaren Zielen.</param>
    public ZusammenfuehrenFenster(ZusammenfuehrenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.Gewaehlt += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus in die Liste, damit Pfeiltasten sofort wirken
        Loaded += (_, _) => Liste.Focus();
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

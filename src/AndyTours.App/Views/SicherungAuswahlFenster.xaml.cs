using System.Windows;
using System.Windows.Input;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Sicherung wiederherstellen“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Liste, Auswahl und Befehl liegen im <see cref="SicherungAuswahlViewModel"/>.
/// Hier wird nur das Fenster geschlossen, sobald das ViewModel eine Auswahl meldet, und der
/// Doppelklick auf eine Zeile an den Befehl „Wiederherstellen…“ weitergegeben. „Abbrechen“ ist als
/// Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class SicherungAuswahlFenster : Window
{
    /// <summary>Das ViewModel dieses Fensters.</summary>
    private readonly SicherungAuswahlViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den Sicherungen.</param>
    public SicherungAuswahlFenster(SicherungAuswahlViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.Gewaehlt += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus auf die vorausgewählte Zeile, damit Pfeiltasten und Enter sofort wirken
        Loaded += (_, _) =>
        {
            if (Liste.SelectedItem is not null)
            {
                Liste.UpdateLayout();
                (Liste.ItemContainerGenerator.ContainerFromItem(Liste.SelectedItem) as UIElement)?.Focus();
            }
        };
    }

    /// <summary>Doppelklick auf eine Zeile: wie „Wiederherstellen…“.</summary>
    private void Zeile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.WiederherstellenCommand.CanExecute(null))
        {
            _viewModel.WiederherstellenCommand.Execute(null);
        }
    }
}

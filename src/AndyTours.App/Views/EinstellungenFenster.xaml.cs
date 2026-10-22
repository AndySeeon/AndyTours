using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Einstellungsfensters.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Daten und Befehle liegen im <see cref="EinstellungenViewModel"/>.
/// „Schließen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt das
/// Fenster ohne eigenen Code.
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
        DataContext = viewModel;
    }
}

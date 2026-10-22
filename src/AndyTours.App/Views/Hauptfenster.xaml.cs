using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Hauptfensters.
/// </summary>
/// <remarks>
/// Hier steht absichtlich keine Programmlogik. Daten und Befehle liegen im
/// <see cref="HauptfensterViewModel"/>, das beim Programmstart erzeugt und hier als
/// DataContext gesetzt wird.
/// </remarks>
public partial class Hauptfenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster, baut die in XAML beschriebenen Steuerelemente auf und verbindet sie mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">Das ViewModel mit dem bereits geladenen Datenbestand.</param>
    public Hauptfenster(HauptfensterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

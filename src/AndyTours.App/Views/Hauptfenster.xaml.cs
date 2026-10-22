using System.Windows;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Hauptfensters.
/// </summary>
/// <remarks>
/// Hier steht absichtlich keine Programmlogik. Daten und Befehle liegen im
/// <see cref="ViewModels.HauptfensterViewModel"/>, das in <c>Hauptfenster.xaml</c>
/// als DataContext gesetzt wird.
/// </remarks>
public partial class Hauptfenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und baut die in XAML beschriebenen Steuerelemente auf.
    /// </summary>
    public Hauptfenster()
    {
        InitializeComponent();
    }
}

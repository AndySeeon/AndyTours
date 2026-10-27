using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Auf Garmin übertragen“.
/// </summary>
/// <remarks>
/// Hier steht nur das Schließen: nach dem Übertragen schließt sich das Fenster; „Abbrechen“ oder das Schließen ohne
/// Übertragung überträgt nichts und bricht ein laufendes Ermitteln der Abbiegungen ab.
/// </remarks>
public partial class CourseUebertragenFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel der Übertragung.</param>
    public CourseUebertragenFenster(CourseUebertragenViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Fertig += (_, _) => Close();
        Closed += (_, _) => viewModel.Abbrechen();
    }
}

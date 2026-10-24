using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Tourdetail“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Kopfzeile, Diagramm, Achsenwahl, Häkchen und Maussteuerung liegen im
/// <see cref="TourdetailViewModel"/>. Das Fenster wird nicht modal geöffnet und ohne Rückfrage geschlossen.
/// </remarks>
public partial class TourdetailFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Kopfzeile und Verlauf der Tour.</param>
    public TourdetailFenster(TourdetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

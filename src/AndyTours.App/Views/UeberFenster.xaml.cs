using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Über AndyTours“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Die Angaben liegen im <see cref="UeberViewModel"/>. Esc schließt das Fenster über
/// <c>IsCancel</c> der Schaltfläche von selbst; „OK“ und Enter schließen es über <see cref="Ok_Click"/>.
/// </remarks>
public partial class UeberFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Name, Version und Autor.</param>
    public UeberFenster(UeberViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
    }

    /// <summary>„OK“ oder Enter: Fenster schließen.</summary>
    private void Ok_Click(object sender, RoutedEventArgs e) => Close();
}

using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Garmin-Import“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Liste, Häkchen, Equipment und Befehl liegen im <see cref="GarminImportViewModel"/>.
/// Hier wird nur die Spalte „Gerät“ ein- oder ausgeblendet (Tabellenspalten lassen sich nicht direkt binden)
/// und das Fenster geschlossen, sobald das ViewModel „Importieren“ meldet. „Abbrechen“ ist als
/// Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class GarminImportFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den neuen Touren.</param>
    public GarminImportFenster(GarminImportViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
        SpalteGeraet.Visibility = viewModel.MehrereGeraete ? Visibility.Visible : Visibility.Collapsed;
        viewModel.Bestaetigt += (_, _) => DialogResult = true;
    }
}

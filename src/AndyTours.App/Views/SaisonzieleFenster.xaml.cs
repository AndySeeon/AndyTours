using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Saisonziele“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Jahre, Zeilen, Prüfung und Speichern liegen im <see cref="SaisonzieleViewModel"/>.
/// Hier wird nur das Fenster geschlossen, sobald das ViewModel gespeichert hat. „Abbrechen“ ist als
/// Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class SaisonzieleFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den Saisonzielen.</param>
    public SaisonzieleFenster(SaisonzieleViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Gespeichert += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus in die Tabelle
        Loaded += (_, _) => Tabelle.Focus();
    }
}

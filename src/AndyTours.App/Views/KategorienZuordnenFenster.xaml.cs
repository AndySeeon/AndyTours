using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Kategorien zuordnen“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Zeilen und Ergebnis liegen im <see cref="KategorienZuordnenViewModel"/>. Hier wird
/// nur das Fenster geschlossen, sobald „OK“ gewählt wurde. „Abbrechen“ ist als Abbrechen-Schaltfläche
/// (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class KategorienZuordnenFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den Kategorien.</param>
    public KategorienZuordnenFenster(KategorienZuordnenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
        viewModel.Bestaetigt += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus in die Liste, damit Pfeiltasten und Leertaste sofort wirken
        Loaded += (_, _) => Liste.Focus();
    }
}

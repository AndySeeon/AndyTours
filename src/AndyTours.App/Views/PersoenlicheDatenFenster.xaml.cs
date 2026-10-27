using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Persönliche Daten“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Felder, Prüfung und Speichern liegen im <see cref="PersoenlicheDatenViewModel"/>.
/// Hier wird nur das Fenster geschlossen, sobald das ViewModel gespeichert hat. „Abbrechen“ ist als
/// Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class PersoenlicheDatenFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den persönlichen Daten.</param>
    public PersoenlicheDatenFenster(PersoenlicheDatenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
        viewModel.Gespeichert += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus ins erste Feld
        Loaded += (_, _) => VornameFeld.Focus();
    }
}

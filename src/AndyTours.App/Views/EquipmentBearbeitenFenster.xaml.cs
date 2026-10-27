using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Equipment“.
/// </summary>
/// <remarks>
/// Keine Programmlogik: Felder, Prüfung und Speichern liegen im <see cref="EquipmentBearbeitenViewModel"/>.
/// Hier wird nur das Fenster geschlossen, sobald das ViewModel gespeichert hat. „Abbrechen“ ist als
/// Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert und schließt ohne eigenen Code.
/// </remarks>
public partial class EquipmentBearbeitenFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den Feldern des Equipments.</param>
    public EquipmentBearbeitenFenster(EquipmentBearbeitenViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
        viewModel.Gespeichert += (_, _) => DialogResult = true;

        // Tastaturbedienung: Fokus in den Namen
        Loaded += (_, _) => NameFeld.Focus();
    }
}

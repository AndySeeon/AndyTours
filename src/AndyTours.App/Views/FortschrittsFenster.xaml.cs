using System.ComponentModel;
using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fortschrittsfensters.
/// </summary>
/// <remarks>
/// Enthält nur die Sperre gegen vorzeitiges Schließen; die Anzeige kommt aus dem
/// <see cref="FortschrittsViewModel"/>, der Kopiervorgang aus <c>KopiereMitFortschritt</c>.
/// </remarks>
public partial class FortschrittsFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit Titel und Fortschrittswerten.</param>
    public FortschrittsFenster(FortschrittsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Wird nach dem Ende des Kopierens auf <c>true</c> gesetzt; erst dann lässt sich das Fenster schließen.
    /// </summary>
    public bool DarfSchliessen { get; set; }

    /// <summary>
    /// Verhindert das Schließen (X, Alt+F4), solange das Kopieren läuft – sonst bliebe eine Teilkopie zurück.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = !DarfSchliessen;
        base.OnClosing(e);
    }
}

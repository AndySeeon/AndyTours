using System.ComponentModel;
using System.Windows;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Bearbeitungsfensters einer Trainingseinheit.
/// </summary>
/// <remarks>
/// Daten, Prüfung und Speichern liegen im <see cref="EinheitBearbeitenViewModel"/>. Hier stehen nur die
/// Fensterabläufe: nach erfolgreichem Speichern schließen und beim Schließen ohne Speichern nachfragen,
/// wenn etwas geändert wurde. „Abbrechen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>, auch Esc)
/// markiert und löst ebenfalls diese Rückfrage aus.
/// </remarks>
public partial class EinheitBearbeitenFenster : Window
{
    /// <summary>Das ViewModel des Fensters.</summary>
    private readonly EinheitBearbeitenViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster, verbindet es mit dem ViewModel und setzt den Cursor ins Datumsfeld.
    /// </summary>
    /// <param name="viewModel">ViewModel mit der zu bearbeitenden Einheit.</param>
    public EinheitBearbeitenFenster(EinheitBearbeitenViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.SchliessenAngefordert += (_, _) => DialogResult = true;
        Loaded += (_, _) => DatumFeld.Focus();
    }

    /// <summary>
    /// Fragt beim Schließen ohne Speichern nach, wenn etwas geändert wurde („Änderungen verwerfen?“);
    /// bei „Nein“ bleibt das Fenster mit den Eingaben offen.
    /// </summary>
    /// <param name="e">Ereignisdaten; <c>Cancel</c> verhindert das Schließen.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (_viewModel.IstGespeichert || !_viewModel.IstGeaendert)
        {
            return;
        }

        MessageBoxResult antwort = MessageBox.Show(this, "Änderungen verwerfen?", Title,
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        e.Cancel = antwort != MessageBoxResult.Yes;
    }
}

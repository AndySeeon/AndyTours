using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AndyTours.App.ViewModels;
using AndyTours.Core.Modell;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind der Karteikarte „Wartung“.
/// </summary>
/// <remarks>
/// Daten und Befehle liegen im <see cref="HauptfensterViewModel"/>. Hier stehen nur die Abläufe der Tabelle:
/// die Markierung an das ViewModel melden, Doppelklick und Entf an dessen Befehle weitergeben und nach dem
/// Speichern die gespeicherte Wartung markieren.
/// </remarks>
public partial class WartungenAnsicht : UserControl
{
    /// <summary>Das ViewModel, bei dem die Ansicht angemeldet ist.</summary>
    private HauptfensterViewModel? _viewModel;

    /// <summary>Erzeugt die Ansicht; das ViewModel kommt über den geerbten Datenkontext.</summary>
    public WartungenAnsicht()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => BeobachteViewModel();
    }

    /// <summary>Meldet sich beim (neuen) ViewModel an, um nach dem Speichern die Wartung zu markieren.</summary>
    private void BeobachteViewModel()
    {
        if (_viewModel is not null)
        {
            _viewModel.MarkiereWartung -= ViewModel_MarkiereWartung;
        }

        _viewModel = DataContext as HauptfensterViewModel;
        if (_viewModel is not null)
        {
            _viewModel.MarkiereWartung += ViewModel_MarkiereWartung;
        }
    }

    /// <summary>
    /// Meldet dem ViewModel die markierten Wartungen, damit „Bearbeiten…“ und „Löschen“ richtig wählbar sind.
    /// </summary>
    private void Tabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, Tabelle) || _viewModel is null)
        {
            return;
        }

        _viewModel.MarkierteWartungen = Tabelle.SelectedItems.OfType<Wartungszeile>().Select(z => z.Id).ToList();
    }

    /// <summary>Markiert die Zeile einer Wartung (nach dem Speichern) und holt sie in den sichtbaren Bereich.</summary>
    /// <param name="id">Nummer der Wartung.</param>
    private void ViewModel_MarkiereWartung(long id)
    {
        if (_viewModel?.Wartungszeilen.FirstOrDefault(z => z.Id == id) is not Wartungszeile zeile)
        {
            return;
        }

        Tabelle.SelectedItem = zeile;
        Tabelle.ScrollIntoView(zeile);
    }

    /// <summary>Doppelklick auf eine Zeile: öffnet das Wartungsfenster der markierten Wartung.</summary>
    private void Zeile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel is null || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        e.Handled = true;
        if (_viewModel.WartungBearbeitenCommand.CanExecute(null))
        {
            _viewModel.WartungBearbeitenCommand.Execute(null);
        }
    }

    /// <summary>Entf löscht die markierten Wartungen (nach der Rückfrage des ViewModels).</summary>
    private void Tabelle_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || _viewModel is null)
        {
            return;
        }

        e.Handled = true;
        if (_viewModel.WartungenLoeschenCommand.CanExecute(null))
        {
            _viewModel.WartungenLoeschenCommand.Execute(null);
        }
    }

    /// <summary>
    /// Rechtsklick auf eine nicht markierte Zeile markiert diese, damit sich das Rechtsklickmenü auf sie bezieht;
    /// bei einer schon markierten Zeile bleibt eine Mehrfachmarkierung erhalten.
    /// </summary>
    private void Tabelle_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        DependencyObject? element = e.OriginalSource as DependencyObject;
        while (element is not null and not DataGridRow)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is DataGridRow zeile && !zeile.IsSelected)
        {
            Tabelle.SelectedItem = zeile.Item;
        }
    }
}

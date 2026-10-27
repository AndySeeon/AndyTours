using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AndyTours.App.ViewModels;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Overlays“.
/// </summary>
/// <remarks>
/// Hier steht nur, was sich nicht binden lässt: Doppelklick auf ein Overlay, Markieren per Rechtsklick vor dem
/// Kontextmenü, Bearbeiten des Namens eines neuen Ordners, Schließen nach „Übernehmen“ und das Abmelden des ViewModels
/// beim Schließen.
/// </remarks>
public partial class OverlaysFenster : Window
{
    /// <summary>Das ViewModel.</summary>
    private readonly OverlaysViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel der Overlay-Verwaltung.</param>
    public OverlaysFenster(OverlaysViewModel viewModel)
    {
        InitializeComponent();
        Oberflaechenskalierung.Anmelden(this);
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.NameBearbeiten += (_, _) => Dispatcher.BeginInvoke(() =>
        {
            NamensFeld.Focus();
            NamensFeld.SelectAll();
        });
        _viewModel.Schliessen += (_, _) => Close();
        Closed += (_, _) => _viewModel.Beenden();
    }

    /// <summary>Doppelklick auf ein Overlay: die offenen Karten zeigen seine ganze Strecke.</summary>
    private void Baum_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Doppelklick auf das Kontrollkästchen schaltet nur um
        if (Element<CheckBox>(e.OriginalSource) is null && Element<TreeViewItem>(e.OriginalSource)?.DataContext is Overlayknoten { IstOverlay: true } knoten)
        {
            _viewModel.ZeigeGanz(knoten);
            e.Handled = true;
        }
    }

    /// <summary>Rechtsklick markiert den Knoten unter der Maus, damit das Kontextmenü für ihn gilt.</summary>
    private void Baum_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Element<TreeViewItem>(e.OriginalSource) is TreeViewItem eintrag)
        {
            eintrag.IsSelected = true;
            eintrag.Focus();
        }
    }

    /// <summary>Das nächste Element des angegebenen Typs vom angeklickten Element aufwärts, oder <c>null</c>.</summary>
    private static T? Element<T>(object quelle) where T : DependencyObject
    {
        for (var element = quelle as DependencyObject; element is not null;
             element = element is Visual ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is T gefunden)
            {
                return gefunden;
            }
        }

        return null;
    }
}

using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using AndyTours.App.ViewModels;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind der Ansicht „Aktuell“.
/// </summary>
/// <remarks>
/// Keine Fachlogik: Bestleistungen, Texte und Diagramm liegen im <see cref="BestleistungenViewModel"/> bzw.
/// <see cref="JahresfortschrittViewModel"/>. Hier wird nur die Bedienung des Diagramms festgelegt:
/// <list type="bullet">
/// <item>Kein Hinweis beim Überfahren oder Anklicken, kein Zoomen und Verschieben mit Maus oder Mausrad.</item>
/// <item>Linksklick schaltet den gezeigten Wert weiter (<see cref="JahresfortschrittViewModel.WertWechselnCommand"/>).</item>
/// </list>
/// </remarks>
public partial class AktuellAnsicht : UserControl
{
    /// <summary>
    /// Erzeugt die Ansicht und legt die Bedienung des Diagramms fest.
    /// </summary>
    public AktuellAnsicht()
    {
        InitializeComponent();

        // Alle Standardbelegungen von OxyPlot entfernen; den Klick wertet Diagramm_Geklickt aus
        var steuerung = new PlotController();
        steuerung.UnbindAll();
        Diagramm.Controller = steuerung;
    }

    /// <summary>Linksklick ins Diagramm: nächsten Wert zeigen.</summary>
    private void Diagramm_Geklickt(object sender, MouseButtonEventArgs e)
    {
        // Die Ansicht selbst hat das Hauptfenster-ViewModel; das Diagramm den Jahresfortschritt
        if (Diagramm.DataContext is JahresfortschrittViewModel viewModel)
        {
            viewModel.WertWechselnCommand.Execute(null);
        }
    }
}

using System.ComponentModel;
using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.App.Funktionen;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Hauptfensters.
/// </summary>
/// <remarks>
/// Hier steht absichtlich keine Programmlogik. Daten und Befehle liegen im
/// <see cref="HauptfensterViewModel"/>, das beim Programmstart erzeugt und hier als
/// DataContext gesetzt wird. Der Code-Behind setzt nur die gemerkte Fenstergröße und -position sowie
/// die zuletzt gewählte Karteikarte und merkt sie sich beim Schließen zusammen mit der Spaltenanordnung
/// der Tabelle. Nach dem ersten Anzeigen lässt er das ViewModel fällige Wartungsalarme melden.
/// </remarks>
public partial class Hauptfenster : Window
{
    /// <summary>Das ViewModel des Fensters.</summary>
    private readonly HauptfensterViewModel _viewModel;

    /// <summary>
    /// Erzeugt das Fenster, baut die in XAML beschriebenen Steuerelemente auf, verbindet sie mit dem ViewModel
    /// und stellt die gemerkte Fenstergröße und -position wieder her.
    /// </summary>
    /// <param name="viewModel">Das ViewModel mit dem bereits geladenen Datenbestand.</param>
    public Hauptfenster(HauptfensterViewModel viewModel)
    {
        InitializeComponent();
        // Nur der Inhalt wächst mit der Größe der Oberfläche; das Fenster behält seine gemerkte Größe und Position
        Oberflaechenskalierung.Anmelden(this, groesseUmrechnen: false);
        _viewModel = viewModel;
        DataContext = viewModel;

        StelleFensterpositionWiederHer();
        StelleKarteikarteWiederHer();
        Closing += Hauptfenster_Closing;
        ContentRendered += Hauptfenster_ContentRendered;
    }

    /// <summary>
    /// Nach dem ersten Anzeigen des Fensters (Programmstart): fällige Wartungsalarme melden. Das Ereignis wird
    /// danach abgemeldet, damit der Hinweis nur einmal erscheint.
    /// </summary>
    private void Hauptfenster_ContentRendered(object? sender, EventArgs e)
    {
        ContentRendered -= Hauptfenster_ContentRendered;
        _viewModel.MeldeFaelligeAlarme();
    }

    /// <summary>
    /// Wählt die beim letzten Beenden gewählte Karteikarte.
    /// </summary>
    /// <remarks>
    /// Ohne gemerkten Wert oder mit einer Nummer, zu der es keine Karteikarte (mehr) gibt, bleibt die
    /// erste Karteikarte gewählt.
    /// </remarks>
    private void StelleKarteikarteWiederHer()
    {
        if (_viewModel.GemerkteKarteikarte is int nummer && nummer < Karteikarten.Items.Count)
        {
            Karteikarten.SelectedIndex = nummer;
        }
    }

    /// <summary>
    /// Setzt Größe, Position und „maximiert“ auf die beim letzten Beenden gemerkten Werte.
    /// </summary>
    /// <remarks>
    /// <see cref="PruefeFensterposition"/> holt ein Fenster, das z. B. auf einem inzwischen abgesteckten
    /// Bildschirm lag, auf den Hauptbildschirm. Ohne gemerkte Werte gilt die Standardgröße aus dem XAML,
    /// mittig auf dem Bildschirm. Ein maximiertes Fenster öffnet maximiert auf dem Bildschirm, auf dem
    /// seine gemerkte Position liegt.
    /// </remarks>
    private void StelleFensterpositionWiederHer()
    {
        if (_viewModel.GemerkteFensterposition is not Fensterposition gemerkt)
        {
            return;
        }

        Fensterposition position = PruefeFensterposition.Ausfuehren(gemerkt,
            new Bildschirmbereich(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight),
            new Bildschirmbereich(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top,
                SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height));

        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = position.Links;
        Top = position.Oben;
        Width = position.Breite;
        Height = position.Hoehe;
        if (position.Maximiert)
        {
            WindowState = WindowState.Maximized;
        }
    }

    /// <summary>
    /// Merkt beim Schließen die Spaltenanordnung der Tabelle, Größe, Position und Zustand des Fensters
    /// sowie die gewählte Karteikarte.
    /// </summary>
    /// <remarks>
    /// Ist das Fenster maximiert oder minimiert, liefert <see cref="Window.RestoreBounds"/> die Lage im
    /// normalen Zustand – dorthin kehrt es nach dem Wiederherstellen zurück.
    /// </remarks>
    private void Hauptfenster_Closing(object? sender, CancelEventArgs e)
    {
        Einheiten.MerkeAnordnung();

        Rect lage = WindowState == WindowState.Normal || RestoreBounds.IsEmpty
            ? new Rect(Left, Top, ActualWidth, ActualHeight)
            : RestoreBounds;
        _viewModel.MerkeFensterposition(new Fensterposition(lage.Left, lage.Top, lage.Width, lage.Height,
            WindowState == WindowState.Maximized));

        if (Karteikarten.SelectedIndex >= 0)
        {
            _viewModel.MerkeKarteikarte(Karteikarten.SelectedIndex);
        }
    }
}

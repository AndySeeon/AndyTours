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
/// Daten und Befehle liegen im <see cref="HauptfensterViewModel"/>. Hier stehen nur die Abläufe der beiden
/// Listen (Alarme und Wartungen): die Markierung an das ViewModel melden, Doppelklick und Entf an dessen Befehle
/// weitergeben und nach dem Speichern die gespeicherten Einträge markieren.
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

    /// <summary>Meldet sich beim (neuen) ViewModel an, um nach dem Speichern Einträge zu markieren.</summary>
    private void BeobachteViewModel()
    {
        if (_viewModel is not null)
        {
            _viewModel.MarkiereWartung -= ViewModel_MarkiereWartung;
            _viewModel.MarkiereAlarme -= ViewModel_MarkiereAlarme;
        }

        _viewModel = DataContext as HauptfensterViewModel;
        if (_viewModel is not null)
        {
            _viewModel.MarkiereWartung += ViewModel_MarkiereWartung;
            _viewModel.MarkiereAlarme += ViewModel_MarkiereAlarme;
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

    /// <summary>
    /// Meldet dem ViewModel die markierten Alarme, damit „Bearbeiten…“, „Löschen“ und „Ein-/Ausschalten“ richtig
    /// wählbar sind.
    /// </summary>
    private void AlarmTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, AlarmTabelle) || _viewModel is null)
        {
            return;
        }

        _viewModel.MarkierteAlarme = AlarmTabelle.SelectedItems.OfType<Alarmzeile>().Select(z => z.Id).ToList();
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

    /// <summary>
    /// Markiert die Zeilen von Alarmen (nach dem Speichern oder Ein-/Ausschalten) und holt die erste in den
    /// sichtbaren Bereich; Alarme, die das gewählte Rad ausblendet, werden übergangen.
    /// </summary>
    /// <param name="ids">Nummern der Alarme.</param>
    private void ViewModel_MarkiereAlarme(IReadOnlyCollection<long> ids)
    {
        if (_viewModel is null)
        {
            return;
        }

        List<Alarmzeile> zeilen = _viewModel.Alarmzeilen.Where(z => ids.Contains(z.Id)).ToList();
        AlarmTabelle.SelectedItems.Clear();
        foreach (Alarmzeile zeile in zeilen)
        {
            AlarmTabelle.SelectedItems.Add(zeile);
        }

        if (AlarmTabelle.Items.OfType<Alarmzeile>().FirstOrDefault(z => ids.Contains(z.Id)) is Alarmzeile erste)
        {
            AlarmTabelle.ScrollIntoView(erste);
        }
    }

    /// <summary>Doppelklick auf eine Wartung: öffnet das Wartungsfenster der markierten Wartung.</summary>
    private void Zeile_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        FuehreAusBeiDoppelklick(e, _viewModel?.WartungBearbeitenCommand);

    /// <summary>Doppelklick auf einen Alarm: öffnet das Alarmfenster des markierten Alarms.</summary>
    private void Alarmzeile_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        FuehreAusBeiDoppelklick(e, _viewModel?.AlarmBearbeitenCommand);

    /// <summary>Entf löscht die markierten Wartungen (nach der Rückfrage des ViewModels).</summary>
    private void Tabelle_PreviewKeyDown(object sender, KeyEventArgs e) =>
        FuehreAusBeiEntf(e, _viewModel?.WartungenLoeschenCommand);

    /// <summary>Entf löscht die markierten Alarme (nach der Rückfrage des ViewModels).</summary>
    private void AlarmTabelle_PreviewKeyDown(object sender, KeyEventArgs e) =>
        FuehreAusBeiEntf(e, _viewModel?.AlarmeLoeschenCommand);

    /// <summary>Führt bei einem Doppelklick mit der linken Maustaste einen Befehl aus, sofern er wählbar ist.</summary>
    private static void FuehreAusBeiDoppelklick(MouseButtonEventArgs e, ICommand? befehl)
    {
        if (befehl is null || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        e.Handled = true;
        if (befehl.CanExecute(null))
        {
            befehl.Execute(null);
        }
    }

    /// <summary>Führt bei der Entf-Taste einen Befehl aus, sofern er wählbar ist.</summary>
    private static void FuehreAusBeiEntf(KeyEventArgs e, ICommand? befehl)
    {
        if (e.Key != Key.Delete || befehl is null)
        {
            return;
        }

        e.Handled = true;
        if (befehl.CanExecute(null))
        {
            befehl.Execute(null);
        }
    }

    /// <summary>
    /// Rechtsklick auf eine nicht markierte Zeile markiert diese, damit sich das Rechtsklickmenü auf sie bezieht;
    /// bei einer schon markierten Zeile bleibt eine Mehrfachmarkierung erhalten.
    /// </summary>
    private void Liste_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        DependencyObject? element = e.OriginalSource as DependencyObject;
        while (element is not null and not DataGridRow)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is DataGridRow zeile && !zeile.IsSelected && sender is DataGrid liste)
        {
            liste.SelectedItem = zeile.Item;
        }
    }
}

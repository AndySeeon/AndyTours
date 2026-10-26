using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: das Fenster „Overlays“ öffnen.
/// </summary>
public partial class HauptfensterViewModel
{
    /// <summary>Das geöffnete Fenster „Overlays“ oder <c>null</c>.</summary>
    private OverlaysFenster? _overlaysFenster;

    /// <summary>
    /// Befehl „Tourenplanung → Overlays…“: öffnet die Overlay-Verwaltung (nicht modal); ist sie schon offen, wird sie
    /// nach vorn geholt.
    /// </summary>
    /// <remarks>
    /// Die Verwaltung arbeitet immer im aktuellen Datenordner; wechselt er, zeigt sie dessen Overlays
    /// (<see cref="Funktionen.Overlaybestand.Neugeladen"/>).
    /// </remarks>
    [RelayCommand]
    private void OverlaysOeffnen()
    {
        if (_overlaysFenster is not null)
        {
            if (_overlaysFenster.WindowState == WindowState.Minimized)
            {
                _overlaysFenster.WindowState = WindowState.Normal;
            }

            _overlaysFenster.Activate();
            return;
        }

        _overlaysFenster = new OverlaysFenster(new OverlaysViewModel(() => Datenordner)) { Owner = Application.Current.MainWindow };
        _overlaysFenster.Closed += (_, _) => _overlaysFenster = null;
        _overlaysFenster.Show();
    }
}

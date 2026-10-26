using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Ein Knoten im Baum der Overlay-Verwaltung: ein Ordner mit seinen Unterknoten oder ein Overlay.
/// </summary>
/// <remarks>
/// Das Kontrollkästchen ist bei einem Ordner dreistufig: angehakt, wenn alle Overlays darin (auf allen Ebenen)
/// eingeschaltet sind, leer, wenn keines eingeschaltet ist, sonst gefüllt. Ein Klick schaltet nicht selbst, sondern
/// meldet den Wunsch an das <see cref="OverlaysViewModel"/>, das speichert und danach die Zustände aller Knoten setzt
/// (<see cref="ZeigeZustand"/>). Ein Klick auf ein gefülltes Kästchen schaltet aus (Verhalten von WPF).
/// </remarks>
public sealed partial class Overlayknoten : ObservableObject
{
    /// <summary>Meldet den Wunsch, den Knoten ein- (<c>true</c>) oder auszuschalten (<c>false</c>).</summary>
    private readonly Action<Overlayknoten, bool> _umschalten;

    /// <summary>Zustand des Kontrollkästchens; <c>null</c> = teilweise eingeschaltet (nur bei Ordnern).</summary>
    private bool? _istEingeschaltet;

    /// <summary>Erzeugt den Knoten eines Ordners.</summary>
    /// <param name="ordner">Der Ordner.</param>
    /// <param name="umschalten">Meldet den Wunsch, den Knoten umzuschalten.</param>
    public Overlayknoten(Overlayordner ordner, Action<Overlayknoten, bool> umschalten)
    {
        Ordner = ordner;
        _umschalten = umschalten;
    }

    /// <summary>Erzeugt den Knoten eines Overlays.</summary>
    /// <param name="overlay">Das Overlay.</param>
    /// <param name="farbe">Pinsel des Farbfelds (eigene oder gemeinsame Farbe).</param>
    /// <param name="umschalten">Meldet den Wunsch, den Knoten umzuschalten.</param>
    public Overlayknoten(Overlay overlay, Brush farbe, Action<Overlayknoten, bool> umschalten)
    {
        Overlay = overlay;
        _farbe = farbe;
        _istEingeschaltet = overlay.IstEingeschaltet;
        _umschalten = umschalten;
    }

    /// <summary>Der Ordner, wenn der Knoten ein Ordner ist; sonst <c>null</c>.</summary>
    public Overlayordner? Ordner { get; }

    /// <summary>
    /// Das Overlay, wenn der Knoten ein Overlay ist; sonst <c>null</c>. Wird nach dem Umschalten oder einer neuen Farbe
    /// durch den gespeicherten Stand ersetzt, ohne den Baum neu aufzubauen.
    /// </summary>
    public Overlay? Overlay { get; set; }

    /// <summary><c>true</c> bei einem Ordner.</summary>
    public bool IstOrdner => Ordner is not null;

    /// <summary><c>true</c> bei einem Overlay (für die Sichtbarkeit des Farbfelds).</summary>
    public bool IstOverlay => Overlay is not null;

    /// <summary>Angezeigter Name.</summary>
    public string Name => Ordner?.Name ?? Overlay!.Name;

    /// <summary>Die Unterordner und Overlays eines Ordners: zuerst die Ordner, dann die Overlays, jeweils alphabetisch.</summary>
    public ObservableCollection<Overlayknoten> Kinder { get; } = [];

    /// <summary>Pinsel des Farbfelds eines Overlays.</summary>
    [ObservableProperty]
    private Brush? _farbe;

    /// <summary>Ob der Ordner aufgeklappt ist (bleibt beim Neuaufbau des Baums erhalten).</summary>
    [ObservableProperty]
    private bool _istAufgeklappt;

    /// <summary>Ob der Knoten im Baum markiert ist.</summary>
    [ObservableProperty]
    private bool _istAusgewaehlt;

    /// <summary>Wird ausgelöst, wenn der Knoten markiert wird.</summary>
    public event EventHandler? Ausgewaehlt;

    /// <summary>
    /// Zustand des Kontrollkästchens. Das Setzen (durch einen Klick) meldet nur den Wunsch zum Umschalten.
    /// </summary>
    public bool? IstEingeschaltet
    {
        get => _istEingeschaltet;
        set
        {
            if (value != _istEingeschaltet)
            {
                _umschalten(this, value == true);

                // Falls das Speichern scheitert, zeigt das Kästchen so wieder den tatsächlichen Zustand
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Zeigt den gespeicherten Zustand an, ohne ein Umschalten auszulösen.
    /// </summary>
    /// <param name="zustand">Eingeschaltet, ausgeschaltet oder (bei Ordnern) teilweise.</param>
    public void ZeigeZustand(bool? zustand) => SetProperty(ref _istEingeschaltet, zustand, nameof(IstEingeschaltet));

    /// <summary>Meldet die Markierung weiter.</summary>
    partial void OnIstAusgewaehltChanged(bool value)
    {
        if (value)
        {
            Ausgewaehlt?.Invoke(this, EventArgs.Empty);
        }
    }
}

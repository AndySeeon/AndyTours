using System.Windows.Media;
using AndyTours.App.Funktionen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Ein Eintrag der Farbauswahl für Overlays: Name mit Farbfeld.
/// </summary>
/// <param name="Farbe">Die Farbe; <c>null</c> beim Eintrag „Gemeinsame Farbe“ der Overlay-Verwaltung.</param>
/// <param name="Name">Angezeigter Name, z. B. „Türkis“ oder „Gemeinsame Farbe (Violett)“.</param>
/// <param name="Pinsel">Pinsel für das Farbfeld.</param>
public record Overlayfarbeintrag(Overlayfarbe? Farbe, string Name, Brush Pinsel)
{
    /// <summary>Die zwölf wählbaren Farben in der Reihenfolge der Aufzählung.</summary>
    /// <returns>Die Einträge.</returns>
    public static List<Overlayfarbeintrag> Alle() =>
        [.. Enum.GetValues<Overlayfarbe>().Select(f => new Overlayfarbeintrag(f, Overlaydarstellung.Anzeigename(f), ErzeugePinsel(f)))];

    /// <summary>Der Eintrag „Gemeinsame Farbe (…)“, der die eigene Farbe eines Overlays zurücksetzt.</summary>
    /// <param name="gemeinsam">Die aktuelle gemeinsame Overlay-Farbe.</param>
    /// <returns>Der Eintrag.</returns>
    public static Overlayfarbeintrag Gemeinsam(Overlayfarbe gemeinsam) =>
        new(null, $"Gemeinsame Farbe ({Overlaydarstellung.Anzeigename(gemeinsam)})", ErzeugePinsel(gemeinsam));

    /// <summary>Ein eingefrorener Pinsel einer Farbe (eingefroren: unveränderlich und schneller gezeichnet).</summary>
    /// <param name="farbe">Die Farbe.</param>
    /// <returns>Der Pinsel.</returns>
    public static Brush ErzeugePinsel(Overlayfarbe farbe)
    {
        var pinsel = new SolidColorBrush(Overlaydarstellung.Farbwert(farbe));
        pinsel.Freeze();
        return pinsel;
    }

    /// <summary>Für die Anzeige ohne Vorlage.</summary>
    /// <returns>Der Name.</returns>
    public override string ToString() => Name;
}

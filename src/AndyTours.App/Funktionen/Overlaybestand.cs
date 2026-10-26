// WPF-Projekte binden System.IO nicht automatisch ein
using System.IO;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Gemeinsamer Stand der Overlays für alle Karten: die eingeschalteten Overlays und die gemeinsame Overlay-Farbe.
/// </summary>
/// <remarks>
/// Die Kartenfenster (Tourdetail, Mehrtagestour, Tourenplanung) lesen von hier, welche Overlays sie zeichnen, und
/// melden sich bei <see cref="Geaendert"/> an. So zeigt jede offene Karte sofort, was in der Overlay-Verwaltung oder in
/// den Einstellungen geändert wird. Die Fenster müssen sich beim Schließen wieder abmelden, weil die Ereignisse
/// statisch sind (wie <see cref="Kartenabruf.Fehlgeschlagen"/>).
/// <para>
/// Geladen wird beim Öffnen eines Datenordners (Start, Wechsel, Wiederherstellung einer Sicherung); danach meldet die
/// Overlay-Verwaltung jede Änderung über <see cref="Uebernimm"/>, die Einstellungen über <see cref="SetzeFarbe"/>.
/// </para>
/// </remarks>
public static class Overlaybestand
{
    /// <summary>Die eingeschalteten Overlays mit ihren Punkten.</summary>
    public static IReadOnlyList<Overlay> Eingeschaltete { get; private set; } = [];

    /// <summary>Die gemeinsame Farbe der Overlays ohne eigene Farbe.</summary>
    public static Overlayfarbe Farbe { get; private set; } = OverlayfarbeEinstellung.Standard;

    /// <summary>Wird ausgelöst, wenn sich die eingeschalteten Overlays oder eine Farbe geändert haben.</summary>
    public static event EventHandler? Geaendert;

    /// <summary>
    /// Wird ausgelöst, wenn die Karten die ganze Strecke eines Overlays zeigen sollen (Doppelklick in der Verwaltung).
    /// </summary>
    public static event EventHandler<Overlay>? GanzesOverlayAngefordert;

    /// <summary>
    /// Wird ausgelöst, nachdem ein Datenordner geladen wurde, damit eine offene Overlay-Verwaltung dessen Overlays zeigt.
    /// </summary>
    public static event EventHandler? Neugeladen;

    /// <summary>
    /// Liest die eingeschalteten Overlays und die gemeinsame Farbe aus der Datenbank des Datenordners.
    /// </summary>
    /// <remarks>
    /// Ist die Datenbank nicht lesbar, gibt es keine eingeschalteten Overlays; die Karten funktionieren trotzdem.
    /// Danach werden <see cref="Neugeladen"/> und <see cref="Geaendert"/> ausgelöst.
    /// </remarks>
    /// <param name="datenordner">Der geöffnete Datenordner.</param>
    public static void Laden(string datenordner)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei));
            Eingeschaltete = [.. OverlaysTabelle.Laden(verbindung).Where(o => o.IstEingeschaltet)];
            Farbe = OverlayfarbeEinstellung.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Eingeschaltete = [];
            Farbe = OverlayfarbeEinstellung.Standard;
        }

        Neugeladen?.Invoke(null, EventArgs.Empty);
        Geaendert?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Übernimmt den aktuellen Stand aller Overlays aus der Verwaltung und meldet ihn den Karten.
    /// </summary>
    /// <param name="alle">Alle Overlays; eingeblendet werden die eingeschalteten.</param>
    public static void Uebernimm(IEnumerable<Overlay> alle)
    {
        Eingeschaltete = [.. alle.Where(o => o.IstEingeschaltet)];
        Geaendert?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Übernimmt eine neue gemeinsame Farbe aus den Einstellungen und meldet sie den Karten.
    /// </summary>
    /// <param name="farbe">Die neue Farbe.</param>
    public static void SetzeFarbe(Overlayfarbe farbe)
    {
        Farbe = farbe;
        Geaendert?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Fordert die offenen Karten auf, die ganze Strecke eines Overlays zu zeigen.
    /// </summary>
    /// <param name="overlay">Das Overlay.</param>
    public static void ZeigeGanz(Overlay overlay) => GanzesOverlayAngefordert?.Invoke(null, overlay);
}

using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Die aus <c>bikexperience.ini</c> übernommenen Stammdaten.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="LeseBikexperienceIni"/>. Oberflächen-Einstellungen des Originals
/// (Fenstergrößen, Farben der Diagramme, Proxy, Dropbox-Zugang …) werden bewusst nicht übernommen.
/// </remarks>
public class AltEinstellungen
{
    /// <summary>Der Sportler aus <c>[Biker]</c>.</summary>
    public Fahrer Fahrer { get; set; } = new();

    /// <summary>Aktivitäten aus <c>[Activity]</c>.</summary>
    public List<Aktivitaet> Aktivitaeten { get; set; } = new();

    /// <summary>Kategorien aus <c>[Category]</c>.</summary>
    public List<Kategorie> Kategorien { get; set; } = new();

    /// <summary>Equipments aus <c>[Equipment.*]</c>.</summary>
    public List<Equipment> Equipments { get; set; } = new();

    /// <summary>Schwellwerte aus <c>[Limits*]</c>.</summary>
    public List<Schwellwert> Schwellwerte { get; set; } = new();
}

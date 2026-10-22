namespace AndyTours.Core.Modell;

/// <summary>
/// Der gesamte Datenbestand von AndyTours in einem Objekt.
/// </summary>
/// <remarks>
/// Wird verwendet, um alle Daten auf einmal zu speichern (z. B. nach der Übernahme der Altdaten)
/// oder zu laden. Bei rund tausend Trainingseinheiten passt alles bequem in den Arbeitsspeicher,
/// deshalb arbeitet die Oberfläche mit diesem Gesamtbestand statt mit einzelnen Abfragen.
/// </remarks>
public class Datenbestand
{
    /// <summary>Alle Sportler.</summary>
    public List<Fahrer> Fahrer { get; set; } = new();

    /// <summary>Alle Aktivitäten.</summary>
    public List<Aktivitaet> Aktivitaeten { get; set; } = new();

    /// <summary>Alle Kategorien.</summary>
    public List<Kategorie> Kategorien { get; set; } = new();

    /// <summary>Alle Equipments, auch ausgemusterte.</summary>
    public List<Equipment> Equipments { get; set; } = new();

    /// <summary>Alle Trainingseinheiten.</summary>
    public List<Trainingseinheit> Trainingseinheiten { get; set; } = new();

    /// <summary>Alle Wartungseinträge und Alarme.</summary>
    public List<Wartung> Wartungen { get; set; } = new();

    /// <summary>Alle Schwellwerte (Puls-, Geschwindigkeits-, Steigungs- und Leistungsbereiche).</summary>
    public List<Schwellwert> Schwellwerte { get; set; } = new();
}

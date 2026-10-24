namespace AndyTours.Core.Modell;

/// <summary>
/// Die Einträge der Auswahllisten Aktivität, Equipment und Trainingsbereich beim Bearbeiten einer Einheit.
/// </summary>
/// <remarks>Ergebnis von <see cref="Berechnungen.ErmittleEingabelisten"/>.</remarks>
/// <param name="Aktivitaeten">Alle Aktivitäten.</param>
/// <param name="Equipments">„(kein)“, dann die angebotenen Equipments.</param>
/// <param name="Trainingsbereiche">Ein leerer Eintrag, dann die vorkommenden Kürzel.</param>
public record Eingabelisten(
    IReadOnlyList<Auswahleintrag> Aktivitaeten,
    IReadOnlyList<Auswahleintrag> Equipments,
    IReadOnlyList<string> Trainingsbereiche)
{
    /// <summary>Name des Eintrags „kein Equipment“.</summary>
    public const string KeinEquipment = "(kein)";
}

namespace AndyTours.Core.Modell;

/// <summary>
/// Die Einträge der Auswahllisten Kategorie, Equipment und Trainingsbereich in der Filterleiste.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.ErmittleFilterwerte"/>. Jede Liste beginnt mit <see cref="Alle"/>
/// (kein Filter); danach folgen die Werte alphabetisch.
/// </remarks>
/// <param name="Kategorien">Einträge der Kategorieliste.</param>
/// <param name="Equipments">Einträge der Equipmentliste.</param>
/// <param name="Trainingsbereiche">Einträge der Trainingsbereichsliste.</param>
public record Filterwerte(IReadOnlyList<string> Kategorien, IReadOnlyList<string> Equipments, IReadOnlyList<string> Trainingsbereiche)
{
    /// <summary>Eintrag „kein Filter“ am Anfang jeder Liste.</summary>
    public const string Alle = "(alle)";
}

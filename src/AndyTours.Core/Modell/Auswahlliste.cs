namespace AndyTours.Core.Modell;

/// <summary>
/// Die Auswahllisten mit festen Werten, die der Benutzer in den Einstellungen pflegt.
/// </summary>
/// <remarks>
/// Der Name jedes Werts ist Teil des Schlüssels, unter dem die Liste gespeichert wird
/// („Auswahlliste.Trainingsbereich“). Werte daher nicht umbenennen – sonst wäre die gespeicherte
/// Liste nicht mehr auffindbar. Weitere Listen mit festen Werten kommen als neue Werte hinzu.
/// <para>
/// Aktivitäten, Kategorien und Equipments sind eigene Tabellen mit Id; für sie gilt der Schlüssel nicht – ihre
/// Einträge stehen in den Tabellen selbst, die Reihenfolge der Aktivitäten unter einem eigenen Schlüssel.
/// </para>
/// </remarks>
public enum Auswahlliste
{
    /// <summary>Kürzel der Trainingsbereiche, z. B. „GA1“.</summary>
    Trainingsbereich,

    /// <summary>Aktivitäten (Sportarten), z. B. „Bike“; Reihenfolge vom Benutzer festgelegt.</summary>
    Aktivitaet,

    /// <summary>Kategorien der Einheiten, z. B. „Urlaub“; alphabetisch angezeigt.</summary>
    Kategorie,

    /// <summary>Räder und Ausrüstung, z. B. „Genius“; alphabetisch angezeigt (ausgemusterte am Ende) und in einem eigenen Fenster bearbeitet.</summary>
    Equipment,
}

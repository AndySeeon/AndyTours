namespace AndyTours.Core.Modell;

/// <summary>
/// Die Auswahllisten mit festen Werten, die der Benutzer in den Einstellungen pflegt.
/// </summary>
/// <remarks>
/// Der Name jedes Werts ist Teil des Schlüssels, unter dem die Liste gespeichert wird
/// („Auswahlliste.Trainingsbereich“). Werte daher nicht umbenennen – sonst wäre die gespeicherte
/// Liste nicht mehr auffindbar. Weitere Listen mit festen Werten kommen als neue Werte hinzu.
/// </remarks>
public enum Auswahlliste
{
    /// <summary>Kürzel der Trainingsbereiche, z. B. „GA1“.</summary>
    Trainingsbereich,
}

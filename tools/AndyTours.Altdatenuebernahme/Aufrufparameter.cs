namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Die beim Aufruf des Programms festgelegten Ordner (siehe <see cref="LeseAufrufparameter"/>).
/// </summary>
/// <param name="Quellordner">Vollständiger Pfad des BikeXperience-Datenordners (wird nur gelesen).</param>
/// <param name="Zielordner">Vollständiger Pfad des AndyTours-Datenordners, der befüllt wird.</param>
public record Aufrufparameter(string Quellordner, string Zielordner);

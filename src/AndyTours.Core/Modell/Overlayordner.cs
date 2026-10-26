namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Ordner der Overlay-Verwaltung; Ordner können Unterordner und Overlays enthalten.
/// </summary>
/// <param name="Id">Nummer in der Datenbank.</param>
/// <param name="ElternId">Nummer des übergeordneten Ordners; <c>null</c> = Ordner auf oberster Ebene.</param>
/// <param name="Name">Angezeigter Name; im selben übergeordneten Ordner eindeutig (ohne Rücksicht auf Groß-/Kleinschreibung).</param>
public record Overlayordner(long Id, long? ElternId, string Name);

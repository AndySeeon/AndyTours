namespace AndyTours.Core.Modell;

/// <summary>
/// Größe, Position und Zustand des Hauptfensters, wie sie beim Beenden gemerkt werden.
/// </summary>
/// <remarks>
/// Alle Werte in geräteunabhängigen Pixeln (wie in WPF). Ist das Fenster maximiert, beschreiben
/// Links/Oben/Breite/Höhe die Lage vor dem Maximieren – dorthin kehrt es beim Wiederherstellen zurück.
/// </remarks>
/// <param name="Links">Linker Rand.</param>
/// <param name="Oben">Oberer Rand.</param>
/// <param name="Breite">Breite.</param>
/// <param name="Hoehe">Höhe.</param>
/// <param name="Maximiert"><c>true</c>, wenn das Fenster maximiert war.</param>
public record Fensterposition(double Links, double Oben, double Breite, double Hoehe, bool Maximiert);

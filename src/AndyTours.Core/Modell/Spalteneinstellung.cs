namespace AndyTours.Core.Modell;

/// <summary>
/// Gespeicherte Anordnung einer Spalte der Tabelle „Trainingseinheiten“.
/// </summary>
/// <remarks>
/// Eine Liste dieser Einträge in Anzeigereihenfolge beschreibt Reihenfolge und Breiten aller Spalten
/// (siehe <see cref="Berechnungen.VervollstaendigeSpaltenanordnung"/>).
/// </remarks>
/// <param name="Spalte">Die Spalte.</param>
/// <param name="Breite">Breite in geräteunabhängigen Pixeln; <c>null</c> = automatisch an den Inhalt angepasst.</param>
public record Spalteneinstellung(Einheitenspalte Spalte, double? Breite);

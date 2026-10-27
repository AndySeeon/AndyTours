namespace AndyTours.Core.Modell;

/// <summary>
/// Alles, was die Karteikarte „Aktuell“ für einen Wert zeigt: die Jahrespunkte des Diagramms und die
/// Kennzahlen des laufenden Jahres.
/// </summary>
/// <param name="Wert">Der gezeigte Wert.</param>
/// <param name="Punkte">Ein Punkt je Jahr, aufsteigend; leer, wenn die Auswahl keine Einheiten enthält.</param>
/// <param name="Kennzahlen">Die Kennzahlen des laufenden Jahres.</param>
public record Jahresfortschritt(Fortschrittswert Wert, IReadOnlyList<Jahrespunkt> Punkte, Jahreskennzahlen Kennzahlen);

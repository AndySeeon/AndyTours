namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Jahresrekord: das Jahr mit der höchsten Jahressumme eines Werts.
/// </summary>
/// <param name="Wert">Der Wert (Kilometer, Höhenmeter, Anzahl oder Fahrzeit).</param>
/// <param name="Jahr">Das Rekordjahr.</param>
/// <param name="Summe">Die Jahressumme in der Einheit des Werts (Fahrzeit in Stunden).</param>
public record Jahresrekord(Fortschrittswert Wert, int Jahr, double Summe);

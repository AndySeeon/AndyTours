namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Zeile der Auskunft „Equipment-Einsatz“: was ein Equipment insgesamt gefahren wurde.
/// </summary>
/// <remarks>
/// Ein Wert, der gerundet 0 ergibt, ist <c>null</c> – die Zelle bleibt dann leer statt „0“ zu zeigen.
/// Die Werte selbst sind nicht gerundet; das übernimmt das Zahlenformat der Anzeige.
/// </remarks>
/// <param name="Name">Angezeigter Name des Equipments (ggf. mit „(ausgemustert)“), „ohne Equipment“ oder „Summe“.</param>
/// <param name="VorherKm">Anfangskilometer des Equipments (vor der Erfassung in AndyTours gefahren).</param>
/// <param name="GesamtKm">Anfangskilometer plus die Distanzen aller Einheiten; entspricht dem km-Stand.</param>
/// <param name="GesamtHm">Summe der Höhenmeter aller Einheiten.</param>
/// <param name="IstSumme"><c>true</c> für die Zeile „Summe“, die beim Sortieren immer unten bleibt.</param>
public record Einsatzzeile(string Name, double? VorherKm, double? GesamtKm, int? GesamtHm, bool IstSumme = false);

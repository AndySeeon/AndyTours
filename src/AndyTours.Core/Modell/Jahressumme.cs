namespace AndyTours.Core.Modell;

/// <summary>
/// Aufsummierte Kennwerte aller Trainingseinheiten eines Jahres.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.BerechneJahressummen"/>. Wird u. a. verwendet, um nach der
/// Übernahme der Altdaten zu prüfen, dass die Summen mit dem Original übereinstimmen.
/// </remarks>
/// <param name="Jahr">Kalenderjahr.</param>
/// <param name="Anzahl">Anzahl der Trainingseinheiten.</param>
/// <param name="DistanzKm">Summe der Distanzen in Kilometern.</param>
/// <param name="Hoehenmeter">Summe der Höhenmeter.</param>
/// <param name="Fahrzeit">Summe der Fahrzeiten.</param>
public record Jahressumme(int Jahr, int Anzahl, double DistanzKm, int Hoehenmeter, TimeSpan Fahrzeit);

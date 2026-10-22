namespace AndyTours.Core.Modell;

/// <summary>
/// Summen der im Hauptfenster gewählten Trainingseinheiten (Jahr und Aktivität).
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.BerechneAuswahlsummen"/>; wird oben neben der Auswahl angezeigt.
/// </remarks>
/// <param name="Anzahl">Anzahl der Trainingseinheiten.</param>
/// <param name="DistanzKm">Summe der Distanzen in Kilometern.</param>
/// <param name="Hoehenmeter">Summe der Höhenmeter.</param>
public record Auswahlsumme(int Anzahl, double DistanzKm, int Hoehenmeter);

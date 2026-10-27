namespace AndyTours.Core.Modell;

/// <summary>
/// Der durchgehende Verlauf aller Etappen für das Höhenprofil des Tourdetails.
/// </summary>
/// <remarks>
/// Die Messpunkte der Etappen stehen hintereinander; Distanz und Fahrzeit jeder Etappe sind um das Ende der vorigen
/// verschoben, sodass sie über die Etappen weiterzählen und Nächte und Pausen dazwischen entfallen. Es sind nur
/// Etappen mit Verlauf enthalten. Zu jeder davon steht in <see cref="Beginn"/> die Stelle ihres ersten Messpunkts:
/// Die Messpunkte der Etappe <c>i</c> reichen von <c>Beginn[i]</c> bis vor <c>Beginn[i + 1]</c> bzw. bis zum Ende.
/// </remarks>
/// <param name="Punkte">Alle Messpunkte, Distanz und Fahrzeit durchgehend.</param>
/// <param name="Etappen">Die Etappen mit Verlauf, in ihrer Reihenfolge.</param>
/// <param name="Beginn">Je Etappe die Stelle ihres ersten Messpunkts in <see cref="Punkte"/>.</param>
public record Mehrtagesverlauf(IReadOnlyList<Verlaufspunkt> Punkte, IReadOnlyList<Etappe> Etappen, IReadOnlyList<int> Beginn);

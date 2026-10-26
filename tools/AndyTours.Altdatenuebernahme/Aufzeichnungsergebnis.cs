namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Ergebnis der Übernahme von Aufzeichnungen als Touren (siehe <see cref="ErstelleAufzeichnungsordner"/>).
/// </summary>
/// <param name="Gefunden">Anzahl der gefundenen FIT-Dateien.</param>
/// <param name="Uebernommen">Anzahl der Touren, die im Datenordner angelegt wurden.</param>
/// <param name="Uebersprungen">
/// Die nicht übernommenen Dateien mit Grund, z. B. nicht lesbar oder dieselbe Tour aus einer anderen Datei.
/// <c>Datei</c> ist der Pfad relativ zum Ordner mit den Aufzeichnungen.
/// </param>
public record Aufzeichnungsergebnis(
    int Gefunden,
    int Uebernommen,
    IReadOnlyList<(string Datei, string Grund)> Uebersprungen);

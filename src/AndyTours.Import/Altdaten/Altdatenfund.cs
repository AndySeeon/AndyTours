namespace AndyTours.Import.Altdaten;

/// <summary>
/// Ergebnis von <see cref="ErkenneAltdaten"/>: was im Ordner liegt und welche Aufzeichnungen gefunden wurden.
/// </summary>
/// <param name="Art">Die erkannte Art der bisherigen Daten.</param>
/// <param name="Aufzeichnungen">
/// Vollständige Pfade der gefundenen FIT-Dateien, sortiert; nur bei <see cref="Altdatenart.NurAufzeichnungen"/>
/// gefüllt, sonst leer.
/// </param>
public record Altdatenfund(Altdatenart Art, IReadOnlyList<string> Aufzeichnungen);

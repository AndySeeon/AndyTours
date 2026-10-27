using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Findet die Aufzeichnung einer Einheit, aus der das Tourdetail gelesen wird.
/// </summary>
public static class FindeAufzeichnung
{
    /// <summary>
    /// Liefert den Pfad der Aufzeichnung relativ zum Datenordner.
    /// </summary>
    /// <remarks>
    /// Bevorzugt wird der Anhang „Höhenprofil“, sonst der „GPS-Track“; bei Garmin-Aufzeichnungen zeigen beide
    /// meist auf dieselbe Datei. Eine Dokumentation zählt nicht, ebenso ein Anhang ohne Pfad.
    /// </remarks>
    /// <param name="einheit">Die Einheit.</param>
    /// <returns>Der relative Pfad, z. B. „2026/2026-09-26-08-02-49.fit“, oder <c>null</c> ohne Aufzeichnung.</returns>
    public static string? Ausfuehren(Trainingseinheit einheit) =>
        Pfad(einheit, AnhangTyp.Hoehenprofil) ?? Pfad(einheit, AnhangTyp.GpsTrack);

    /// <summary>Pfad des ersten Anhangs dieser Art mit Pfad.</summary>
    private static string? Pfad(Trainingseinheit einheit, AnhangTyp typ) =>
        einheit.Anhaenge.FirstOrDefault(a => a.Typ == typ && !string.IsNullOrWhiteSpace(a.RelativerPfad))?.RelativerPfad;
}

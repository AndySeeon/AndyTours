namespace AndyTours.Import.Garmin;

/// <summary>
/// Eine Aufzeichnung, die auf einem angeschlossenen Garmin-Gerät liegt.
/// </summary>
/// <remarks>
/// Edge 800 und Edge 1000 erscheinen als Laufwerk, der Edge 1040 nur als Mediengerät. Beide Wege liefern diese
/// gemeinsame Form, damit der weitere Import nicht wissen muss, woher die Datei kommt.
/// </remarks>
/// <param name="Dateiname">Dateiname auf dem Gerät, z. B. „2026-09-26-08-02-49.fit“.</param>
/// <param name="Geraet">Name des Geräts, z. B. „Edge 1000“.</param>
/// <param name="KopiereNach">Kopiert die Aufzeichnung in die angegebene Datei (vollständiger Pfad).</param>
public record Geraeteaufzeichnung(string Dateiname, string Geraet, Action<string> KopiereNach);

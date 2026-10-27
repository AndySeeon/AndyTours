namespace AndyTours.Import.Garmin;

/// <summary>
/// Ein angeschlossenes Garmin-Gerät als Ziel für Courses (Spec 032).
/// </summary>
/// <remarks>
/// Courses kommen in den Ordner <c>Garmin\NewFiles</c>; nach dem Trennen vom Computer übernimmt der Edge sie unter
/// „Courses“. Edge 800 und Edge 1000 erscheinen als Laufwerk (<see cref="FindeLaufwerksziele"/>), der Edge 1040 nur als
/// Mediengerät (im App-Projekt). Beide Wege liefern diese gemeinsame Form, damit die Übertragung nicht wissen muss,
/// wie das Gerät angeschlossen ist. Beide Aktionen werfen bei einem Gerätefehler eine <see cref="IOException"/>.
/// </remarks>
/// <param name="Geraet">Name des Geräts, z. B. „Edge 1040“.</param>
/// <param name="DateiVorhanden">Prüft, ob in <c>Garmin\NewFiles</c> schon eine Datei dieses Namens wartet.</param>
/// <param name="Schreibe">Schreibt eine Datei (Name, Inhalt) nach <c>Garmin\NewFiles</c>; eine vorhandene wird ersetzt.</param>
public record Courseziel(string Geraet, Func<string, bool> DateiVorhanden, Action<string, byte[]> Schreibe);

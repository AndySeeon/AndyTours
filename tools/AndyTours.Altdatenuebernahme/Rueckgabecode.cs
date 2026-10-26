namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Die Rückgabecodes des Programms.
/// </summary>
/// <remarks>
/// Das Installationsprogramm (Spezifikation 033) ruft das Programm auf und entscheidet anhand dieser Codes,
/// welchen Hinweis oder welche Rückfrage es zeigt. Die Werte dürfen sich deshalb nicht ändern.
/// </remarks>
public static class Rueckgabecode
{
    /// <summary>Umwandlung bzw. Übernahme erfolgreich.</summary>
    public const int Erfolg = 0;

    /// <summary>Fehler oder abgebrochen; der Zielordner ist unverändert, der Grund steht in der Fehlerausgabe.</summary>
    public const int Fehler = 1;

    /// <summary>Im Zielordner liegen schon AndyTours-Daten; sie bleiben unverändert (nur mit <c>--ohne-rueckfrage</c>).</summary>
    public const int ZielHatDaten = 2;

    /// <summary>Ergebnis von <c>--erkennen</c>: Im Ordner liegen BikeXperience-Daten.</summary>
    public const int BikeXperience = 10;

    /// <summary>Ergebnis von <c>--erkennen</c>: Im Ordner liegen nur Aufzeichnungen; ihre Anzahl steht in der Ausgabe.</summary>
    public const int NurAufzeichnungen = 11;

    /// <summary>Ergebnis von <c>--erkennen</c>: Im Ordner liegt nichts Übernehmbares.</summary>
    public const int Nichts = 12;
}

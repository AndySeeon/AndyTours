namespace AndyTours.Import.Altdaten;

/// <summary>
/// Korrekturliste für Wörter, deren Umlaute in den Altdaten durch das Ersatzzeichen „�" verloren gegangen sind.
/// </summary>
/// <remarks>
/// Die Einträge wurden aus den tatsächlich vorkommenden beschädigten Wörtern in <c>workouts.csv</c>,
/// <c>maintanence.dat</c> und <c>bikexperience.ini</c> zusammengestellt. Bei Ortsnamen wurde die
/// richtige Schreibweise u. a. aus den unbeschädigten Latin-1-Einträgen von 2025/2026 abgeleitet
/// (dort steht z. B. eindeutig „Bründling" und „Rötelmoos").
/// <para>
/// Es werden Wortteile ersetzt, nicht ganze Wörter. So deckt z. B. „h�tte" auch „Burgerh�tte" ab.
/// <see cref="RepariereUmlaute"/> wendet längere Einträge zuerst an, damit spezielle Einträge
/// Vorrang vor allgemeinen haben.
/// </para>
/// <para>
/// „B�ckeralm" und „(�PD)" waren nicht eindeutig; die richtige Schreibweise („Bäckeralm",
/// „ÜPD") hat der Benutzer festgelegt (Spezifikation 002). Neue Einträge können einfach unten
/// ergänzt werden.
/// </para>
/// </remarks>
public static class UmlautKorrekturliste
{
    /// <summary>
    /// Paare aus beschädigtem Wortteil und korrigierter Schreibweise.
    /// </summary>
    public static IReadOnlyList<(string Beschaedigt, string Korrigiert)> Eintraege { get; } = new List<(string, string)>
    {
        // --- Orts- und Tourennamen (workouts.csv) ---
        ("Br�ndling", "Bründling"),
        ("R�thelmoos", "Röthelmoos"),
        ("R�telmoos", "Rötelmoos"),
        ("Eggst�tt", "Eggstätt"),
        ("Eggst�dt", "Eggstädt"),
        ("M�seralm", "Möseralm"),
        ("K�ssen", "Kössen"),
        ("Unterw�ssen", "Unterwössen"),
        ("St�ffling", "Stöffling"),
        ("M�nchen", "München"),
        ("B�ckeralm", "Bäckeralm"),   // „Stoisser Bäckeralm" (vom Benutzer bestätigt)
        ("(�PD)", "(ÜPD)"),           // „Arbeit KW … (ÜPD)" (vom Benutzer bestätigt)

        // --- allgemeine Wörter ---
        ("H�tte", "Hütte"),
        ("h�tte", "hütte"),
        ("H�he", "Höhe"),
        ("�ber", "über"),      // auch in „Hartseest�berl" → „Hartseestüberl"
        ("Br�cke", "Brücke"),
        ("zur�ck", "zurück"),
        ("hei�", "heiß"),
        ("Fu�", "Fuß"),
        ("�C", "°C"),          // „0�C" → „0°C"
        ("40�", "40°"),        // „40� in der Sonne"

        // --- Wartungseinträge (maintanence.dat) ---
        ("D�mpfer", "Dämpfer"),
        ("f�r", "für"),
        ("Schaltr�llchen", "Schaltröllchen"),
        ("Kettenbl�tter", "Kettenblätter"),
        ("Bremsfl�ssigkeit", "Bremsflüssigkeit"),
        ("Z�hne", "Zähne"),

        // --- bikexperience.ini: hier wurde das „ü" sogar zu einem Leerzeichen ---
        ("H gel", "Hügel"),
    };
}

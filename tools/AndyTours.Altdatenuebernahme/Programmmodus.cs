namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Was das Programm bei diesem Aufruf tut (siehe <see cref="LeseAufrufparameter"/>).
/// </summary>
public enum Programmmodus
{
    /// <summary>BikeXperience-Daten in einen AndyTours-Datenordner umwandeln (bisheriger Aufruf, Spezifikation 002).</summary>
    Umwandeln,

    /// <summary>Nur feststellen, was in einem Ordner liegt (<c>--erkennen</c>); es wird nichts geschrieben.</summary>
    Erkennen,

    /// <summary>Aufzeichnungen im FIT-Format als Touren in einen AndyTours-Datenordner übernehmen (<c>--aufzeichnungen</c>).</summary>
    Aufzeichnungen,
}

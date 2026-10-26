namespace AndyTours.Core.Modell;

/// <summary>
/// Eine gespeicherte Strecke, die zusätzlich auf den Karten eingeblendet werden kann (z. B. eine geplante Tour oder
/// eine fremde GPX-Strecke).
/// </summary>
/// <remarks>
/// Die Punkte werden beim Hinzufügen aus der GPX-Datei übernommen und in der Datenbank gespeichert; die Datei selbst
/// wird danach nicht mehr gebraucht. Jeder Abschnitt (z. B. ein Track-Segment) wird als eigene Linie gezeichnet, damit
/// zwischen den Abschnitten kein Verbindungsstrich entsteht.
/// </remarks>
/// <param name="Id">Nummer in der Datenbank.</param>
/// <param name="OrdnerId">Nummer des Ordners; <c>null</c> = auf oberster Ebene.</param>
/// <param name="Name">Angezeigter Name, anfangs der Dateiname ohne Endung.</param>
/// <param name="Farbe">Eigene Farbe; <c>null</c> = die gemeinsame Overlay-Farbe.</param>
/// <param name="IstEingeschaltet"><c>true</c>, wenn das Overlay auf den Karten eingeblendet ist.</param>
/// <param name="Abschnitte">Die Abschnitte der Strecke; jeder mit mindestens einem Punkt.</param>
public record Overlay(
    long Id,
    long? OrdnerId,
    string Name,
    Overlayfarbe? Farbe,
    bool IstEingeschaltet,
    IReadOnlyList<IReadOnlyList<Geoposition>> Abschnitte);

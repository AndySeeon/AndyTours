namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Ergebnis der Prüfung eines Datenordners (siehe <see cref="PruefeDatenordner"/>).
/// </summary>
public enum DatenordnerZustand
{
    /// <summary>Der Ordner ist erreichbar, beschreibbar und enthält bereits eine AndyTours-Datenbank.</summary>
    MitDaten,

    /// <summary>Der Ordner ist erreichbar und beschreibbar, enthält aber noch keine AndyTours-Datenbank.</summary>
    Leer,

    /// <summary>Der Ordner ist erreichbar, aber die App darf dort nicht schreiben.</summary>
    NichtBeschreibbar,

    /// <summary>
    /// Der Ordner ist nicht erreichbar: Er existiert nicht und lässt sich auch nicht anlegen
    /// (z. B. NAS ausgeschaltet, Laufwerk fehlt), oder die Prüfung hat das Zeitlimit überschritten.
    /// </summary>
    NichtErreichbar,
}

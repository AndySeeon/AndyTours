namespace AndyTours.Daten;

/// <summary>
/// Schlüssel der Einträge in der Tabelle <c>Einstellung</c>.
/// </summary>
/// <remarks>
/// Die Schlüssel tragen den Bereich als Vorsilbe („Sicherung.…“), damit spätere Einstellungen
/// (z. B. persönliche Daten) eindeutig bleiben.
/// </remarks>
public static class Einstellungsschluessel
{
    /// <summary>Vom Benutzer gewählter Sicherungsordner; fehlt der Eintrag, gilt der Standardordner.</summary>
    public const string Sicherungsordner = "Sicherung.Ordner";

    /// <summary>Sicherungsintervall in Programmstarts.</summary>
    public const string Sicherungsintervall = "Sicherung.Intervall";

    /// <summary>Höchstzahl der aufbewahrten Sicherungen.</summary>
    public const string Sicherungshoechstzahl = "Sicherung.Hoechstzahl";

    /// <summary>Programmstarts seit der letzten erfolgreichen Sicherung.</summary>
    public const string StartsSeitSicherung = "Sicherung.StartsSeitSicherung";

    /// <summary>In der Tabelle „Trainingseinheiten“ ausgeblendete Spalten, kommagetrennt; fehlt der Eintrag, sind alle sichtbar.</summary>
    public const string AusgeblendeteSpalten = "Einheitentabelle.AusgeblendeteSpalten";

    /// <summary>Reihenfolge und Breiten der Spalten der Tabelle „Trainingseinheiten“, z. B. „Datum:96,Startzeit:,Name:250“.</summary>
    public const string Spaltenanordnung = "Einheitentabelle.Spalten";

    /// <summary>Größe, Position und Zustand des Hauptfensters: „Links;Oben;Breite;Höhe;Maximiert“.</summary>
    public const string Fensterposition = "Hauptfenster.Position";

    /// <summary>Zuletzt gewählte Karteikarte des Hauptfensters als Nummer ab 0, z. B. „1“ für „Trainingseinheiten“.</summary>
    public const string Karteikarte = "Hauptfenster.Karteikarte";

    /// <summary>Reihenfolge der Aktivitäten als Ids, durch Semikolon getrennt, z. B. „2;1“.</summary>
    public const string AktivitaetenReihenfolge = "Aktivitaeten.Reihenfolge";

    /// <summary>Zuletzt für „Als GPX exportieren…“ verwendeter Ordner.</summary>
    public const string GpxOrdner = "GpxExport.Ordner";

    /// <summary>Zuletzt gewähltes Fahrprofil der Tourenplanung, z. B. „Trekking“.</summary>
    public const string PlanungProfil = "Tourenplanung.Profil";

    /// <summary>Zuletzt gezeigter Kartenausschnitt der Tourenplanung: „Südbreite;Westlänge;Nordbreite;Ostlänge“.</summary>
    public const string PlanungAusschnitt = "Tourenplanung.Ausschnitt";

    /// <summary>Zuletzt zum Speichern oder Öffnen einer Planung verwendeter Ordner.</summary>
    public const string PlanungOrdner = "Tourenplanung.Ordner";

    /// <summary>Gewählter Routingdienst, z. B. „BRouter“; fehlt der Eintrag, gilt BRouter.</summary>
    public const string RoutingDienst = "Routing.Dienst";

    /// <summary>Adresse des BRouter-Servers; fehlt der Eintrag, gilt der öffentliche Server.</summary>
    public const string RoutingBRouterAdresse = "Routing.BRouterAdresse";

    /// <summary>Zugangsschlüssel für openrouteservice. Geheim: nie im Klartext anzeigen oder protokollieren.</summary>
    public const string RoutingOpenrouteserviceSchluessel = "Routing.OpenrouteserviceSchluessel";

    /// <summary>Gemeinsame Farbe der Overlays ohne eigene Farbe, z. B. „Violett“.</summary>
    public const string OverlayFarbe = "Overlay.Farbe";

    /// <summary>Größe der Oberfläche in Prozent, z. B. „125“ (Einstellungen, Gruppe „Darstellung“).</summary>
    public const string DarstellungGroesse = "Darstellung.Groesse";

    /// <summary>
    /// Schlüssel einer in den Einstellungen gepflegten Auswahlliste, z. B. „Auswahlliste.Trainingsbereich“;
    /// der Wert enthält die Einträge in der Reihenfolge des Benutzers, einen je Zeile.
    /// </summary>
    /// <param name="liste">Die Liste.</param>
    /// <returns>Der Schlüssel.</returns>
    public static string Auswahlliste(AndyTours.Core.Modell.Auswahlliste liste) => $"Auswahlliste.{liste}";
}

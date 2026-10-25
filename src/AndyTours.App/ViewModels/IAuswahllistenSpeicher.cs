using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Schnittstelle zwischen dem Bereich „Auswahllisten“ der Einstellungen und dem Hauptfenster, das den
/// Datenbestand hält.
/// </summary>
/// <remarks>
/// Das <see cref="AuswahllistenViewModel"/> kennt nur die Namen der Einträge; Ids, Tabellen und die
/// Aktualisierung von Werkzeugleiste, Filter und Tabelle bleiben beim Hauptfenster
/// (<see cref="HauptfensterViewModel"/>). Jede ändernde Methode schreibt zuerst in die Datenbank und
/// ändert erst nach Erfolg den Bestand im Speicher; schlägt das Schreiben fehl, zeigt sie die
/// Fehlermeldung und liefert <c>false</c> – dann bleibt alles beim vorherigen Stand.
/// </remarks>
public interface IAuswahllistenSpeicher
{
    /// <summary>Liefert die Zeilen einer Liste in der Reihenfolge der Anzeige.</summary>
    /// <param name="liste">Die Liste.</param>
    /// <returns>Die Zeilen.</returns>
    IReadOnlyList<Auswahllistenzeile> Lesen(Auswahlliste liste);

    /// <summary>Zählt, wie viele Einheiten und Equipments einen Eintrag verwenden (für Rückfragen).</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="name">Der Eintrag.</param>
    /// <returns>
    /// Anzahl der Einheiten und – nur bei Aktivitäten – der Equipments; bei Equipments nur die Einheiten;
    /// bei Tätigkeiten steht im ersten Wert die Anzahl der Wartungen.
    /// </returns>
    (int Einheiten, int Equipments) ZaehleVerwendung(Auswahlliste liste, string name);

    /// <summary>Liefert den Hinweis, warum ein Eintrag nicht gelöscht werden kann, oder <c>null</c>.</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="name">Der Eintrag.</param>
    /// <returns>Der Hinweis oder <c>null</c>.</returns>
    string? ErmittleLoeschsperre(Auswahlliste liste, string name);

    /// <summary>Fügt einen neuen Eintrag hinzu (bei Aktivitäten ans Ende der Reihenfolge).</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="name">Der neue, bereits geprüfte Name.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool Hinzufuegen(Auswahlliste liste, string name);

    /// <summary>
    /// Benennt einen Eintrag um; bei Trainingsbereichen samt der Einheiten, bei Kategorien samt der
    /// Equipment-Standardkategorie.
    /// </summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="alt">Der bisherige Name.</param>
    /// <param name="neu">Der neue, bereits geprüfte Name.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool Umbenennen(Auswahlliste liste, string alt, string neu);

    /// <summary>Löscht einen Eintrag, für den <see cref="ErmittleLoeschsperre"/> keine Sperre meldet.</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="name">Der Eintrag.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool Loeschen(Auswahlliste liste, string name);

    /// <summary>Speichert eine neue Reihenfolge (Trainingsbereiche und Aktivitäten).</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="reihenfolge">Alle Namen in der neuen Reihenfolge.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool Verschieben(Auswahlliste liste, IReadOnlyList<string> reihenfolge);

    /// <summary>Macht eine Aktivität zur Hauptaktivität.</summary>
    /// <param name="name">Die Aktivität.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool SetzeHauptaktivitaet(string name);

    /// <summary>Schaltet das Standard-Kennzeichen einer Kategorie um.</summary>
    /// <param name="name">Die Kategorie.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool SchalteStandardUm(string name);

    /// <summary>Führt zwei Aktivitäten oder Kategorien zusammen; der Ursprung wird entfernt.</summary>
    /// <param name="liste">Die Liste (Aktivität oder Kategorie).</param>
    /// <param name="ursprung">Der Eintrag, der entfernt wird.</param>
    /// <param name="ziel">Der Eintrag, der bleibt.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    bool Zusammenfuehren(Auswahlliste liste, string ursprung, string ziel);

    /// <summary>
    /// Öffnet das Fenster „Equipment“ für ein neues oder ein vorhandenes Equipment und speichert mit „OK“.
    /// </summary>
    /// <param name="name">Name des vorhandenen Equipments oder <c>null</c> für ein neues.</param>
    /// <returns>Der gespeicherte Name oder <c>null</c>, wenn abgebrochen wurde.</returns>
    string? BearbeiteEquipment(string? name);
}

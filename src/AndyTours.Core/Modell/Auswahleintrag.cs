namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Eintrag einer Auswahlliste mit Nummer und angezeigtem Namen (Aktivität oder Equipment).
/// </summary>
/// <param name="Id">Nummer in der Datenbank; <c>null</c> beim Eintrag „(kein)“.</param>
/// <param name="Name">Angezeigter Name.</param>
public record Auswahleintrag(long? Id, string Name)
{
    /// <summary>Für die Anzeige in Auswahllisten.</summary>
    /// <returns>Der Name.</returns>
    public override string ToString() => Name;
}

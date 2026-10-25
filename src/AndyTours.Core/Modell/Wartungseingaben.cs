namespace AndyTours.Core.Modell;

/// <summary>
/// Die Eingaben im Wartungsfenster, so wie der Benutzer sie gemacht hat.
/// </summary>
/// <remarks>Das Datum ist Text, damit auch ungültige Eingaben geprüft und markiert werden können.</remarks>
public record Wartungseingaben
{
    /// <summary>Datum als Text, z. B. „06.09.2026“.</summary>
    public string Datum { get; init; } = string.Empty;

    /// <summary>Nummer des gewählten Rads; <c>null</c>, solange keines gewählt ist.</summary>
    public long? EquipmentId { get; init; }

    /// <summary>Die gewählte Tätigkeit; leer, solange keine gewählt ist.</summary>
    public string Taetigkeit { get; init; } = string.Empty;

    /// <summary>Freier Kommentar.</summary>
    public string Kommentar { get; init; } = string.Empty;
}

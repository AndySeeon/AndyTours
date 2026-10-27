namespace AndyTours.Core.Modell;

/// <summary>
/// Die Eingaben im Alarmfenster, so wie der Benutzer sie gemacht hat.
/// </summary>
/// <remarks>Das Intervall ist Text, damit auch ungültige Eingaben geprüft und markiert werden können.</remarks>
public record Alarmeingaben
{
    /// <summary>Nummer des gewählten Rads; <c>null</c>, solange keines gewählt ist.</summary>
    public long? EquipmentId { get; init; }

    /// <summary>Die gewählte Tätigkeit; leer, solange keine gewählt ist.</summary>
    public string Taetigkeit { get; init; } = string.Empty;

    /// <summary>Intervall in km als Text, z. B. „3000“.</summary>
    public string Intervall { get; init; } = string.Empty;

    /// <summary><c>true</c>, wenn der Alarm eingeschaltet ist.</summary>
    public bool Eingeschaltet { get; init; } = true;
}

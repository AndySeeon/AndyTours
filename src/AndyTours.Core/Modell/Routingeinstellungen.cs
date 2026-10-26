namespace AndyTours.Core.Modell;

/// <summary>
/// Die Einstellungen „Routing (Tourenplanung)“: welcher Dienst die geplanten Strecken ermittelt und wie er erreicht wird.
/// </summary>
/// <param name="Dienst">Der gewählte Routingdienst.</param>
/// <param name="BRouterAdresse">Adresse des BRouter-Servers, z. B. <see cref="StandardBRouterAdresse"/>.</param>
/// <param name="OpenrouteserviceSchluessel">
/// Zugangsschlüssel für openrouteservice oder <c>null</c>, wenn keiner eingetragen ist. Geheim: nie im Klartext anzeigen
/// oder protokollieren.
/// </param>
public record Routingeinstellungen(Routingdienst Dienst, string BRouterAdresse, string? OpenrouteserviceSchluessel)
{
    /// <summary>Adresse des öffentlichen BRouter-Servers.</summary>
    public const string StandardBRouterAdresse = "https://brouter.de/brouter";

    /// <summary>Die Einstellungen, solange der Benutzer nichts geändert hat: BRouter mit dem öffentlichen Server.</summary>
    public static Routingeinstellungen Standard { get; } = new(Routingdienst.BRouter, StandardBRouterAdresse, null);

    /// <summary>Schützt den Zugangsschlüssel vor der Ausgabe, etwa in Protokollen oder im Debugger.</summary>
    /// <returns>Die Einstellungen mit maskiertem Schlüssel.</returns>
    public override string ToString() =>
        $"{Dienst}, {BRouterAdresse}, Schlüssel {(string.IsNullOrEmpty(OpenrouteserviceSchluessel) ? "keiner" : "••••")}";
}

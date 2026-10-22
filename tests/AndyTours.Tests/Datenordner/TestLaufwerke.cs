namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Hilfsfunktionen rund um Laufwerke für die Datenordner-Tests.
/// </summary>
public static class TestLaufwerke
{
    /// <summary>
    /// Liefert einen Laufwerksbuchstaben, der auf diesem Rechner nicht belegt ist – geeignet,
    /// um einen „nicht erreichbaren“ Ordner zu simulieren.
    /// </summary>
    /// <returns>Ein freier Buchstabe (z. B. „Q“) oder <c>null</c>, wenn alle belegt sind.</returns>
    public static string? FreierLaufwerksbuchstabe()
    {
        var belegt = DriveInfo.GetDrives().Select(l => char.ToUpperInvariant(l.Name[0])).ToHashSet();

        // Von hinten suchen: hohe Buchstaben sind seltener belegt als C, D, E …
        for (char buchstabe = 'Y'; buchstabe >= 'F'; buchstabe--)
        {
            if (!belegt.Contains(buchstabe))
            {
                return buchstabe.ToString();
            }
        }

        return null;
    }
}

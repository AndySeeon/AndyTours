using AndyTours.Core.Modell;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ermittelt den Ordner, in den die Datenbank gesichert wird.
/// </summary>
public static class ErmittleSicherungsordner
{
    /// <summary>
    /// Liefert den vom Benutzer gewählten Sicherungsordner oder, wenn keiner gewählt ist,
    /// den Standardordner <c>Sicherungen</c> im Datenordner.
    /// </summary>
    /// <param name="datenordner">Der aktive Datenordner.</param>
    /// <param name="einstellungen">Die Sicherungseinstellungen dieses Datenordners.</param>
    /// <returns>Vollständiger Pfad des Sicherungsordners (muss noch nicht existieren).</returns>
    public static string Ausfuehren(string datenordner, Sicherungseinstellungen einstellungen)
    {
        return einstellungen.Sicherungsordner
            ?? Path.Combine(datenordner, Sicherungseinstellungen.StandardUnterordner);
    }
}

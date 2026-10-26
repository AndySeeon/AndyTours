namespace AndyTours.Core.Modell;

/// <summary>
/// Die in den Einstellungen wählbaren Routingdienste der Tourenplanung (siehe <c>docs/adr/0004-routingdienst.md</c>).
/// </summary>
public enum Routingdienst
{
    /// <summary>BRouter: ohne Zugangsschlüssel, Serveradresse änderbar; voreingestellt.</summary>
    BRouter,

    /// <summary>openrouteservice: mit Zugangsschlüssel des Benutzers.</summary>
    Openrouteservice,
}

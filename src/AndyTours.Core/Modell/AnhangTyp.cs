namespace AndyTours.Core.Modell;

/// <summary>
/// Art einer an eine Trainingseinheit angehängten Datei.
/// </summary>
/// <remarks>
/// Im Original gibt es drei feste Anhangfelder je Einheit (<c>profile</c>, <c>gpstrack</c>, <c>text</c>).
/// Bei Garmin-Aufzeichnungen verweisen Höhenprofil und GPS-Track meist auf dieselbe FIT-Datei.
/// </remarks>
public enum AnhangTyp
{
    /// <summary>Datei mit dem Höhenprofil (Zeitreihe aus Höhe, Puls, Geschwindigkeit …).</summary>
    Hoehenprofil,

    /// <summary>Datei mit dem GPS-Track (Koordinaten für die Kartenanzeige).</summary>
    GpsTrack,

    /// <summary>Dokumentation zur Tour (HTML, Text oder Bildergalerie).</summary>
    Dokumentation
}

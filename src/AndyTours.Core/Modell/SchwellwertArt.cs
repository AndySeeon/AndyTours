namespace AndyTours.Core.Modell;

/// <summary>
/// Messgröße, für die ein <see cref="Schwellwert"/> gilt.
/// </summary>
/// <remarks>
/// Jede Art entspricht einem INI-Abschnitt des Originals (Kommentar am jeweiligen Wert).
/// </remarks>
public enum SchwellwertArt
{
    /// <summary>Pulsbereiche für die Trainingssteuerung (<c>[LimitsPulseTrain]</c>).</summary>
    PulsTraining,

    /// <summary>Pulsbereiche für die statistische Auswertung (<c>[LimitsPulseStat]</c>).</summary>
    PulsStatistik,

    /// <summary>Geschwindigkeitsbereiche in km/h (<c>[LimitsSpeed]</c>).</summary>
    Geschwindigkeit,

    /// <summary>Steigungsbereiche in Prozent (<c>[LimitsGrad]</c>).</summary>
    Steigung,

    /// <summary>Leistungsbereiche in Watt (<c>[LimitsPower]</c>).</summary>
    Leistung
}

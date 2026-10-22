namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Sportler, dessen Trainingseinheiten verwaltet werden.
/// </summary>
/// <remarks>
/// Entspricht im Original dem INI-Abschnitt <c>[Biker]</c>. Das Original kennt mehrere Benutzer
/// nur über getrennte Datenverzeichnisse; AndyTours legt die Sportler stattdessen in einer Tabelle ab
/// und ordnet jede Trainingseinheit über <see cref="Trainingseinheit.FahrerId"/> einem Sportler zu.
/// </remarks>
public class Fahrer
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Vorname.</summary>
    public string Vorname { get; set; } = string.Empty;

    /// <summary>Nachname.</summary>
    public string Nachname { get; set; } = string.Empty;

    /// <summary>Geburtstag, falls bekannt (wird für altersabhängige Pulsformeln gebraucht).</summary>
    public DateOnly? Geburtstag { get; set; }

    /// <summary>Körpergröße in Zentimetern.</summary>
    public int GroesseCm { get; set; }

    /// <summary>Aktuelles Körpergewicht in Kilogramm (Vorschlag für neue Einheiten).</summary>
    public double GewichtKg { get; set; }

    /// <summary>Ruhepuls in Schlägen pro Minute (Original: <c>pulsemin</c>).</summary>
    public int Ruhepuls { get; set; }

    /// <summary>Maximalpuls in Schlägen pro Minute (Original: <c>pulsemax</c>).</summary>
    public int Maxpuls { get; set; }

    /// <summary>Untere Grenze des persönlichen Trainingspulses (Original: <c>hfMin</c>).</summary>
    public int TrainingspulsVon { get; set; }

    /// <summary>Obere Grenze des persönlichen Trainingspulses (Original: <c>hfMax</c>).</summary>
    public int TrainingspulsBis { get; set; }

    /// <summary>Name des Heimatorts (Startpunkt der Kartenanzeige).</summary>
    public string Heimatort { get; set; } = string.Empty;

    /// <summary>Geografische Breite des Heimatorts in Grad, falls bekannt.</summary>
    public double? HeimatBreitengrad { get; set; }

    /// <summary>Geografische Länge des Heimatorts in Grad, falls bekannt.</summary>
    public double? HeimatLaengengrad { get; set; }
}

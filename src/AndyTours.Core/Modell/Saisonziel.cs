namespace AndyTours.Core.Modell;

/// <summary>
/// Ein eigenes Saisonziel einer Aktivität, gültig ab einem Jahr.
/// </summary>
/// <remarks>
/// Ein Ziel gilt für <see cref="AbJahr"/> und alle späteren Jahre, bis für ein späteres Jahr ein eigenes
/// Ziel eingetragen ist. Für Jahre vor dem ersten eigenen Ziel gilt das Grundziel der Aktivität
/// (<see cref="Aktivitaet.ZielKm"/> usw., aus BikeXperience übernommen). Der Wert 0 bedeutet „kein Ziel“
/// und verdeckt ein früheres Ziel.
/// </remarks>
public class Saisonziel
{
    /// <summary>Nummer der Aktivität.</summary>
    public long AktivitaetId { get; set; }

    /// <summary>Jahr, ab dem das Ziel gilt.</summary>
    public int AbJahr { get; set; }

    /// <summary>Ziel Distanz in Kilometern (0 = kein Ziel).</summary>
    public int ZielKm { get; set; }

    /// <summary>Ziel Höhenmeter (0 = kein Ziel).</summary>
    public int ZielHm { get; set; }

    /// <summary>Ziel Anzahl Trainingseinheiten (0 = kein Ziel).</summary>
    public int ZielAnzahl { get; set; }
}

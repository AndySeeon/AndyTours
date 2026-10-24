namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Von-bis-Bereich der Filterleiste (Distanz oder Höhenmeter), gelesen aus den beiden Eingabefeldern.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.LeseFilterbereich"/>. Eine leere Grenze ist <c>null</c> und schränkt
/// nicht ein. Ist ein Feld fehlerhaft (keine Zahl, oder „von“ größer als „bis“), wird der ganze Bereich nicht
/// angewendet; die Oberfläche markiert das betroffene Feld rot.
/// </remarks>
/// <param name="Von">Untere Grenze (eingeschlossen) oder <c>null</c>.</param>
/// <param name="Bis">Obere Grenze (eingeschlossen) oder <c>null</c>.</param>
/// <param name="VonFehlerhaft"><c>true</c>, wenn das Feld „von“ rot markiert werden muss.</param>
/// <param name="BisFehlerhaft"><c>true</c>, wenn das Feld „bis“ rot markiert werden muss.</param>
public record Filterbereich(double? Von, double? Bis, bool VonFehlerhaft = false, bool BisFehlerhaft = false)
{
    /// <summary>Bereich ohne Grenzen (kein Filter).</summary>
    public static Filterbereich Leer { get; } = new(null, null);

    /// <summary><c>true</c>, wenn keines der beiden Felder fehlerhaft ist.</summary>
    public bool IstGueltig => !VonFehlerhaft && !BisFehlerhaft;

    /// <summary><c>true</c>, wenn der Bereich gültig ist und mindestens eine Grenze hat, also tatsächlich filtert.</summary>
    public bool IstAktiv => IstGueltig && (Von is not null || Bis is not null);

    /// <summary>
    /// Prüft, ob ein Wert im Bereich liegt (Grenzen eingeschlossen).
    /// </summary>
    /// <param name="wert">Der zu prüfende Wert.</param>
    /// <returns><c>true</c>, wenn der Wert weder unter „von“ noch über „bis“ liegt.</returns>
    public bool Enthaelt(double wert) => (Von is null || wert >= Von) && (Bis is null || wert <= Bis);
}

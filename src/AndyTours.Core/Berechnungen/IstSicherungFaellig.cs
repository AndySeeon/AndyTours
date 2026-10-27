namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Entscheidet beim Programmstart, ob die Datenbank gesichert werden soll.
/// </summary>
public static class IstSicherungFaellig
{
    /// <summary>
    /// Liefert <c>true</c>, wenn seit der letzten Sicherung die eingestellte Zahl von
    /// Programmstarts erreicht ist und die Datenbank Trainingseinheiten enthält.
    /// </summary>
    /// <remarks>
    /// Eine Datenbank ohne Touren wird nicht gesichert – sie enthält nichts, was verloren gehen könnte.
    /// Der Startzähler ist größer als das Intervall, wenn das Intervall nachträglich verkleinert
    /// wurde oder eine Sicherung fehlgeschlagen ist; auch dann ist die Sicherung fällig.
    /// </remarks>
    /// <param name="startsSeitSicherung">Programmstarts seit der letzten Sicherung, einschließlich des aktuellen Starts.</param>
    /// <param name="intervall">Eingestelltes Intervall (Anzahl der Programmstarts).</param>
    /// <param name="anzahlEinheiten">Anzahl der Trainingseinheiten in der Datenbank.</param>
    /// <returns><c>true</c>, wenn jetzt gesichert werden soll.</returns>
    public static bool Ausfuehren(int startsSeitSicherung, int intervall, int anzahlEinheiten)
    {
        return anzahlEinheiten > 0 && startsSeitSicherung >= intervall;
    }
}

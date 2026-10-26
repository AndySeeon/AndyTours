using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft den Namen eines Overlay-Ordners beim Anlegen und Umbenennen.
/// </summary>
public static class PruefeOrdnername
{
    /// <summary>
    /// Liefert den Hinweis, warum der Name nicht übernommen werden kann, oder <c>null</c>, wenn er passt.
    /// </summary>
    /// <remarks>
    /// Ein Name ist ungültig, wenn er leer ist (auch nur aus Leerzeichen) oder im selben übergeordneten Ordner schon
    /// vorkommt. Groß-/Kleinschreibung und Leerzeichen am Rand zählen dabei nicht. In verschiedenen Ordnern darf derselbe
    /// Name vorkommen. Beim Umbenennen ist der Ordner selbst ausgenommen, sodass z. B. „alpen“ in „Alpen“ geändert
    /// werden kann.
    /// </remarks>
    /// <param name="name">Der eingegebene Name.</param>
    /// <param name="elternId">Der übergeordnete Ordner; <c>null</c> = oberste Ebene.</param>
    /// <param name="ordner">Alle vorhandenen Ordner.</param>
    /// <param name="eigeneId">Beim Umbenennen die Nummer des Ordners selbst, beim Anlegen <c>null</c>.</param>
    /// <returns>Der Hinweistext oder <c>null</c>.</returns>
    public static string? Ausfuehren(string name, long? elternId, IReadOnlyList<Overlayordner> ordner, long? eigeneId = null)
    {
        string bereinigt = name.Trim();
        if (bereinigt.Length == 0)
        {
            return "Bitte einen Namen für den Ordner eingeben.";
        }

        bool vorhanden = ordner.Any(o => o.ElternId == elternId && o.Id != eigeneId
            && string.Equals(o.Name.Trim(), bereinigt, StringComparison.OrdinalIgnoreCase));
        return vorhanden ? $"Einen Ordner „{bereinigt}“ gibt es hier schon." : null;
    }
}

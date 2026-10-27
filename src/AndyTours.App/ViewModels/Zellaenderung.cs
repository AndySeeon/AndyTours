namespace AndyTours.App.ViewModels;

/// <summary>
/// Ergebnis einer Änderung in einer Zelle der Tabelle „Trainingseinheiten“.
/// </summary>
/// <remarks>Ergebnis von <see cref="HauptfensterViewModel.AendereZelle"/>.</remarks>
public enum Zellaenderung
{
    /// <summary>Der Wert ist ungültig; die Zelle bleibt in Bearbeitung und wird rot markiert.</summary>
    Ungueltig,

    /// <summary>Der Wert wurde gespeichert; die Tabelle wird nach dem Ende der Bearbeitung neu aufgebaut.</summary>
    Gespeichert,

    /// <summary>Nichts gespeichert (Wert unverändert oder Fehler beim Schreiben); die Zelle zeigt den alten Wert.</summary>
    NichtGespeichert,
}

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Zustand des Zielordners vor der Umwandlung (siehe <see cref="PruefeZielordner"/>).
/// </summary>
public enum ZielordnerZustand
{
    /// <summary>Der Ordner existiert nicht oder ist leer – er kann ohne Rückfrage befüllt werden.</summary>
    FehltOderLeer,

    /// <summary>Der Ordner enthält bereits AndyTours-Daten – Ersetzen nur nach ausdrücklicher Bestätigung.</summary>
    MitDaten,

    /// <summary>Der Ordner enthält andere Dateien, aber keine AndyTours-Datenbank – er wird nicht angetastet.</summary>
    FremderInhalt,
}

namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Zwischenstand beim Kopieren eines Datenbestands (siehe <see cref="KopiereDatenbestand"/>).
/// </summary>
/// <remarks>
/// Wird nach jeder kopierten Datei gemeldet. Die Oberfläche berechnet daraus den Anteil für
/// den Fortschrittsbalken und die Texte „Dateien x von y“ und „MB x von y“.
/// </remarks>
/// <param name="KopierteDateien">Anzahl der bereits kopierten Dateien.</param>
/// <param name="GesamtDateien">Anzahl aller zu kopierenden Dateien.</param>
/// <param name="KopierteBytes">Datenmenge der bereits kopierten Dateien in Byte.</param>
/// <param name="GesamtBytes">Datenmenge aller zu kopierenden Dateien in Byte.</param>
public record KopierFortschritt(int KopierteDateien, int GesamtDateien, long KopierteBytes, long GesamtBytes);

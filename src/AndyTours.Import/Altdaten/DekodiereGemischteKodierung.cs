using System.Text;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt den Byte-Inhalt einer Altdatei in Text um, auch wenn darin UTF-8 und Latin-1 gemischt vorkommen.
/// </summary>
/// <remarks>
/// Die BikeXperience-Dateien sind grundsätzlich UTF-8-kodiert. Einige neuere Einträge (2025/2026)
/// enthalten jedoch Umlaute als einzelne Latin-1-Bytes (z. B. 0xFC für „ü"). Ein normaler
/// UTF-8-Leser macht daraus das Ersatzzeichen „�" – obwohl der Umlaut eindeutig rekonstruierbar ist.
/// <para>
/// Diese Funktion prüft deshalb jedes Nicht-ASCII-Byte einzeln: Bildet es zusammen mit den
/// folgenden Bytes eine gültige UTF-8-Folge, wird diese als UTF-8 gelesen, andernfalls wird das
/// einzelne Byte als Latin-1-Zeichen übernommen. Bereits in der Datei gespeicherte Ersatzzeichen
/// (UTF-8-Folge EF BF BD) bleiben als „�" erhalten; sie werden später von
/// <see cref="RepariereUmlaute"/> behandelt.
/// </para>
/// </remarks>
public static class DekodiereGemischteKodierung
{
    /// <summary>
    /// UTF-8-Dekodierer, der bei ungültigen Bytefolgen eine Ausnahme auslöst, statt still „�"
    /// einzusetzen. Nur so lässt sich erkennen, ob eine Folge wirklich gültiges UTF-8 ist.
    /// </summary>
    private static readonly UTF8Encoding StrengesUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Dekodiert die Bytes zu Text.
    /// </summary>
    /// <param name="bytes">Dateiinhalt.</param>
    /// <param name="anzahlLatin1Zeichen">Anzahl der Bytes, die als Latin-1 gedeutet wurden (für den Migrationsbericht).</param>
    /// <returns>Der dekodierte Text ohne Byte-Order-Mark.</returns>
    public static string Ausfuehren(byte[] bytes, out int anzahlLatin1Zeichen)
    {
        var text = new StringBuilder(bytes.Length);
        anzahlLatin1Zeichen = 0;

        // Eine Byte-Order-Mark (EF BB BF) am Dateianfang gehört nicht zum Inhalt und wird übersprungen.
        int position = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;

        while (position < bytes.Length)
        {
            byte aktuell = bytes[position];

            // ASCII-Zeichen (0x00–0x7F) sind in UTF-8 und Latin-1 identisch.
            if (aktuell < 0x80)
            {
                text.Append((char)aktuell);
                position++;
                continue;
            }

            // Aus dem ersten Byte ergibt sich, wie lang eine UTF-8-Folge sein müsste:
            // 110xxxxx = 2 Bytes, 1110xxxx = 3 Bytes, 11110xxx = 4 Bytes.
            int folgenlaenge = aktuell switch
            {
                >= 0xC2 and <= 0xDF => 2,
                >= 0xE0 and <= 0xEF => 3,
                >= 0xF0 and <= 0xF4 => 4,
                _ => 0 // kein gültiges UTF-8-Startbyte
            };

            if (folgenlaenge > 0 && position + folgenlaenge <= bytes.Length)
            {
                try
                {
                    // Gelingt das strenge Dekodieren, war es echtes UTF-8 (z. B. „ä" = C3 A4).
                    text.Append(StrengesUtf8.GetString(bytes, position, folgenlaenge));
                    position += folgenlaenge;
                    continue;
                }
                catch (DecoderFallbackException)
                {
                    // Keine gültige Folge – unten als Latin-1 behandeln.
                }
            }

            // Einzelnes Byte als Latin-1 deuten: Dort entspricht der Bytewert direkt dem
            // Unicode-Zeichen (0xFC → U+00FC „ü", 0xF6 → „ö", 0xDF → „ß").
            text.Append((char)aktuell);
            anzahlLatin1Zeichen++;
            position++;
        }

        return text.ToString();
    }
}

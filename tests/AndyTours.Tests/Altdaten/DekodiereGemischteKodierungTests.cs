using System.Text;
using AndyTours.Import.Altdaten;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Tests für <see cref="DekodiereGemischteKodierung"/>.
/// </summary>
public class DekodiereGemischteKodierungTests
{
    [Fact]
    public void Utf8Umlaute_werden_normal_gelesen()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Dämpfer für Höhe");

        string text = DekodiereGemischteKodierung.Ausfuehren(bytes, out int latin1);

        Assert.Equal("Dämpfer für Höhe", text);
        Assert.Equal(0, latin1);
    }

    [Fact]
    public void Latin1Bytes_werden_als_Umlaute_wiederhergestellt()
    {
        // „Br" + 0xFC (ü in Latin-1) + „ndling" – so steht es in workouts.csv ab 2025
        byte[] bytes = { (byte)'B', (byte)'r', 0xFC, (byte)'n', (byte)'d', (byte)'l', (byte)'i', (byte)'n', (byte)'g' };

        string text = DekodiereGemischteKodierung.Ausfuehren(bytes, out int latin1);

        Assert.Equal("Bründling", text);
        Assert.Equal(1, latin1);
    }

    [Fact]
    public void Gespeichertes_Ersatzzeichen_bleibt_erhalten()
    {
        // EF BF BD ist das in UTF-8 gespeicherte „�" – der Umlaut ist hier wirklich verloren
        byte[] bytes = { (byte)'D', 0xEF, 0xBF, 0xBD, (byte)'m' };

        string text = DekodiereGemischteKodierung.Ausfuehren(bytes, out int latin1);

        Assert.Equal("D�m", text);
        Assert.Equal(0, latin1);
    }

    [Fact]
    public void Gemischte_Zeile_wird_korrekt_gelesen()
    {
        // UTF-8-„ä" (C3 A4) und Latin-1-„ö" (F6) in derselben Zeile
        byte[] bytes = { 0xC3, 0xA4, (byte)' ', 0xF6 };

        Assert.Equal("ä ö", DekodiereGemischteKodierung.Ausfuehren(bytes, out _));
    }

    [Fact]
    public void ByteOrderMark_wird_entfernt()
    {
        byte[] bytes = { 0xEF, 0xBB, 0xBF, (byte)'A' };

        Assert.Equal("A", DekodiereGemischteKodierung.Ausfuehren(bytes, out _));
    }
}

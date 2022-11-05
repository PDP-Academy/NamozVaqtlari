using System.Text.Json.Serialization;

namespace NamozVaqtlari.Models;

public class PrayerTime
{
    [JsonPropertyName("times")]
    public Time Time { get; set; }
}

public class Time
{
    [JsonPropertyName("tong_saharlik")]
    public string TongSaharlik { get; set; }

    [JsonPropertyName("quyosh")]
    public string Quyosh { get; set; }

    [JsonPropertyName("peshin")]
    public string Peshin { get; set; }

    [JsonPropertyName("asr")]
    public string Asr { get; set; }

    [JsonPropertyName("shom_iftor")]
    public string Shom { get; set; }

    [JsonPropertyName("hufton")]
    public string Hufton { get; set; }

    public override string ToString()
    {
        return $"Saharlik: {TongSaharlik}\n" +
            $"Quyosh: {Quyosh}\n" +
            $"Peshin: {Peshin}\n" +
            $"Asr: {Asr}\n" +
            $"Shom: {Shom}\n" +
            $"Hufton: {Hufton}\n";
    }
}
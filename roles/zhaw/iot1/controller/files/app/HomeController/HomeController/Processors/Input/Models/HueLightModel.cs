namespace HomeController.Processors.Input.Models
{
    public record Brightness(byte Value, int MaxLumens);

    public record ReciprocalMegakelvin(int Value, int Min, int Max);

    public record HueLightModel(Brightness Brightness, ReciprocalMegakelvin MK1, int Hue, int Saturation, bool On);
}

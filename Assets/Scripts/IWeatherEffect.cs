/// <summary>
/// A switchable visual effect (rain, snow, clouds). The environment controller turns every
/// effect off before turning on the ones a new weather state needs, so nothing from the
/// previous state can linger.
/// </summary>
public interface IWeatherEffect
{
    void Activate();
    void Deactivate();
}

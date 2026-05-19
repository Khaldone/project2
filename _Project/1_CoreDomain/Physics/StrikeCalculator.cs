// 1. The Interface (Abstraction)
// This defines what the calculator needs, without caring how Unity handles it.
public interface ITableProperties
{
    float GetFrictionCoefficient();
}


// 2. The Pure C# Logic (Domain)
// No MonoBehaviour here. This is the protected core of the game.
public class StrikeCalculator
{
    private readonly ITableProperties _tableProperties;


    // Dependency Injection: We pass the table properties in via the constructor.
    public StrikeCalculator(ITableProperties tableProperties)
    {
        _tableProperties = tableProperties;
    }


    public float CalculateFinalVelocity(float initialForce, float mass)
    {
        if (mass <= 0) return 0f;


        // We ask the interface for the friction, completely unaware of Unity GameObjects
        float friction = _tableProperties.GetFrictionCoefficient();
        float velocity = (initialForce / mass) - friction;


        return velocity > 0 ? velocity : 0f;
    }
}

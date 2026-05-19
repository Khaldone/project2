/// <summary>
/// Factory for creating clip selector instances
/// </summary>
public static class ClipSelectorFactory
{
    public static IClipSelector Create(ClipSelectorType type)
    {
        switch (type)
        {
            case ClipSelectorType.FirstMatch:
                return new FirstMatchSelector();
                
            case ClipSelectorType.Random:
                return new RandomMatchSelector();
                
            case ClipSelectorType.WeightedRandom:
                return new WeightedRandomSelector();
                
            case ClipSelectorType.NoRepeatRandom:
                return new NoRepeatRandomSelector();
                
            case ClipSelectorType.RoundRobin:
                return new RoundRobinSelector();
                
            case ClipSelectorType.Shuffle:
                return new ShuffleSelector();
                
            default:
                return new RandomMatchSelector();
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents;

internal class GameCallbackModelEqualityComparer : IEqualityComparer<GameCallbackModel>
{
    public bool Equals(GameCallbackModel x, GameCallbackModel y)
    {
        if (ReferenceEquals(x, y))
            return true;
        
        if (ReferenceEquals(x, null))
            return false;
        
        if (ReferenceEquals(y, null))
            return false;

        return x.Name == y.Name && x.Type == y.Type && x.CallbackMethodName == y.CallbackMethodName &&
               x.ArgumentTypes.SequenceEqual(y.ArgumentTypes);
    }

    public int GetHashCode(GameCallbackModel obj)
    {
        unchecked
        {
            var hashCode = obj.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.Type.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.CallbackMethodName.GetHashCode();
            return hashCode;
        }
    }
}
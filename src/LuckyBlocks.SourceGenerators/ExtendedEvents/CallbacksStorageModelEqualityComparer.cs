using System.Collections.Generic;
using System.Linq;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents;

internal class CallbacksStorageModelEqualityComparer : IEqualityComparer<CallbacksStorageModel>
{
    public bool Equals(CallbacksStorageModel x, CallbacksStorageModel y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null))
            return false;

        if (ReferenceEquals(y, null))
            return false;

        return x.Name == y.Name && x.Type == y.Type && x.FilterObjectExists == y.FilterObjectExists &&
               x.CallbackType == y.CallbackType && x.ArgumentTypes.SequenceEqual(y.ArgumentTypes);
    }

    public int GetHashCode(CallbacksStorageModel obj)
    {
        unchecked
        {
            var hashCode = obj.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.Type.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.FilterObjectExists.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.CallbackType.GetHashCode();
            return hashCode;
        }
    }
}
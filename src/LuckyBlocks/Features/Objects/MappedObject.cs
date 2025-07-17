using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Objects;

[Inject]
internal class MappedObject
{
    public int UniqueId => GetActualObject().UniqueId;

    [InjectMappedObjectsService]
    private static IMappedObjectsService MappedObjectsService { get; set; }

    private readonly int _initialObjectId;

    private IObject _actualObject;

    public MappedObject(IObject @object)
    {
        _actualObject = @object;
        _initialObjectId = @object.UniqueId;
    }

    public IObject AsIObject() => GetActualObject();

    public static implicit operator IObject(MappedObject mappedObject) =>
        GetActualObject(ref mappedObject._actualObject);

    public override int GetHashCode()
    {
        return _initialObjectId;
    }

    private IObject GetActualObject()
    {
        return GetActualObject(ref _actualObject);
    }

    private static IObject GetActualObject(ref IObject @object)
    {
        if (!@object.IsValid())
        {
            @object = MappedObjectsService.GetActualObject(@object.UniqueId);
        }

        return @object;
    }
}
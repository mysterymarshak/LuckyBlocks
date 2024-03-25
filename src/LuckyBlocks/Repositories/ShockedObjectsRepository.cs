using System.Collections.Generic;
using LuckyBlocks.Entities;
using SFDGameScriptInterface;

namespace LuckyBlocks.Repositories;

internal interface IShockedObjectsRepository
{
    void AddShockedObject(ShockedObject shockedObject);
    bool IsShockedObject(IObject @object);
    void RemoveShockedObject(int id);
} 

internal class ShockedObjectsRepository : IShockedObjectsRepository
{
    private readonly Dictionary<int, ShockedObject> _objects = new();
    
    public void AddShockedObject(ShockedObject shockedObject)
    {
        var objectId = shockedObject.ObjectId;
        _objects.Add(objectId, shockedObject);
    }

    public bool IsShockedObject(IObject @object)
    {
        return _objects.ContainsKey(@object.UniqueId);
    }

    public void RemoveShockedObject(int id)
    {
        _objects.Remove(id);
    }
}
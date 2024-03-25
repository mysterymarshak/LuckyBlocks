using System.Linq;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal interface IEntity
{
    string Id { get; }
    Vector2 Position { get; }
    bool IsValid();
}

internal class ObjectEntity : IEntity
{
    public IObject Object { get; }
    public string Id => $"obj_{Object.UniqueId}";
    public Vector2 Position => Object.GetWorldPosition();
    public bool IsValid() => Object.IsValid();
    
    public ObjectEntity(IObject @object)
        => (Object) = (@object);
}

internal class ProjectileEntity : IEntity
{
    public IProjectile Projectile { get; }
    public string Id => $"proj_{Projectile.InstanceID}";
    public Vector2 Position => Projectile.Position;
    public bool IsValid() => !Projectile.IsRemoved;
    
    public ProjectileEntity(IProjectile projectile)
        => (Projectile) = (projectile);
}

internal class FireNodeEntity : IEntity
{
    public FireNode FireNode { get; }
    public string Id => $"firenode_{FireNode.InstanceID}";
    public Vector2 Position => FireNode.Position;

    private readonly IGame _game;
    
    public FireNodeEntity(FireNode fireNode, IGame game)
        => (FireNode, _game) = (fireNode, game);
    
    public bool IsValid()
    {
        return _game.GetFireNodes().Any(x => x.InstanceID == FireNode.InstanceID);
    }
}
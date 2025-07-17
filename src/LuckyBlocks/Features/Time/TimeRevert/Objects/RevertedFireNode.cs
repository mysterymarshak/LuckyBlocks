using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedFireNode : IRevertedEntity
{
    public int InstanceId => _fireNode.InstanceID;

    private readonly FireNode _fireNode;

    public RevertedFireNode(FireNode fireNode)
    {
        _fireNode = fireNode;
    }

    public void Restore(IGame game)
    {
        game.EndFireNode(_fireNode.InstanceID);
        game.SpawnFireNode(_fireNode.Position, _fireNode.Velocity, FireNodeType.Flamethrower);
    }
}
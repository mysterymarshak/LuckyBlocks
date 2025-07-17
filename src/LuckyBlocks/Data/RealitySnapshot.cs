using System.Collections.Generic;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Time.TimeRevert.Objects;

namespace LuckyBlocks.Data;

internal record RealitySnapshot(
    int Id,
    List<IRevertedObject> StaticObjects,
    List<IRevertedObject> DynamicObjects,
    List<IRevertedObject> AdditionalObjects,
    List<RevertedFireNode> FireNodes,
    List<RevertedProjectile> Projectiles,
    float ElapsedGameTime,
    MagicServiceState MagicServiceState,
    EntitiesServiceState EntitiesServiceState,
    int SpawnChanceId);
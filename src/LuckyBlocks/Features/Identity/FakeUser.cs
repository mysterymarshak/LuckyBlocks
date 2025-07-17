using System;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal class FakeUser : IUser
{
    private IPlayer _player;

    public FakeUser(IPlayer player)
    {
        _player = player;
    }

    public override string Name => _player.Name;
    public override long UserID => _player.UniqueId;
    public override long UserId => _player.UniqueId;
    public override int UserIdentifier => _player.UniqueId;
    public override int GameSlotIndex => 0;
    public override bool IsHost => false;
    public override bool IsModerator => false;
    public override bool IsSpectator => false;
    public override bool Spectating => false;
    public override bool JoinedAsSpectator => false;
    public override int Ping => 0;
    public override string ConnectionIP => string.Empty;
    public override string AccountID => _player.Name;
    public override string AccountName => _player.Name;
    public override int TotalGames => 0;
    public override int TotalWins => 0;
    public override int TotalLosses => 0;
    public override bool IsBot => true;
    public override PredefinedAIType BotPredefinedAIType => PredefinedAIType.BotA;
    public override bool IsUser => false;
    public override Gender Gender => Gender.Male;
    public override bool IsRemoved => !_player.IsValid();

    public override void IncreaseScore() => throw new NotImplementedException();
    public override PlayerTeam GetTeam() => _player.GetTeam();
    public override IPlayer GetPlayer() => _player;
    public override void SetPlayer(IPlayer player, bool flash = true) => _player = player;
    public override IProfile GetProfile() => _player.GetProfile();
}
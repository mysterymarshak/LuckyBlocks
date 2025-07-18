using System;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal class FakeUser : IUser
{
    private readonly Player? _sourcePlayer;

    private IPlayer _playerInstance;

    public FakeUser(IPlayer playerInstance, Player? sourcePlayer = null)
    {
        _playerInstance = playerInstance;
        _sourcePlayer = sourcePlayer;
    }

    public override string Name => _sourcePlayer?.Name ?? _playerInstance.Name;
    public override long UserID => _playerInstance.UniqueId;
    public override long UserId => _playerInstance.UniqueId;
    public override int UserIdentifier => _playerInstance.UniqueId;
    public override int GameSlotIndex => 0;
    public override bool IsHost => false;
    public override bool IsModerator => false;
    public override bool IsSpectator => false;
    public override bool Spectating => false;
    public override bool JoinedAsSpectator => false;
    public override int Ping => 0;
    public override string ConnectionIP => string.Empty;
    public override string AccountID => _playerInstance.Name;
    public override string AccountName => _playerInstance.Name;
    public override int TotalGames => 0;
    public override int TotalWins => 0;
    public override int TotalLosses => 0;
    public override bool IsBot => true;
    public override PredefinedAIType BotPredefinedAIType => PredefinedAIType.BotA;
    public override bool IsUser => false;
    public override Gender Gender => Gender.Male;
    public override bool IsRemoved => !_playerInstance.IsValid();

    public override void IncreaseScore() => throw new NotImplementedException();
    public override PlayerTeam GetTeam() => _playerInstance.GetTeam();
    public override IPlayer GetPlayer() => _playerInstance;
    public override void SetPlayer(IPlayer player, bool flash = true) => _playerInstance = player;
    public override IProfile GetProfile() => _sourcePlayer?.Profile ?? _playerInstance.GetProfile();
}
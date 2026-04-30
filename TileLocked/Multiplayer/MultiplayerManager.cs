using StardewModdingAPI;
using StardewModdingAPI.Events;
using TileLocked.Config;

namespace TileLocked.Multiplayer
{
  internal sealed class MultiplayerManager
  {
    private readonly IModHelper helper;
    private readonly IManifest modManifest;
    private readonly TileManager tileManager;

    public MultiplayerManager(IModHelper helper, IManifest modManifest, TileManager tileManager)
    {
      this.helper = helper;
      this.modManifest = modManifest;
      this.tileManager = tileManager;
    }

    public void OnPeerContextReceived(object? sender, PeerContextReceivedEventArgs e)
    {
      if (Context.IsMainPlayer)
      {
        helper.Multiplayer.SendMessage(
          tileManager.GetPeerConnectionMessage(),
          PeerConnectionMessage.TYPE,
          new[] { modManifest.UniqueID },
          new[] { e.Peer.PlayerID }
        );
      }
    }

    public void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
      if (e.FromModID != modManifest.UniqueID)
        return;

      switch (e.Type)
      {
        case PeerConnectionMessage.TYPE:
          tileManager.LoadFromPeerConnectionMessage(e.ReadAs<PeerConnectionMessage>());
          break;
        case TileUnlockedMessage.TYPE:
          tileManager.TileUnlocked(e.ReadAs<TileUnlockedMessage>());
          break;
        case BankedTilesAddedMessage.TYPE:
          tileManager.BankedTilesAdded(e.ReadAs<BankedTilesAddedMessage>().quantity);
          break;
        case BankedTileUsedMessage.TYPE:
          tileManager.BankedTileUsed();
          break;
        case PerSaveConfigUpdateMessage.TYPE:
          var msg = e.ReadAs<PerSaveConfigUpdateMessage>();
          PerSaveConfig.Set(msg.key, msg.value);
          break;
      }
    }
  }
}
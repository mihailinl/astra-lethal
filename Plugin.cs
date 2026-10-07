using Astra.Sdk;
using BepInEx;

namespace AstraLethal
{
    /// <summary>
    /// Astra for Lethal Company: she comes along on the job.
    /// <list type="bullet">
    /// <item>she accompanies YOUR employee and is composited into the camera you look through (the
    /// game's active camera: yours, or the spectator's), pixelated with the rest of the game;</item>
    /// <item>she is sized to your employee's standing height, walks the game's own walls and floors
    /// (its colliders-and-rooms mask), and follows you in and out of the facility;</item>
    /// <item>raw facts for her animation set every frame — how frightened you are, whether you are
    /// inside the facility or in the ship, crouching, on a ladder, under water. What they look like is
    /// the set's business (a pack can make her scared, quiet, or curious).</item>
    /// </list>
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "Lethal Company");
            astra.Defaults.MatchPlayerHeight = 0.95f;
            astra.Defaults.TeleportDistance = 25f;
            astra.UseCamera(() => StartOfRound.Instance != null ? StartOfRound.Instance.activeCamera : null);
            astra.UsePlayer(_ => LethalPlayer.Locate());
            astra.OnFrame(f =>
            {
                var round = StartOfRound.Instance;
                if (round != null)
                {
                    // The game's own walls-and-floors mask, once the round exists.
                    astra.Defaults.GroundMask = round.collidersAndRoomMaskAndDefault;
                    f.Params.Set("fear", round.fearLevel);
                }
                var p = LethalPlayer.Local;
                if (p == null) return;
                f.Params.Set("inside", p.isInsideFactory)
                    .Set("in_ship", p.isInHangarShipRoom)
                    .Set("crouching", p.isCrouching)
                    .Set("ladder", p.isClimbingLadder)
                    .Set("underwater", p.isUnderwater);
            });
            Logger.LogInfo("Astra is coming along on the job");
        }
    }
}

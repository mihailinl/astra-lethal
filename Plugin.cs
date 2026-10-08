using Astra.Sdk;
using BepInEx;
using UnityEngine;

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
    /// Every game type (<c>StartOfRound</c>, <c>GameNetworkManager</c>, <c>PlayerControllerB</c>) is
    /// reached by NAME through <see cref="Astra.Sdk.GameType"/>: this plugin compiles against the
    /// Astra SDK, BepInEx and Unity only — never the game's own assemblies.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        static readonly GameType StartOfRoundType = GameType.Find("StartOfRound");
        static readonly Member<Camera> ActiveCamera = StartOfRoundType.Member<Camera>("activeCamera");
        static readonly Member<int> CollidersAndRoomMaskAndDefault =
            StartOfRoundType.Member<int>("collidersAndRoomMaskAndDefault");
        static readonly Member<float> FearLevel = StartOfRoundType.Member<float>("fearLevel");

        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "Lethal Company");
            astra.Defaults.MatchPlayerHeight = 0.95f;
            astra.Defaults.TeleportDistance = 25f;
            astra.UseCamera(() => ActiveCamera.Get(StartOfRoundType.Static<object>("Instance")));
            astra.UsePlayer(_ => LethalPlayer.Locate());
            // Its dark is darker than most games': at the engine's default floor she read as a lit
            // figure in a pitch-black corridor (the user's live check, 2026-10-07). Half of it.
            astra.UseLook(floor: 0.10f);
            astra.OnFrame(f =>
            {
                var round = StartOfRoundType.Static<object>("Instance");
                if (round != null)
                {
                    // The game's own walls-and-floors mask, once the round exists.
                    astra.Defaults.GroundMask = CollidersAndRoomMaskAndDefault.Get(round);
                    f.Params.Set("fear", FearLevel.Get(round));
                }
                var p = LethalPlayer.Local;
                if (p == null) return;
                f.Params.Set("inside", LethalPlayer.IsInsideFactory.Get(p))
                    .Set("in_ship", LethalPlayer.IsInHangarShipRoom.Get(p))
                    .Set("crouching", LethalPlayer.IsCrouching.Get(p))
                    .Set("ladder", LethalPlayer.IsClimbingLadder.Get(p))
                    .Set("underwater", LethalPlayer.IsUnderwater.Get(p));
            });
            Logger.LogInfo("Astra is coming along on the job");
        }
    }
}

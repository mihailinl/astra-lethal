using Astra.Sdk;
using UnityEngine;

namespace AstraLethal
{
    /// <summary>
    /// Lethal Company's local employee, as the Astra SDK sees a player. Reached by NAME
    /// (<see cref="Astra.Sdk.GameType"/>): this plugin never compiles against the game's own
    /// assemblies (verified against the installed game with <c>tools/check-game-members</c> in
    /// astra-bepinex).
    /// </summary>
    static class LethalPlayer
    {
        /// <summary>A standing employee's controller is 2.5 units tall; a crouch lerps it to 1.5.</summary>
        const float StandingHeight = 2.5f;

        static readonly GameType GameNetworkManagerType = GameType.Find("GameNetworkManager");
        static readonly GameType PlayerControllerBType = GameType.Find("PlayerControllerB");

        static readonly Member<object> LocalPlayerController =
            GameNetworkManagerType.Member<object>("localPlayerController");
        static readonly Member<bool> IsPlayerDead = PlayerControllerBType.Member<bool>("isPlayerDead");
        static readonly Member<bool> IsPlayerControlled = PlayerControllerBType.Member<bool>("isPlayerControlled");
        static readonly Member<CharacterController> ThisController =
            PlayerControllerBType.Member<CharacterController>("thisController");

        /// <summary>Raw facts <see cref="AstraLethal.Plugin"/> reads every frame.</summary>
        public static readonly Member<bool> IsInsideFactory = PlayerControllerBType.Member<bool>("isInsideFactory");
        public static readonly Member<bool> IsInHangarShipRoom = PlayerControllerBType.Member<bool>("isInHangarShipRoom");
        public static readonly Member<bool> IsCrouching = PlayerControllerBType.Member<bool>("isCrouching");
        public static readonly Member<bool> IsClimbingLadder = PlayerControllerBType.Member<bool>("isClimbingLadder");
        public static readonly Member<bool> IsUnderwater = PlayerControllerBType.Member<bool>("isUnderwater");

        /// <summary>YOUR employee (never another player's), or null in the menu, before spawning and
        /// while you are dead.</summary>
        public static object Local
        {
            get
            {
                var mgr = GameNetworkManagerType.Static<object>("Instance");
                return mgr != null ? LocalPlayerController.Get(mgr) : null;
            }
        }

        public static PlayerInfo? Locate()
        {
            var p = Local;
            var root = p as Component;
            if (root == null || IsPlayerDead.Get(p) || !IsPlayerControlled.Get(p)) return null;
            var cc = ThisController.Get(p);
            if (cc == null)
                return new PlayerInfo { Feet = root.transform.position, Forward = root.transform.forward, Root = root.gameObject, Grounded = true };
            var b = cc.bounds;
            return new PlayerInfo
            {
                Feet = new Vector3(b.center.x, b.min.y, b.center.z),
                Forward = root.transform.forward,
                Root = root.gameObject,
                Grounded = cc.isGrounded && !IsClimbingLadder.Get(p),
                // Her size follows the employee's STANDING height only: a crouch never shrinks her.
                Height = !IsCrouching.Get(p) && cc.height > StandingHeight - 0.1f ? b.size.y : 0,
            };
        }
    }
}

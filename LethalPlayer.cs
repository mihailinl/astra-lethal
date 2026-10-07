using Astra.Sdk;
using GameNetcodeStuff;
using UnityEngine;

namespace AstraLethal
{
    /// <summary>Lethal Company's local employee, as the Astra SDK sees a player.</summary>
    static class LethalPlayer
    {
        /// <summary>A standing employee's controller is 2.5 units tall; a crouch lerps it to 1.5.</summary>
        const float StandingHeight = 2.5f;

        /// <summary>YOUR employee (never another player's), or null in the menu, before spawning and
        /// while you are dead.</summary>
        public static PlayerControllerB Local =>
            GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;

        public static PlayerInfo? Locate()
        {
            var p = Local;
            if (p == null || p.isPlayerDead || !p.isPlayerControlled) return null;
            var cc = p.thisController;
            if (cc == null) return new PlayerInfo { Feet = p.transform.position, Forward = p.transform.forward, Root = p.gameObject, Grounded = true };
            var b = cc.bounds;
            return new PlayerInfo
            {
                Feet = new Vector3(b.center.x, b.min.y, b.center.z),
                Forward = p.transform.forward,
                Root = p.gameObject,
                Grounded = cc.isGrounded && !p.isClimbingLadder,
                // Her size follows the employee's STANDING height only: a crouch never shrinks her.
                Height = !p.isCrouching && cc.height > StandingHeight - 0.1f ? b.size.y : 0,
            };
        }
    }
}

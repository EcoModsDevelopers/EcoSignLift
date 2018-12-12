using Asphalt.Api.Event;
using Asphalt.Api.Event.PlayerEvents;
using Eco.Gameplay.Components;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Shared.Items;
using Eco.Shared.Math;
using Eco.Simulation.RouteProbing;
using System;

namespace EcoSignLift
{
    public class SignEventHandler
    {
        private const string SIGN_IDENTIFIER = "SignObject";

        [EventHandler]
        public void OnPlayerInteract(PlayerInteractEvent evt)
        {
            if (evt.Context.Method != InteractionMethod.Left && evt.Context.Method != InteractionMethod.Right)
                return;

            if (evt.Context.Player == null || evt.Context.Target == null || !(evt.Context.Target.GetType().ToString().Contains(SIGN_IDENTIFIER)))
                return;

            WorldObject sign = evt.Context.Target as WorldObject;

            if (sign == null)
                return;

            string text = sign.GetComponent<CustomTextComponent>().Text?.ToLower();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text.Contains("lift up"))
            {
                FindDestinationSignAndTeleport(evt.Context.Player, sign.Position3i, new Vector3i(0, 1, 0));
            }
            else if (text.Contains("lift down"))
            {
                FindDestinationSignAndTeleport(evt.Context.Player, sign.Position3i, new Vector3i(0, -1, 0));
            }
        }

        private void FindDestinationSignAndTeleport(Player pPlayer, Vector3i pos, Vector3i pDirection)
        {
            if (!hasNeightborIfNeeded(pos))
            {
                pPlayer.SendTemporaryError($"The sign lift only works if there is a { EcoSignLift.Config.GetString("NameOfRequiredItemNextToSign") } next to it.");
                return;
            }

            pos += pDirection * 2;

            while (pos.Y > 0 && pos.Y < EcoSignLift.Config.GetInt("MaximumLiftHeight"))
            {
                pos += pDirection;

                WorldObjectBlock worldObjectBlock = Eco.World.World.GetBlock(pos) as WorldObjectBlock;

                if (worldObjectBlock != null)
                {
                    WorldObject destination = worldObjectBlock.WorldObjectHandle.Object;

                    if (!destination.GetType().ToString().Contains(SIGN_IDENTIFIER))
                        continue;

                    if (destination != null)
                    {
                        string destext = destination.GetComponent<CustomTextComponent>().Text;
                        if (destext == null || !destext.ToLower().Contains("lift"))
                            continue;

                        Vector3 ppos = pPlayer.Position;

                        Vector3i destvec = new Vector3i((int)Math.Round(ppos.X), (int)Math.Round(destination.Position.Y - 1), (int)Math.Round(ppos.Z));

                        if (RouteManager.WorldBlockIsWalkable(destvec))
                        {
                            pPlayer.SetPosition(destvec);
                            return;
                        }

                        foreach (var neightbor in destvec.XZNeighbors)
                            if (RouteManager.WorldBlockIsWalkable(neightbor))
                            {
                                pPlayer.SetPosition(neightbor);
                                return;
                            }

                        pPlayer.SendTemporaryError($"Destination is occupied");
                        return;
                    }
                }
            }

            pPlayer.SendTemporaryError($"No destination found!");
        }

        private bool hasNeightborIfNeeded(Vector3i pPosition)
        {
            if (!EcoSignLift.Config.Get<bool>("RequireItemNextToSign"))
                return true;

            foreach (var neightbor in pPosition.Full26Neighbors)
                if (Eco.World.World.GetBlock(neightbor).GetType().Name.Contains(EcoSignLift.Config.GetString("NameOfRequiredItemNextToSign")))
                    return true;

            return false;
        }
    }
}
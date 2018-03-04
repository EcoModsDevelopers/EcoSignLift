// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using Eco.Gameplay.Interactions;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Shared.Math;
    using Eco.Shared.Serialization;
    using Eco.Simulation.RouteProbing;
    using Eco.World.Blocks;
    using Gameplay.Components;
    using Gameplay.Components.Auth;
    using System;

    [RequireComponent(typeof(CustomTextComponent))]
    public partial class WoodSignObject : WorldObject
    {

        /***************************** Configuration *******************************/

        private const bool NEEDS_ITEM_NEXT_TO_SIGN = false;     //use true or false
        private const string NAME_OF_BLOCK = "FlatSteel";       //Every blockname that contains FlatStell will be ok. So FlatSteelFloorBlock, FlatSteelWallBlock, etc will work

        /***************************************************************************/

        private const int MAX_HEIGHT = 10000;

        protected override void PostInitialize()
        {
            this.GetComponent<PropertyAuthComponent>().Initialize(AuthModeType.Inherited);
        }

        public override InteractResult OnActRight(InteractionContext context)
        {
            string text = GetComponent<CustomTextComponent>().Text;

            if (string.IsNullOrWhiteSpace(text))
                return base.OnActRight(context);

            if (text.ToLower().Contains("lift up"))
            {
                FindDestinationSignAndTeleport(context.Player, new Vector3i(0, 1, 0));
            }
            else if (text.ToLower().Contains("lift down"))
            {
                FindDestinationSignAndTeleport(context.Player, new Vector3i(0, -1, 0));
            }

            return base.OnActRight(context);
        }

        private void FindDestinationSignAndTeleport(Player pPlayer, Vector3i pDirection)
        {
            Vector3i pos = Position3i;

            if (!hasNeightborIfNeeded(pos))
            {
                pPlayer.SendTemporaryError("The sign lift only works if there is a " + NAME_OF_BLOCK + " next to it.");
                return;
            }

            pos += pDirection * 2;

            while (pos.Y > 0 && pos.Y < MAX_HEIGHT)
            {
                pos += pDirection;

                WorldObjectBlock worldObjectBlock = Eco.World.World.GetBlock(pos) as WorldObjectBlock;

                if (worldObjectBlock != null)
                {
                    WoodSignObject destination = worldObjectBlock.WorldObjectHandle.Object as WoodSignObject;

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

                        pPlayer.SendTemporaryError("Destination is occupied");
                        return;
                    }
                }
            }

            pPlayer.SendTemporaryError("No destination found!");
        }

        private bool hasNeightborIfNeeded(Vector3i pPosition)
        {
            if (!NEEDS_ITEM_NEXT_TO_SIGN)
                return true;

            foreach (var neightbor in pPosition.Full26Neighbors)
                if (Eco.World.World.GetBlock(neightbor).GetType().Name.Contains(NAME_OF_BLOCK))
                    return true;

            return false;
        }
    }

    [Serialized]
    public partial class SmallWoodSignObject : WoodSignObject
    { }
}
﻿namespace ClashRoyale.Server.Network.Packets.Server
{
    using System.Collections.Generic;

    using ClashRoyale.Enums;
    using ClashRoyale.Extensions;
    using ClashRoyale.Server.Logic;
    using ClashRoyale.Server.Logic.Commands;
    using ClashRoyale.Server.Logic.Commands.Manager;

    internal class SectorHearbeatMessage : Message
    {
        internal int ServerTurn;
        internal int Checksum;

        internal List<Command> Commands;

        /// <summary>
        /// Gets the type of this message.
        /// </summary>
        internal override short Type
        {
            get
            {
                return 21902;
            }
        }

        /// <summary>
        /// Gets the service node of this message.
        /// </summary>
        internal override Node ServiceNode
        {
            get
            {
                return Node.Sector;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectorHearbeatMessage"/> class.
        /// </summary>
        /// <param name="Device">The device.</param>
        /// <param name="ServerTurn">The server turn.</param>
        /// <param name="Checksum">The checksum.</param>
        /// <param name="Commands">The commands.</param>
        public SectorHearbeatMessage(Device Device, int ServerTurn, int Checksum, List<Command> Commands) : base(Device)
        {
            this.ServerTurn = ServerTurn;
            this.Checksum = Checksum;
            this.Commands = Commands;
        }

        /// <summary>
        /// Encodes this instance.
        /// </summary>
        internal override void Encode()
        {
            this.Stream.WriteVInt(this.ServerTurn);
            this.Stream.WriteVInt(this.Checksum);

            if (this.Commands.Count > 0)
            {
                this.Stream.WriteVInt(this.Commands.Count);

                ChecksumEncoder Encoder = new ChecksumEncoder(this.Stream);

                for (int I = 0; I < this.Commands.Count; I++)
                {
                    CommandManager.EncodeCommand(this.Commands[I], Encoder);
                }
            }
            else
            {
                this.Stream.WriteVInt(0);
            }
        }
    }
}
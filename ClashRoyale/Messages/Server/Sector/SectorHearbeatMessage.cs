﻿namespace ClashRoyale.Messages.Server.Sector
{
    using System.Collections.Generic;

    using ClashRoyale.Enums;
    using ClashRoyale.Extensions;
    using ClashRoyale.Logic.Commands;
    using ClashRoyale.Logic.Commands.Manager;

    public class SectorHearbeatMessage : Message
    {
        /// <summary>
        /// Gets the type of this message.
        /// </summary>
        public override short Type
        {
            get
            {
                return 21443;
            }
        }

        /// <summary>
        /// Gets the service node of this message.
        /// </summary>
        public override Node ServiceNode
        {
            get
            {
                return Node.Sector;
            }
        }

        public int ServerTurn;
        public int Checksum;

        public List<Command> Commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectorHearbeatMessage"/> class.
        /// </summary>
        /// <param name="ServerTurn">The server turn.</param>
        /// <param name="Checksum">The checksum.</param>
        /// <param name="Commands">The commands.</param>
        public SectorHearbeatMessage(int ServerTurn, int Checksum, List<Command> Commands)
        {
            this.ServerTurn = ServerTurn;
            this.Checksum = Checksum;
            this.Commands = Commands;
        }

        /// <summary>
        /// Encodes this instance.
        /// </summary>
        public override void Encode()
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
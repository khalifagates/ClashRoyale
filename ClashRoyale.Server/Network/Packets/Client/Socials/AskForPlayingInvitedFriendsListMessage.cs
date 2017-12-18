﻿namespace ClashRoyale.Server.Network.Packets.Client
{
    using System.Collections.Generic;

    using ClashRoyale.Enums;
    using ClashRoyale.Extensions;
    using ClashRoyale.Server.Logic;
    using ClashRoyale.Server.Logic.Player;
    using ClashRoyale.Server.Network.Packets.Server;

    internal class AskForPlayingInvitedFriendsListMessage : Message
    {
        /// <summary>
        /// The type of this message.
        /// </summary>
        internal override short Type
        {
            get
            {
                return 10504;
            }
        }

        /// <summary>
        /// The service node of this message.
        /// </summary>
        internal override Node ServiceNode
        {
            get
            {
                return Node.Friend;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AskForPlayingInvitedFriendsListMessage"/> class.
        /// </summary>
        /// <param name="Device">The device.</param>
        /// <param name="ByteStream">The byte stream.</param>
        public AskForPlayingInvitedFriendsListMessage(Device Device, ByteStream ByteStream) : base(Device, ByteStream)
        {
            // AskForPlayingInvitedFriendsListMessage.
        }

        /// <summary>
        /// Processes this message.
        /// </summary>
        internal override void Process()
        {
            this.Device.NetworkManager.SendMessage(new FriendsListMessage(this.Device, new List<Player>(0)));
        }
    }
}
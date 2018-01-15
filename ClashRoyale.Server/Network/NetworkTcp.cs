﻿namespace ClashRoyale.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using ClashRoyale.Enums;
    using ClashRoyale.Logic;
    using ClashRoyale.Logic.Collections;
    using ClashRoyale.Messages;

    public static class NetworkTcp
    {
        private static NetworkPool ReadPool;
        private static NetworkPool WritePool;

        private static Socket Listener;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NetworkTcp"/> has been already initialized.
        /// </summary>
        private static bool Initialized
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGateway"/> class.
        /// </summary>
        public static void Initialize()
        {
            if (NetworkTcp.Initialized)
            {
                return;
            }

            NetworkTcp.ReadPool             = new NetworkPool();
            NetworkTcp.WritePool            = new NetworkPool();

            foreach (var AsyncEvent in NetworkTcp.ReadPool)
            {
                AsyncEvent.Completed += NetworkTcp.OnReceiveCompleted;
            }

            foreach (var AsyncEvent in NetworkTcp.WritePool)
            {
                AsyncEvent.Completed += NetworkTcp.OnSendCompleted;
            }

            NetworkTcp.Listener             = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            NetworkTcp.Listener.NoDelay     = true;
            NetworkTcp.Listener.Blocking    = false;

            NetworkTcp.Initialized          = true;

            NetworkTcp.Listener.Bind(new IPEndPoint(IPAddress.Any, 9339));
            NetworkTcp.Listener.Listen(150);

            Logging.Info(typeof(NetworkTcp), "Listener has been bound to " + NetworkTcp.Listener.LocalEndPoint + ".");

            SocketAsyncEventArgs AcceptEvent = new SocketAsyncEventArgs();
            AcceptEvent.Completed += NetworkTcp.OnAcceptCompleted;
            AcceptEvent.DisconnectReuseSocket = true;

            NetworkTcp.StartAccept(AcceptEvent);
        }

        /// <summary>
        /// Accepts a TCP Request.
        /// </summary>
        /// <param name="AcceptEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void StartAccept(SocketAsyncEventArgs AcceptEvent)
        {
            AcceptEvent.AcceptSocket    = null;
            AcceptEvent.RemoteEndPoint  = null;

            if (!NetworkTcp.Listener.AcceptAsync(AcceptEvent))
            {
                NetworkTcp.OnAcceptCompleted(null, AcceptEvent);
            }
        }

        /// <summary>
        /// Called when the client has been accepted.
        /// </summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void OnAcceptCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.SocketError == SocketError.Success)
            {
                NetworkTcp.ProcessAccept(AsyncEvent);
            }
            else
            {
                NetworkTcp.StartAccept(AsyncEvent);
            }
        }

        /// <summary>
        /// Accept the new client and store it in memory.
        /// </summary>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void ProcessAccept(SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.AcceptSocket.Connected)
            {
                string IpAddress = ((IPEndPoint) AsyncEvent.AcceptSocket.RemoteEndPoint).Address.ToString();

                if (IpAddress.StartsWith("192.168."))
                {
                    SocketAsyncEventArgs ReadEvent = NetworkTcp.ReadPool.Dequeue();

                    if (ReadEvent != null)
                    {
                        NetworkToken Token  = new NetworkToken(ReadEvent, AsyncEvent.AcceptSocket);
                        Device Device       = new Device(Token);

                        Devices.Add(Device);

                        if (!Token.Socket.ReceiveAsync(ReadEvent))
                        {
                            NetworkTcp.ProcessReceive(ReadEvent);
                        }
                    }
                    else
                    {
                        Logging.Warning(typeof(NetworkTcp), "Server is full, new connections cannot be accepted.");
                    }
                }
            }

            NetworkTcp.StartAccept(AsyncEvent);
        }

        /// <summary>
        /// Receives data from the specified client.
        /// </summary>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void ProcessReceive(SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.BytesTransferred == 0)
            {
                NetworkTcp.Disconnect(AsyncEvent);
            }

            if (AsyncEvent.SocketError != SocketError.Success)
            {
                NetworkTcp.Disconnect(AsyncEvent);
            }

            NetworkToken Token = (NetworkToken) AsyncEvent.UserToken;

            if (Token.IsConnected)
            {
                Token.AddData();

                try
                {
                    if (Token.Socket.Available == 0)
                    {
                        Token.Process();
                    }

                    if (Token.IsFailing == false)
                    {
                        if (!Token.Socket.ReceiveAsync(AsyncEvent))
                        {
                            NetworkTcp.ProcessReceive(AsyncEvent);
                        }
                    }
                    else
                    {
                        NetworkTcp.Disconnect(AsyncEvent);
                    }
                }
                catch (Exception Exception)
                {
                    Logging.Warning(typeof(NetworkTcp), Exception.GetType().Name + " thrown at ProcessReceive(" + AsyncEvent.RemoteEndPoint + ").");
                    NetworkTcp.Disconnect(AsyncEvent);
                }
            }
            else
            {
                NetworkTcp.Disconnect(AsyncEvent);
            }
        }

        /// <summary>
        /// Called when [receive completed].
        /// </summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void OnReceiveCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.SocketError == SocketError.Success)
            {
                NetworkTcp.ProcessReceive(AsyncEvent);
            }
            else
            {
                NetworkTcp.Disconnect(AsyncEvent);
            }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="Buffer">The buffer.</param>
        /// <param name="Token">The token.</param>
        public static void Send(byte[] Buffer, NetworkToken Token)
        {
            if (Buffer == null)
            {
                throw new ArgumentNullException(nameof(Message), "Buffer == null at Send(Buffer, Token).");
            }

            if (Token == null)
            {
                throw new ArgumentNullException(nameof(Token), "Token == null at Send(Buffer, Token).");
            }

            if (Token.IsConnected)
            {
                SocketAsyncEventArgs WriteEvent = NetworkTcp.WritePool.Dequeue();

                if (WriteEvent == null)
                {
                    WriteEvent = new SocketAsyncEventArgs
                    {
                        DisconnectReuseSocket = false
                    };
                }

                WriteEvent.SetBuffer(Buffer, 0, Buffer.Length);

                WriteEvent.AcceptSocket     = Token.Socket;
                WriteEvent.RemoteEndPoint   = Token.Socket.RemoteEndPoint;
                WriteEvent.UserToken        = Token;

                if (!Token.Socket.SendAsync(WriteEvent))
                {
                    NetworkTcp.ProcessSend(WriteEvent);
                }
            }
            else
            {
                NetworkTcp.Disconnect(Token.AsyncEvent);
            }
        }

        /// <summary>
        /// Processes to send a <see cref="Message"/> using the specified <see cref="SocketAsyncEventArgs"/>.
        /// </summary>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void ProcessSend(SocketAsyncEventArgs AsyncEvent)
        {
            NetworkToken Token = (NetworkToken) AsyncEvent.UserToken;

            if (AsyncEvent.SocketError == SocketError.Success)
            {
                if (AsyncEvent.Count > AsyncEvent.BytesTransferred)
                {
                    int Offset = AsyncEvent.Offset + AsyncEvent.BytesTransferred;

                    if (Token.IsConnected)
                    {
                        AsyncEvent.SetBuffer(Offset, AsyncEvent.Buffer.Length - Offset);

                        if (!Token.Socket.SendAsync(AsyncEvent))
                        {
                            NetworkTcp.ProcessSend(AsyncEvent);
                        }

                        return;
                    }

                    NetworkTcp.Disconnect(Token.AsyncEvent);
                }
            }
            else
            {
                NetworkTcp.Disconnect(Token.AsyncEvent);
            }

            NetworkTcp.OnSendCompleted(null, AsyncEvent);
        }

        /// <summary>
        /// Called when [send completed].
        /// </summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        private static void OnSendCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            NetworkTcp.WritePool.Enqueue(AsyncEvent);
        }

        /// <summary>
        /// Closes the specified client's socket.
        /// </summary>
        /// <param name="AsyncEvent">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        public static void Disconnect(SocketAsyncEventArgs AsyncEvent)
        {
            NetworkToken Token = AsyncEvent.UserToken as NetworkToken;

            if (Token.IsAborting)
            {
                return;
            }

            Token.IsAborting = true;

            if (Token.Device != null)
            {
                Devices.Remove(Token.Device);

                Token.Device.State = State.Disconnected;

                if (Token.IsConnected)
                {
                    try
                    {
                        Token.Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception Exception)
                    {
                        Logging.Warning(typeof(NetworkTcp), Exception.GetType().Name + " thrown at Disconnect(" + AsyncEvent.RemoteEndPoint + ").");
                    }
                }

                Token.Socket.Close();
            }

            NetworkTcp.ReadPool.Enqueue(AsyncEvent);
        }
    }
}

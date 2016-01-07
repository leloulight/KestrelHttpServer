// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Server.Kestrel.Infrastructure;
using Microsoft.AspNet.Server.Kestrel.Networking;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Server.Kestrel.Http
{
    /// <summary>
    /// Implementation of <see cref="Listener"/> that uses TCP sockets as its transport.
    /// </summary>
    public class TcpListener : Listener
    {
        public TcpListener(ServiceContext serviceContext) : base(serviceContext)
        {
        }

        /// <summary>
        /// Creates the socket used to listen for incoming connections
        /// </summary>
        protected override UvStreamHandle CreateListenSocket()
        {
            var socket = new UvTcpHandle(Log);
            socket.Init(Thread.Loop, Thread.QueueCloseHandle);
            socket.NoDelay(NoDelay);
            socket.Bind(ServerAddress);
            socket.Listen(Constants.ListenBacklog, (stream, status, error, state) => ConnectionCallback(stream, status, error, state), this);
            return socket;
        }

        /// <summary>
        /// Handle an incoming connection
        /// </summary>
        /// <param name="listenSocket">Socket being used to listen on</param>
        /// <param name="status">Connection status</param>
        protected override void OnConnection(UvStreamHandle listenSocket, int status)
        {
            var acceptSocket = new UvTcpHandle(Log);

            try
            {
                acceptSocket.Init(Thread.Loop, Thread.QueueCloseHandle);
                acceptSocket.NoDelay(NoDelay);
                listenSocket.Accept(acceptSocket);
                DispatchConnection(acceptSocket);

            }
            catch (UvException ex)
            {
                Log.LogError("TcpListener.OnConnection", ex);
                acceptSocket.Dispose();
                return;
            }
        }
    }
}

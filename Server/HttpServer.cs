﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteFork.Server {
    internal abstract class HttpServer {
        private readonly TcpListener listener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        
        private const int TIME_OUT = 10;

        protected HttpServer(IPAddress ip, int port) {
            Logger.Info("Server start");
            listener = new TcpListener(new IPEndPoint(ip, port));
            listener.Start();
        }

        public void Listen() {
            while (!cts.IsCancellationRequested) {
                try {
                    TcpClient client = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(client, this);

                    ThreadPool.QueueUserWorkItem(processor.Process);

                    Thread.Sleep(TIME_OUT);
                } catch (Exception) {
                    Logger.Error("Server crashed");
                }
            }
        }

        public void Stop() {
            Logger.Info("Server stop");
            cts.Cancel();
            if (listener != null) {
                listener.Stop();
            }
        }

        public abstract void HandleGetRequest(HttpProcessor processor);

        public abstract void HandlePostRequest(HttpProcessor processor, StreamReader inputData);
    }
}

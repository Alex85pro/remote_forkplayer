﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteFork {
    public abstract class HttpServer {
        private readonly TcpListener listener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private const int TimeOut = 10;

        protected HttpServer(IPAddress ip, int port) {
            listener = new TcpListener(new IPEndPoint(ip, port));
            listener.Start();
        }

        public void Listen() {
            while (!cts.IsCancellationRequested) {
                try {
                    TcpClient client = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(client, this);

                    ThreadPool.QueueUserWorkItem(processor.Process);

                    Thread.Sleep(TimeOut);
                } catch (Exception) {
                    Console.WriteLine("Stop");
                }
            }
        }

        public void Stop() {
            cts.Cancel();
            if (listener != null) {
                listener.Stop();
            }
        }

        public abstract void HandleGetRequest(HttpProcessor processor);

        public abstract void HandlePostRequest(HttpProcessor processor, StreamReader inputData);
    }
}

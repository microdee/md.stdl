using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Coding;

namespace md.stdl.Network
{
    /// <inheritdoc />
    /// <summary>
    /// High level observable wrapper around WebClient downloading
    /// </summary>
    public class DownloadSlot : IObservable<DownloadSlot>
    {
        /// <summary>
        /// Url to file to download
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Destination file path
        /// </summary>
        public string Destination { get; private set; }

        /// <summary>
        /// Received bytes
        /// </summary>
        public long Received { get; private set; }
        /// <summary>
        /// Total Bytes
        /// </summary>
        public long Total { get; private set; }

        /// <summary>
        /// The task thread of downloading
        /// </summary>
        public Task DownloadTask { get; private set; }

#pragma warning disable CS1591
        public int Percent { get; private set; }
        public bool Ready { get; private set; }
        public bool Success { get; private set; }
        public bool Error { get; private set; }
        public string Message { get; private set; }
        public WebClient Client { get; private set; }
        public DownloadProgressChangedEventArgs LastProgress { get; private set; }
#pragma warning restore CS1591

        private readonly List<IObserver<DownloadSlot>> _observers = new List<IObserver<DownloadSlot>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="src">Source Url</param>
        /// <param name="dst">Path to destination file</param>
        public DownloadSlot(string src, string dst)
        {
            Client = new WebClient();
            Client.DownloadProgressChanged += (sender, args) =>
            {
                Received = args.BytesReceived;
                Total = args.TotalBytesToReceive;
                Percent = args.ProgressPercentage;
                LastProgress = args;

                foreach (var observer in _observers)
                    observer.OnNext(this);
            };
            Client.DownloadFileCompleted += (sender, args) =>
            {
                Ready = true;
                if (args.Error != null)
                {
                    Error = true;
                    Message = args.Error.Message;
                    Message += "\n" + args.Error.InnerException?.Message;
                    foreach (var observer in _observers)
                        observer.OnError(args.Error.InnerException ?? new Exception("Unknown error"));
                }
                else
                {
                    Success = true;
                    foreach (var observer in _observers)
                        observer.OnCompleted();
                }
            };
            Url = src;
            Destination = dst;
        }
        
        /// <summary>
        /// Invoke to actually start downloading
        /// </summary>
        public void Start()
        {
            DownloadTask = Client.DownloadFileTaskAsync(new Uri(Url), Destination);
        }

        /// <inheritdoc />
        /// <summary>
        /// Subscribe an observer where OnNext is progress, OnError is error, and OnCompleted is completition
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<DownloadSlot> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<DownloadSlot, DownloadSlot>(this, observer, (obsrl, obser) => _observers.Remove(obser));
        }
    }
}

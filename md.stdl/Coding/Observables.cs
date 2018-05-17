﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interfaces;

namespace md.stdl.Coding
{
    /// <inheritdoc />
    /// <summary>
    /// Generic unsubscriber for Observables
    /// </summary>
    /// <typeparam name="TObservable">Type of the observable</typeparam>
    /// <typeparam name="TArg">Argument type of the Observable</typeparam>
    public class Unsubscriber<TObservable, TArg> : IDisposable where TObservable : IObservable<TArg>
    {
        private readonly Action<TObservable, IObserver<TArg>> _removal;
        private readonly TObservable _observable;
        private readonly IObserver<TArg> _observer;

        /// <summary>
        /// Construct the Unsubscriber
        /// </summary>
        /// <param name="observer">The unsubscribing observer</param>
        /// <param name="observable">The observable which has the observer</param>
        /// <param name="removal">The method of subscription removal from the observable</param>
        public Unsubscriber(TObservable observable,
            IObserver<TArg> observer,
            Action<TObservable, IObserver<TArg>> removal)
        {
            _removal = removal;
            _observable = observable;
            _observer = observer;
        }
        public void Dispose()
        {
            _removal?.Invoke(_observable, _observer);
        }
    }

    /// <inheritdoc cref="IMainlooping"/>
    /// <summary>
    /// Flattens an event into a single frame boolean in a mainloop
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    public class EventFlattener<TDelegate, TEventArgs> : IMainlooping
    {
        private readonly MainloopScheduler _scheduler = new MainloopScheduler();
        /// <summary>
        /// True on the frame when the event is occured
        /// </summary>
        public bool Bang { get; private set; }

        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopBegin;
        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopEnd;

        /// <summary></summary>
        /// <param name="addhandler">Action which subscribes to an event</param>
        /// <param name="removehandler">Action which unsubscribes from an event</param>
        public EventFlattener(Action<TDelegate> addhandler, Action<TDelegate> removehandler)
        {
            Observable.FromEvent<TDelegate, TEventArgs>(addhandler, removehandler)
                .Edge()
                .ObserveOn(_scheduler)
                .Subscribe(v => Bang = v);
        }

        /// <inheritdoc cref="IMainlooping"/>
        public void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);
            _scheduler.Mainloop();
            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Extension methods for observables
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Generates an edge (true, false) in the output sequence for each
        /// element received from the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <returns>
        /// An observable sequence containing an edge (true, false) for each 
        /// element from the source sequence.
        /// </returns>
        public static IObservable<bool> Edge<T>(this IObservable<T> source)
        {
            return source.SelectMany(_ => new[] { true, false });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Action<TObservable, IObserver<TArg>> _removal;
        private TObservable _observable;
        private IObserver<TArg> _observer;

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
}

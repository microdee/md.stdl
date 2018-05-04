using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Interfaces
{
    /// <summary>
    /// An interface generalizing per-frame/mainlooping behavior
    /// </summary>
    public interface IMainlooping
    {
        /// <summary>
        /// First thing should be invoked in the mainloop
        /// </summary>
        event EventHandler OnMainLoopBegin;

        /// <summary>
        /// Last thing should be invoked in the mainloop
        /// </summary>
        event EventHandler OnMainLoopEnd;

        /// <summary>
        /// Function to be called once every frame
        /// </summary>
        /// <param name="deltatime">Delta time between frames usually in seconds but can be ms (depending on the implementer)</param>
        void Mainloop(float deltatime);
    }
}

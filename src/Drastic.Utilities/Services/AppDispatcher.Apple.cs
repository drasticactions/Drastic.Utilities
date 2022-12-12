// <copyright file="AppDispatcher.Apple.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace Drastic.Services
{
    /// <summary>
    /// App Dispatcher.
    /// </summary>
    public class AppDispatcher : NSObject, IAppDispatcher
    {
        /// <inheritdoc/>
        public bool Dispatch(Action action)
        {
            this.InvokeOnMainThread(action);
            return true;
        }
    }
}

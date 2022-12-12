// <copyright file="NavigationEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace Drastic.Events
{
    /// <summary>
    /// Navigation Event Arguments.
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationEventArgs"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        public NavigationEventArgs(Type type, object? arguments)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            this.Type = type;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Gets the Type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public object? Arguments { get; }
    }
}

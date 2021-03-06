// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides information to guide the production of a strongly-typed logging method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LoggerMessageAttribute : Attribute
    {
        /// <summary>
        /// Creates an attribute to guide the production of a strongly-typed logging method.
        /// </summary>
        /// <param name="eventId">The stable event id for this log message.</param>
        /// <param name="level">THe logging level produced when invoking the strongly-typed logging method.</param>
        /// <param name="message">The message text output by the logging method. This string is a template that can contain any of the method's parameters.</param>
        /// <remarks>
        /// The method this attribute is applied to:
        ///    - Must be a partial method.
        ///    - Must be a static method.
        ///    - Must return <c>void</c>.
        ///    - Must not be generic.
        ///    - Must have an ILogger as first parameter.
        ///    - None of the parameters can be generic.
        /// </remarks>
        /// <example>
        /// static partial class Log
        /// {
        ///     [LoggerMessage(0, LogLevel.Critical, "Could not open socket for {hostName}")]
        ///     void CouldNotOpenSocket(string hostName);
        /// }
        /// </example>
        public LoggerMessageAttribute(int eventId, LogLevel level, string message) => (EventId, Level, Message) = (eventId, level, message);

        /// <summary>
        /// Gets or sets the logging event id for the logging method.
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Gets or sets the logging event name for the logging method.
        /// </summary>
        /// <remarks>
        /// This will equal the method name if not specified.
        /// </remarks>
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the logging level for the logging method.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the message text for the logging method.
        /// </summary>
        public string Message { get; set; }
    }
}

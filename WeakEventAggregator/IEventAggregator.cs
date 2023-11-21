namespace WeakEventAggregator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for EventAggregators.
    /// </summary>
    /// <remarks>
    ///   <para><b>History</b></para>
    ///   <list type="table">
    ///   <item>
    ///   <term><b>Author:</b></term>
    ///   <description>Marc Armbruster</description>
    ///   </item>
    ///   <item>
    ///   <term><b>Date:</b></term>
    ///   <description>11/20/2023 2:01:26 PM</description>
    ///   </item>
    ///   <item>
    ///   <term><b>Remarks:</b></term>
    ///   <description>Initial version.</description>
    ///   </item>
    ///   </list>
    /// </remarks>
    public interface IEventAggregator
    {
        /// <summary>
        /// Name of this event aggregator instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Are there any registrations for this type.
        /// </summary>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns>True if registrations are found.</returns>
        bool HasRegistrations<TEventType>()
            where TEventType : IEvent;

        /// <summary>
        /// Gets all registered delegates for a given event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        /// <returns>All registered delegates.</returns>
        IEnumerable<Delegate> GetRegisteredDelegates<TEventType>()
            where TEventType : IEvent;

        /// <summary>
        /// Removes all registered delegates for an event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        void RemoveTypeRegistrations<TEventType>()
            where TEventType : IEvent;

        /// <summary>
        /// Publish an event within a separate Task.
        /// </summary>
        /// <typeparam name="TEventType">The event type</typeparam>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <param name="payload">The payload</param>      
        Task PublishAsync<TEventType, TPayload>(TPayload payload)
            where TEventType : IEvent
            where TPayload : IPayload;

        /// <summary>
        /// Publish an event.
        /// </summary>
        /// <typeparam name="TEventType">The event type</typeparam>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <param name="payload">The payload</param>        
        void Publish<TEventType, TPayload>(TPayload payload) 
            where TEventType : IEvent
            where TPayload : IPayload;

        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <param name="action">The delegate to be called.</param>
        void Subscribe<TEventType, TPayload>(Action<TPayload> action)
            where TEventType : IEvent
            where TPayload : IPayload;

        /// <summary>
        /// Unsubscribe event.
        /// </summary>
        /// <param name="action">The delegate to be unregistered.</param>
        void Unsubscribe<TEventType, TPayload>(Action<TPayload> action)
            where TEventType : IEvent
            where TPayload : IPayload;
    }
}
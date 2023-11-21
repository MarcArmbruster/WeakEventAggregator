namespace WeakEventAggregator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Default Event Aggregator.
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
    ///   <description>11/20/2023 1:58:47 PM</description>
    ///   </item>
    ///   <item>
    ///   <term><b>Remarks:</b></term>
    ///   <description>Initial version.</description>
    ///   </item>
    ///   </list>
    /// </remarks>
    internal class EventAggregator : IEventAggregator
    {
        /// <summary>
        /// The registrations repository.
        /// </summary>
        private readonly ConcurrentDictionary<string, List<WeakReference<Delegate>>> registrations
            = new ConcurrentDictionary<string, List<WeakReference<Delegate>>>();

        /// <summary>
        /// Name of this eventa aggregator instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this event aggregator instance.</param>
        public EventAggregator(string name)
        {
            this.Name = name; 
        }

        /// <summary>
        /// Are there any registrations for this type.
        /// </summary>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns>True if registrations are found.</returns>
        public bool HasRegistrations<TEventType>() 
            where TEventType : IEvent
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            return registrations.ContainsKey(key);
        }

        /// <summary>
        /// Gets all registered delegates for a given event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        /// <returns>All registered delegates.</returns>
        public IEnumerable<Delegate> GetRegisteredDelegates<TEventType>()
            where TEventType : IEvent
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (registrations.ContainsKey(key))
            {
                List<Delegate> delegates = new List<Delegate>();
                var references = registrations[key];
                references.ForEach(r =>
                { 
                    if (r.TryGetTarget(out var target))
                    {
                        delegates.Add(target);
                    }
                });

                return delegates;
            }

            return Enumerable.Empty<Delegate>();
        }

        /// <summary>
        /// Removes all registered delegates for an event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        public void RemoveTypeRegistrations<TEventType>()
            where TEventType : IEvent
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (registrations.ContainsKey(key))
            {
                registrations[key].Clear();
                registrations.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Publish an event within a separate Task.
        /// </summary>
        /// <typeparam name="TEventType">The event type</typeparam>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <remarks>
        /// HINT: Use this method only, if you are sure that the registered delegates can run within another background task!
        /// </remarks>
        /// <param name="payload">The payload</param>      
        public Task PublishAsync<TEventType, TPayload>(TPayload payload)
            where TEventType : IEvent
            where TPayload : IPayload
        {
            return Task.Run(() => this.Publish<TEventType, TPayload>(payload));
        }

        /// <summary>
        /// Publish an event.
        /// </summary>
        /// <typeparam name="TEventType">The event type</typeparam>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <param name="payload">The payload</param>      
        public void Publish<TEventType, TPayload>(TPayload payload)
            where TEventType : IEvent
            where TPayload : IPayload
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (registrations.ContainsKey(key))
            {
                foreach (var actionReference in registrations[key])
                {
                    if (actionReference.TryGetTarget(out var action))
                    {
                        if (action is Action<TPayload> payloadAction)
                        {
                            payloadAction.Invoke(payload);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <param name="action">The delegate to be called.</param>
        public void Subscribe<TEventType, TPayload>(Action<TPayload> action)
            where TEventType : IEvent
            where TPayload : IPayload
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (!registrations.ContainsKey(key))
            {
                registrations.AddOrUpdate(key, new List<WeakReference<Delegate>>(), (k, v) => new List<WeakReference<Delegate>>());
                registrations[key].Add(new WeakReference<Delegate>(action));
            }
            else
            {
                foreach (var actionReference in registrations[key])
                {
                    if (actionReference.TryGetTarget(out var registeredAction))
                    {
                        if (registeredAction.Equals(action))
                        {
                            // skip multiple registrations
                            return;                            
                        }
                    }
                }

                registrations[key].Add(new WeakReference<Delegate>(action));
            }
        }

        /// <summary>
        /// Unsubscribe event.
        /// </summary>
        /// <param name="action">The delegate to be unregistered.</param>
        public void Unsubscribe<TEventType, TPayload>(Action<TPayload> action)
            where TEventType : IEvent
            where TPayload : IPayload
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (registrations.ContainsKey(key))
            {
                List<WeakReference<Delegate>> removeables = new List<WeakReference<Delegate>>();
                foreach (var actionReference in registrations[key])
                {
                    if (actionReference.TryGetTarget(out var registeredAction))
                    {
                        if (registeredAction.Equals(action))
                        {
                            removeables.Add(actionReference);                            
                        }
                    }
                }

                foreach (var reference in removeables)
                {
                    registrations[key].Remove(reference);
                }
            }

            if (registrations[key].Count == 0)
            {
                registrations.TryRemove(key, out _);
            }
        }
    }
}
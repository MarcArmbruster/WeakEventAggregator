﻿namespace WeakEventAggregator
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
        private readonly ConcurrentDictionary<string, List<DelegateReference>> registrations
            = new ConcurrentDictionary<string, List<DelegateReference>>();

        /// <summary>
        /// Name of this event aaggregator instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// If true: payload mixture will be checked (raises an exception) 
        /// </summary>
        public bool CheckPayloadMixture { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this event aggregator instance.</param>
        public EventAggregator(string name)
        {
            this.Name = name; 
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of this event aggregator instance.</param>
        /// <param name="checkPayloadMixture">If true: payload mixture will be checked (raises an exception). False by default.</param>
        public EventAggregator(string name, bool checkPayloadMixture) : this(name)
        {
            this.CheckPayloadMixture = checkPayloadMixture;
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
            return this.registrations.ContainsKey(key);
        }

        /// <summary>
        /// Gets all registered delegates for a given event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        /// <returns>All registered delegates.</returns>
        public IEnumerable<DelegateReference> GetRegisteredDelegates<TEventType>()
            where TEventType : IEvent
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (this.registrations.ContainsKey(key))
            {
                return this.registrations[key];
            }

            return Enumerable.Empty<DelegateReference>();
        }

        /// <summary>
        /// Removes all registered delegates for an event type.
        /// </summary>
        /// <typeparam name="TEventType">The event type.</typeparam>
        public void RemoveTypeRegistrations<TEventType>()
            where TEventType : IEvent
        {
            string key = typeof(TEventType).FullName ?? string.Empty;
            if (this.registrations.ContainsKey(key))
            {
                this.registrations[key].Clear();
                _ = this.registrations.TryRemove(key, out _);
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
            if (this.registrations.ContainsKey(key))
            {
                foreach (var actionReference in this.registrations[key])
                {
                    _ = actionReference.Target.DynamicInvoke(payload);
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
                this.registrations.AddOrUpdate(key, new List<DelegateReference> (), (k, v) => new List<DelegateReference>());
                this.registrations[key].Add(new DelegateReference(action, false));
            }
            else
            {
                this.AvoidPayloadMixture(key, action.GetType());
                foreach (var actionReference in registrations[key])
                {
                    if (actionReference.Target is Delegate registeredAction)
                    {
                        if (registeredAction.Equals(action))
                        {
                            // skip multiple registrations
                            return;
                        }
                    }
                }

                this.registrations[key].Add(new DelegateReference(action, false));
            }
        }

        /// <summary>
        /// Checks the uniqueness of EnventType-PayloadType pair within the internal registrations.
        /// </summary>
        /// <param name="typeKey">The type key</param>
        /// <param name="nextActionType">The type of the next delegate to be registered.</param>
        /// <exception cref="PayloadMixtureException">Exception in case of a detected mixture.</exception>
        private void AvoidPayloadMixture(string typeKey, Type nextActionType)
        {
            if (!this.CheckPayloadMixture)
            {
                return;
            }

            if (this.registrations.ContainsKey(typeKey))
            {
                var references = this.registrations[typeKey];
                foreach (var reference in references)
                {
                    if (reference.Target is Delegate action)
                    {
                        string existingFullName = action.GetType().FullName;
                        string newFullName = nextActionType.FullName;
                        if (existingFullName != newFullName)
                        {
                            throw new PayloadMixtureException(
                                $@"For an event type only one distinct payload type is allowed. 
                                   Do not use a mixture of payload types for a event type.\n
                                   Affected event type: {typeKey}");
                        }
                    }
                }
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
            if (this.registrations.ContainsKey(key))
            {
                List<DelegateReference> removeables = new List<DelegateReference>();
                foreach (var actionReference in this.registrations[key])
                {
                    if (actionReference.Target.Equals(action))
                    {
                        removeables.Add(actionReference);
                    }                    
                }

                foreach (var reference in removeables)
                {
                    _ = this.registrations[key].Remove(reference);
                }
            }

            if (this.registrations[key].Count == 0)
            {
                _ = this.registrations.TryRemove(key, out _);
            }
        }
    }
}
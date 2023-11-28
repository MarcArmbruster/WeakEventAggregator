namespace WeakEventAggregator
{
    /// <summary>
    /// Factory for EventAggregator instances.
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
    ///   <description>11/21/2023 6:41:55 AM</description>
    ///   </item>
    ///   <item>
    ///   <term><b>Remarks:</b></term>
    ///   <description>Initial version.</description>
    ///   </item>
    ///   </list>
    /// </remarks>
    public static class Factory
    {
        /// <summary>
        /// Creates a new EventAggregator instance.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        /// <returns>The new instance.</returns>
        public static IEventAggregator GetNewEventAggregatorInstance(string name)
        {
            return new EventAggregator(name);
        }

        /// <summary>
        /// Creates a new EventAggregator instance.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        /// <param name="checkPayloadMixture">If true: payload mixture will be checked (raises an exception). False by default.</param>
        /// <returns>The new instance.</returns>
        public static IEventAggregator GetNewEventAggregatorInstance(string name, bool checkPayloadMixture)
        {
            return new EventAggregator(name, checkPayloadMixture);
        }
    }
}
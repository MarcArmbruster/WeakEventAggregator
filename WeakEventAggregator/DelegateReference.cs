namespace WeakEventAggregator
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Delegate reference instance.
    /// Inspired by the Prism implementation (https://prismlibrary.com/docs/event-aggregator.html). 
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
    ///   <description>11/27/2023 11:40:35 AM</description>
    ///   </item>
    ///   <item>
    ///   <term><b>Remarks:</b></term>
    ///   <description>Initial version.</description>
    ///   </item>
    ///   </list>
    /// </remarks>
    public class DelegateReference
    {
        private readonly Delegate theDelegateReference;

        private readonly WeakReference theWeakReferenceToTarget;

        private readonly MethodInfo theMethodInfo;

        private readonly Type theDelegateType;

        /// <summary>
        /// Gets the registered target delegate
        /// </summary>
        public Delegate Target
        {
            get
            {
                if ((object)theDelegateReference != null)
                {
                    return theDelegateReference;
                }

                return TryGetDelegate();
            }
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="targetDelegate">The target delegate.</param>
        /// <param name="keepReferenceAlive">Flag: keep reference alive.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DelegateReference(Delegate targetDelegate, bool keepReferenceAlive)
        {
            if ((object)targetDelegate == null)
            {
                throw new ArgumentNullException("targetDelegate");
            }

            if (keepReferenceAlive)
            {
                theDelegateReference = targetDelegate;
                return;
            }

            theWeakReferenceToTarget = new WeakReference(targetDelegate.Target);
            theMethodInfo = targetDelegate.GetMethodInfo();
            theDelegateType = targetDelegate.GetType();
        }

        /// <summary>
        /// Gets the delegate.
        /// </summary>
        /// <returns></returns>
        private Delegate TryGetDelegate()
        {
            if (theMethodInfo.IsStatic)
            {
                return theMethodInfo.CreateDelegate(theDelegateType, null);
            }

            object target = theWeakReferenceToTarget.Target;
            if (target != null)
            {
                return theMethodInfo.CreateDelegate(theDelegateType, target);
            }

            return null;
        }
    }
}

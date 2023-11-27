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

        public DelegateReference(Delegate @delegate, bool keepReferenceAlive)
        {
            if ((object)@delegate == null)
            {
                throw new ArgumentNullException("delegate");
            }

            if (keepReferenceAlive)
            {
                theDelegateReference = @delegate;
                return;
            }

            theWeakReferenceToTarget = new WeakReference(@delegate.Target);
            theMethodInfo = @delegate.GetMethodInfo();
            theDelegateType = @delegate.GetType();
        }

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

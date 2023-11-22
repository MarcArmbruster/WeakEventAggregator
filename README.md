# WeakEventAggregator

The WeakEventAggregatoris an easy to use EventAggregator providing following benefits
- Factory to create a named EventAggregator instance
- Subscribe events
- Publish events
- Manage event registrations (uses WeakReferences to registered delegates)
- Thread-safe

Event types are all classes that implements the (empty) IEvent interface

EvantArgs (Payloads) are all classes that implements the (empty) IPayload interface

## Example
Creating an instance
```C#
var eventAggregator = Factory.GetNewEventAggregatorInstance("myEvAgg");
```

Subscribing an event 'MsgEvent' and event args (payload) of type MsgEventArgs
```C#
eventAggregator.Subscribe<MsgEvent, MsgEventArgs>(ea => MessageBox.Show(ea.Message));
```

Raising an event
```C#
eventAggregator.Publish<MsgEvent, MsgEventArgs>(new MsgEventArgs { Message = "Test" });
```

This example uses the event
```C#
public class MsgEvent : IEvent
{
}
```

and the event args (payload)
```C#
public class MsgEventArgs : IPayload
{
    public string Message { get; set; } = string.Empty;
}
```

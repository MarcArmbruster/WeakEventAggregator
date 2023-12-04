# WeakEventAggregator

The WeakEventAggregatoris an easy to use EventAggregator providing following benefits
- Factory to create a named EventAggregator instance ('DI-ready')
- Subscribe events
- Publish events
- Manage event registrations (uses WeakReferences to registered delegates)
- Thread-safe

Event types are all classes that implements the (empty) IEvent interface

EvantArgs (Payloads) are all classes that implements the (empty) IPayload interface

## Release Notes
### Version 1.0.1
- Check to avoid payload mixtures on a defined event type added. You can configure the behavior by the boolean paramter value provided by the factory method.
- Internal event registration and event handling supports also WPF applications.

### Version 1.0.0
- initial version: base event aggregationimplementation

## Example
Creating an instance
```C#
IEventAggregator eventAggregator = Factory.GetNewEventAggregatorInstance("myEvAgg", false);
```
Using the following delegate/method
```C#
private static void ShowMessage(MsgEventArgs eventArgs)
{
    MessageBox.Show(eventArgs.Message, eventArgs.Title);
}
```

Subscribing an event 'MsgEvent' and event args (payload) of type MsgEventArgs
```C#
eventAggregator.Subscribe<MsgEvent, MsgEventArgs>(ShowMessage);
```

Raising an event
```C#
eventAggregator.Publish<MsgEvent, MsgEventArgs>(new MsgEventArgs { Message = "Test" });
```

Unsubscribing an event 'MsgEvent'
```C#
eventAggregator.Unsubscribe<MsgEvent, MsgEventArgs>(ShowMessage);
```

This example uses this event
```C#
public class MsgEvent : IEvent
{
}
```

and this event args (payload)
```C#
public class MsgEventArgs : IPayload
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

# WeakEventAggregator

The WeakEventAggregatoris an easy to use EventAggregator providing following benefits
- Factory to create a named EventAggregator instance
- Subscribe events
- Publish events
- Manage event registrations

Event types are all classes that implements the (empty) IEvent interface

EvantArgs (Payloads) are all classes that implements the (empty) IPayload interface

For a detailed description see:
See [README.md](https://github.com/MarcArmbruster/WeakEventAggregator/blob/master/README.md) file for any details about the WpfCommandAggregator.

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

# WeakEventAggregator

The WeakEventAggregatoris an easy to use EventAggregator providing following benefits
- Factory to create a named EventAggregator instance
- Subscribe events
- Publish events
- Manage event registrations (uses WeakReferences to registered delegates)
- Thread-safe

Event types are all classes that implements the (empty) IEvent interface

EvantArgs (Payloads) are all classes that implements the (empty) IPayload interface

For a detailed description see:
See [README.md](https://github.com/MarcArmbruster/WeakEventAggregator/blob/master/README.md) file for any details about the WpfCommandAggregator.

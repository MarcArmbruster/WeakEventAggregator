namespace UnitTests;

using System.Collections.Concurrent;

[TestClass]
public class EventAggregatorTests
{
    [TestMethod]
    public void MultipleRegistrationsAreIgnoredTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);
        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);
        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);

        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "Marc" });

        Assert.AreEqual(1, operations.Count);
        Assert.AreEqual("Marc", operations[0]);
    }

    [TestMethod]
    public void UnsubscribeTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);
        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "Marc" });

        Assert.AreEqual(1, operations.Count);
        Assert.AreEqual("Marc", operations[0]);
        operations.Clear();

        // unsubscribe            
        aggreagtor.Unsubscribe<MyEvent1, MyPayload1>(myDelegate);
        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "Marc" });
        Assert.AreEqual(0, operations.Count);
    }

    [TestMethod]
    public void RegistrationsTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate1 = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));
        var myDelegate2 = new Action<MyPayload2>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate1);
        aggreagtor.Subscribe<MyEvent2, MyPayload2>(myDelegate2);

        var registrations1 = aggreagtor.GetRegisteredDelegates<MyEvent1>();
        var registrations2 = aggreagtor.GetRegisteredDelegates<MyEvent2>();

        Assert.AreEqual("myAggregator", aggreagtor.Name);
        Assert.AreEqual(1, registrations1.Count());
        Assert.AreEqual(1, registrations2.Count());
    }

    [TestMethod]
    public void MultiplePublishesTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);

        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "A" });
        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "B" });
        aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = "C" });

        Assert.AreEqual(3, operations.Count);
        Assert.IsTrue(operations.Any(c => c == "A"));
        Assert.IsTrue(operations.Any(c => c == "B"));
        Assert.IsTrue(operations.Any(c => c == "C"));
    }

    [TestMethod]
    public async Task TaskTest()
    {
        ConcurrentBag<string> operations = new ConcurrentBag<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);

        var task1 = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
            }
        });

        var task2 = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
            }
        });

        var task3 = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                aggreagtor.Publish<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
            }
        });

        await Task.WhenAll(task1, task2, task3);

        Assert.AreEqual(3000, operations.Count);
        for (int i = 0; i < 1000; i++)
        {
            Assert.AreEqual(3, operations.Count(x => x == i.ToString()));
        }
    }

    [TestMethod]
    public void PublishAsyncTest()
    {
        ConcurrentBag<string> operations = new ConcurrentBag<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate);

        for (int i = 0; i < 1000; i++)
        {
            aggreagtor.PublishAsync<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
        }

        for (int i = 0; i < 1000; i++)
        {
            aggreagtor.PublishAsync<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
        }

        for (int i = 0; i < 1000; i++)
        {
            aggreagtor.PublishAsync<MyEvent1, MyPayload1>(new MyPayload1 { Content = i.ToString() });
        }

        Thread.Sleep(1000);
        Assert.AreEqual(3000, operations.Count);
        for (int i = 0; i < 1000; i++)
        {
            Assert.AreEqual(3, operations.Count(x => x == i.ToString()));
        }
    }

    [TestMethod]
    public void RemoveTest()
    {
        List<string> operations = new List<string>();
        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate1 = new Action<MyPayload1>(x => operations.Add("1"));
        var myDelegate2 = new Action<MyPayload2>(x => operations.Add("2"));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate1);
        aggreagtor.Subscribe<MyEvent2, MyPayload2>(myDelegate2);

        aggreagtor.RemoveTypeRegistrations<MyEvent1>();

        Assert.IsFalse(aggreagtor.HasRegistrations<MyEvent1>());
        Assert.IsTrue(aggreagtor.HasRegistrations<MyEvent2>());
    }

    [TestMethod]
    [ExpectedException(typeof(PayloadMixtureException))]
    public void MixtureNotAllowedTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator", true);
        var myDelegate1 = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));
        var myDelegate2 = new Action<MyPayload2>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate1);
        aggreagtor.Subscribe<MyEvent1, MyPayload2>(myDelegate2);

        Assert.AreEqual(2, aggreagtor.GetRegisteredDelegates<MyEvent1>().Count());
    }

    [TestMethod]
    public void MixtureAllowedTest()
    {
        List<string> operations = new List<string>();

        var aggreagtor = Factory.GetNewEventAggregatorInstance("myAggregator");
        var myDelegate1 = new Action<MyPayload1>(x => operations.Add(x.Content ?? string.Empty));
        var myDelegate2 = new Action<MyPayload2>(x => operations.Add(x.Content ?? string.Empty));

        aggreagtor.Subscribe<MyEvent1, MyPayload1>(myDelegate1);
        aggreagtor.Subscribe<MyEvent1, MyPayload2>(myDelegate2);
    }
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utility;
using PubNubMessaging.Core;
using System.Threading;

namespace TestCloudMessagingApps
{
  class TestObserver : IMessageProcessor
  {
    public void ProcessSentStatus(string result)
    {
      //SentStatus = 
      ev.Set();
    }

    public void ProcesssReceivedMessage(string channel, string message)
    {
      Channel = channel;
      Message = message;
      ev.Set();
    }

    public TestObserver(ManualResetEvent ev)
    {
      this.ev = ev;
    }

    public string Channel {get; set;}
    public string Message { get; set;}
    public bool SentStatus { get; set; }
    private ManualResetEvent ev;
  }

  [TestClass]
  public class TestCommunicator
  {
    private const string PUBLISHKEY = "pub-c-221f8e24-5c10-4f7d-972d-2ebded5abdff";
    private const string SUBSCRIBEKEY = "sub-c-7495c98c-de01-11e6-989b-02ee2ddab7fe";
    private Pubnub pubnub;
    private ManualResetEvent ev;
    private const int timeout = 2000;

    [TestInitialize]
    public void TestInitialize()
    {
      pubnub = new Pubnub(PUBLISHKEY, SUBSCRIBEKEY);
      ev = new ManualResetEvent(false);
    }

    [TestMethod]
    public void ListenChannel_Successful()
    {
      TestObserver ob = new TestObserver(ev);
      Communicator c = new Communicator(ob);

      string channel = "999";
      string message = "hi";

      Assert.IsTrue(c.ListenChannel(channel));

      var res = pubnub.Publish<string>(
          channel,
          message,
          DisplayReturnMessage,
          DisplayErrorMessage
          );
      res = ev.WaitOne(timeout);
      ev.Reset();
      Assert.IsTrue(res);
      Assert.AreEqual(message, ob.Message);
      Assert.AreEqual(channel, ob.Channel);
    }

    [TestMethod]
    public void SendMessage_Successful()
    {
      TestObserver ob = new TestObserver(ev);
      Communicator c = new Communicator(ob);

      string channel = "999";
      string message = "hi";

      c.SendMessage(channel,message);
      var res = ev.WaitOne(timeout);
      ev.Reset();
      Assert.IsTrue(res);
    }

    private void DisplayErrorMessage(PubnubClientError obj)
    {
      throw new NotImplementedException();
    }

    private void DisplayReturnMessage(string obj)
    {
    }
  }
}

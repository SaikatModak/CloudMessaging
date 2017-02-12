using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utility;
using PubNubMessaging.Core;
using System.Threading;

namespace TestCloudMessagingApps
{
  class TestObserver : IMessageProcessor
  {
    public void ProcessSentStatus(bool status, string channel)
    {
      Channel = channel;
      Status = status;
      evMessage.Set();
    }

    public void ProcesssReceivedMessage(string channel, string message)
    {
      Channel = channel;
      Message = message;
      evMessage.Set();
    }

    public void ProcessConnectStatus(bool status, string channel)
    {
      Status = status;
      Channel = channel;
      evConnect.Set();
    }

    public TestObserver(ManualResetEvent evConnect, ManualResetEvent evMessage)
    {
      this.evConnect = evConnect;
      this.evMessage = evMessage;
    }

    public string Channel {get; set;}
    public string Message { get; set;}
    public bool Status { get; set; }
    private ManualResetEvent evConnect;
    private ManualResetEvent evMessage;
  }

  [TestClass]
  public class TestCommunicator
  {
    private const string PUBLISHKEY = "pub-c-221f8e24-5c10-4f7d-972d-2ebded5abdff";
    private const string SUBSCRIBEKEY = "sub-c-7495c98c-de01-11e6-989b-02ee2ddab7fe";
    private Pubnub pubnub;
    private ManualResetEvent evMsg;
    private ManualResetEvent evConnect;
    private const int timeout = 5000;

    [TestInitialize]
    public void TestInitialize()
    {
      pubnub = new Pubnub(PUBLISHKEY, SUBSCRIBEKEY);
      evMsg = new ManualResetEvent(false);
      evConnect = new ManualResetEvent(false);
    }

    [TestMethod]
    public void ListenChannel_Successful()
    {
      TestObserver ob = new TestObserver(evConnect, evMsg);
      Communicator c = new Communicator(ob);

      string channel = "999";
      string message = "hi";

      c.ListenChannel(channel);

      var res = evConnect.WaitOne(timeout);
      Assert.IsTrue(res);
      Assert.IsTrue(ob.Status);
      Assert.AreEqual(channel, ob.Channel);

      res = pubnub.Publish<string>(
          channel,
          message,
          DisplayReturnMessage,
          DisplayErrorMessage
          );
      res = evMsg.WaitOne(timeout);
      evMsg.Reset();
      Assert.IsTrue(res);

      Assert.AreEqual(message, ob.Message);
      Assert.AreEqual(channel, ob.Channel);
    }

    [TestMethod]
    public void SendMessage_Successful()
    {
      TestObserver ob = new TestObserver(evConnect, evMsg);
      Communicator c = new Communicator(ob);

      string channel = "999";
      string message = "hi";

      c.SendMessage(channel,message);
      var res = evMsg.WaitOne(timeout);
      evMsg.Reset();

      Assert.IsTrue(res);
      Assert.IsTrue(ob.Status);
      Assert.AreEqual(channel, ob.Channel);
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

using PubNubMessaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utility
{
  public interface IMessageProcessor
  {
    void ProcesssReceivedMessage(string contact, string message);
    void ProcessSentStatus(string result);
  }

  public class Communicator
  {
    //Public Section
    public Communicator(IMessageProcessor listener, string pubKey = null, string subKey = null)
    {
      mListener = listener;
      mPubnub = new Pubnub(PUBLISHKEY, SUBSCRIBEKEY);
      mWaitHandle = new ManualResetEvent(false);
      if (!String.IsNullOrEmpty(subKey))
        SUBSCRIBEKEY = subKey;
      if (!String.IsNullOrEmpty(pubKey))
        PUBLISHKEY = pubKey;
    }

    public bool ListenChannel(string channel)
    {
      mPubnub.Subscribe<string>(
        channel,
        SubscribeCallback,
        ConnectCallback,
        ErrorCallback);
      bool ret = mWaitHandle.WaitOne(mTimeout);
      mWaitHandle.Reset();
      return ret;
    }

    public void SendMessage(string channel, string message)
    {
      mPubnub.Publish<string>(
          channel,
          message,
          SendCallback,
          ErrorCallback
          );
    }

    private void SendCallback(string obj)
    {
      mListener.ProcessSentStatus(obj);
    }

    private void ErrorCallback(PubnubClientError obj)
    {
      throw new NotImplementedException();
    }

    private void ConnectCallback(string obj)
    {
      mWaitHandle.Set();
    }

    //Private Section
    private void SubscribeCallback(string obj)
    {
      Console.WriteLine("SUBSCRIBE REGULAR CALLBACK:");
      Console.WriteLine(obj);
      string resultActualMessage = "";
      string resultContact = "";
      if (!string.IsNullOrEmpty(obj) && !string.IsNullOrEmpty(obj.Trim()))
      {
        List<object> deserializedMessage = mPubnub.JsonPluggableLibrary.DeserializeToListOfObject(obj);
        if (deserializedMessage != null && deserializedMessage.Count > 0)
        {
          object subscribedObject = (object)deserializedMessage[0];
          if (subscribedObject != null)
          {
            resultActualMessage = subscribedObject.ToString();
          }
          subscribedObject = (object)deserializedMessage[2];
          if (subscribedObject != null)
          {
            resultContact = subscribedObject.ToString();
          }
        }
        mListener.ProcesssReceivedMessage(resultContact, resultActualMessage);
      }
    }

    private readonly string PUBLISHKEY = "pub-c-221f8e24-5c10-4f7d-972d-2ebded5abdff";
    private readonly string SUBSCRIBEKEY = "sub-c-7495c98c-de01-11e6-989b-02ee2ddab7fe";
    private IMessageProcessor mListener;
    private Pubnub mPubnub;
    private const int mTimeout = 2000;
    private ManualResetEvent mWaitHandle;
  }
}

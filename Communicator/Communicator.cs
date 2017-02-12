using PubNubMessaging.Core;
using System;
using System.Collections.Generic;

namespace Utility
{
  public interface IMessageProcessor
  {
    void ProcessConnectStatus(bool status, string message);
    void ProcesssReceivedMessage(string contact, string message);
    void ProcessSentStatus(bool status, string message);
  }

  public class Communicator
  {
    //Public Section
    public Communicator(IMessageProcessor listener, string pubKey = null, string subKey = null)
    {
      mListener = listener;
      mPubnub = new Pubnub(PUBLISHKEY, SUBSCRIBEKEY);
      if (!String.IsNullOrEmpty(subKey))
        SUBSCRIBEKEY = subKey;
      if (!String.IsNullOrEmpty(pubKey))
        PUBLISHKEY = pubKey;
    }

    public void ListenChannel(string channel)
    {
      mPubnub.Subscribe<string>(
        channel,
        SubscribeCallback,
        ConnectCallback,
        ErrorCallbackSubscribe);
    }

    public void SendMessage(string channel, string message)
    {
      mPubnub.Publish<string>(
          channel,
          message,
          SendCallback,
          ErrorCallbackPublish
          );
    }

    //Private Section
    private void SendCallback(string obj)
    {
      var msgs = ParseJson(obj);
      bool bStatus = false;
      string channel = "";
      if (msgs.Count == 4)
      {
        if (msgs[1].Equals("Sent"))
        {
          bStatus = true;
          channel = msgs[3];
        }
      }
      mListener.ProcessSentStatus(bStatus, channel);
    }

    private void ErrorCallbackSubscribe(PubnubClientError obj)
    {
      mListener.ProcessConnectStatus(false, obj.Description);
    }

    private void ErrorCallbackPublish(PubnubClientError obj)
    {
      mListener.ProcessSentStatus(false, obj.Description);
    }

    private void ConnectCallback(string obj)
    {
      var msgs = ParseJson(obj);
      bool bStatus = false;
      string channel = "";
      if (msgs.Count == 3)
      {
        if (msgs[1].Equals("Connected"))
        {
          bStatus = true;
          channel = msgs[2];
        }
      }
      mListener.ProcessConnectStatus(bStatus, channel);
    }

    private void SubscribeCallback(string obj)
    {
      var msgs = ParseJson(obj);
      string msg = "";
      string channel = "";
      if (msgs.Count == 3)
      {
        msg = msgs[0];
        channel = msgs[2];
        mListener.ProcesssReceivedMessage(channel, msg);
      }
    }

    private List<string> ParseJson(string msg)
    {
      List<string> messages = new List<string>();
      if(!string.IsNullOrEmpty(msg) && !string.IsNullOrEmpty(msg.Trim()))
      {
        List<object> deserializedMessage = mPubnub.JsonPluggableLibrary.DeserializeToListOfObject(msg);
        if (deserializedMessage != null && deserializedMessage.Count > 0)
        {
          deserializedMessage.ForEach(x => messages.Add(x.ToString()));
        }
      }
      return messages;
    }

    private readonly string PUBLISHKEY = "pub-c-221f8e24-5c10-4f7d-972d-2ebded5abdff";
    private readonly string SUBSCRIBEKEY = "sub-c-7495c98c-de01-11e6-989b-02ee2ddab7fe";
    private IMessageProcessor mListener;
    private Pubnub mPubnub;
    private const int mTimeout = 2000;
  }
}

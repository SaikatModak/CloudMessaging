using Newtonsoft.Json;
using PubNubMessaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
  interface IMessageProcessor
  {
    void ProcesssReceivedMessage(string contact, string message);
    void ProcessSentStatus(string result);
  }

  class Communicator
  {
    public const string PUBLISHKEY = "pub-c-221f8e24-5c10-4f7d-972d-2ebded5abdff";
    public const string SUBSCRIBEKEY = "sub-c-7495c98c-de01-11e6-989b-02ee2ddab7fe";
    public Communicator(IMessageProcessor listener)
    {
      m_listener = listener;

      m_pubnub = new Pubnub(PUBLISHKEY, SUBSCRIBEKEY);
    }

    public void ListenContact(string contact)
    {
      m_pubnub.Subscribe<string>(
        contact,
        DisplaySubscribeReturnMessage,
        DisplaySubscribeConnectStatusMessage,
        DisplayErrorMessage);
    }

    public void SendMessage(string contact, string message)
    {
      //string msg = @"{""text"":""" + message + @"""}";
      //dynamic obj = new System.Dynamic.ExpandoObject();
      //obj.text = message;
      //string msg = JsonConvert.SerializeObject(obj);
      //var obj = JsonConvert.DeserializeObject(msg);
      //var f = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
      m_pubnub.Publish<string>(
          contact,
          message,
          DisplayReturnMessage,
          DisplayErrorMessage
          );
    }

    private void DisplayReturnMessage(string obj)
    {
      m_listener.ProcessSentStatus(obj);
    }

    private void DisplayErrorMessage(PubnubClientError obj)
    {
      //throw new NotImplementedException();
    }

    private void DisplaySubscribeConnectStatusMessage(string obj)
    {
      //throw new NotImplementedException();
    }

    private void DisplaySubscribeReturnMessage(string obj)
    {
      Console.WriteLine("SUBSCRIBE REGULAR CALLBACK:");
      Console.WriteLine(obj);
      string resultActualMessage = "";
      string resultContact = "";
      if (!string.IsNullOrEmpty(obj) && !string.IsNullOrEmpty(obj.Trim()))
      {
        List<object> deserializedMessage = m_pubnub.JsonPluggableLibrary.DeserializeToListOfObject(obj);
        if (deserializedMessage != null && deserializedMessage.Count > 0)
        {
          object subscribedObject = (object)deserializedMessage[0];
          if (subscribedObject != null)
          {
            //IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
            //var tmp = m_pubnub.JsonPluggableLibrary.SerializeToJsonString(subscribedObject);
            //dynamic dyn = JsonConvert.DeserializeObject(tmp);
            //resultActualMessage = dyn.text;
            resultActualMessage = subscribedObject.ToString();
          }
          subscribedObject = (object)deserializedMessage[2];
          if (subscribedObject != null)
          {
            //IF CUSTOM OBJECT IS EXCEPTED, YOU CAN CAST THIS OBJECT TO YOUR CUSTOM CLASS TYPE
            resultContact = subscribedObject.ToString();
          }
        }
        m_listener.ProcesssReceivedMessage(resultContact, resultActualMessage);
      }
    }

    private IMessageProcessor m_listener;
    private Pubnub m_pubnub;
  }
}

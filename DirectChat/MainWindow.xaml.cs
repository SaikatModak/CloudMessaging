using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirectChat
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IMessageProcessor
  {
    public MainWindow()
    {
      InitializeComponent();
      m_handler = new Communicator(this);
    }
    private Communicator m_handler;

    public void ProcesssReceivedMessage(string contact, string message)
    {
      Dispatcher.Invoke((Action)delegate () { UpdateChatBox(message); });
    }

    private void UpdateChatBox(string message)
    {
      txtChatBox.Text += message + "\n";
    }

    public void ProcessSentStatus(string status)
    {
      Dispatcher.Invoke((Action)delegate ()
      {
        UpdateChatBox("Me:" + txtMessage.Text);
        txtMessage.Text = "";
      });
    }

    private void StartChat(object sender, RoutedEventArgs e)
    {
      m_handler.ListenContact(txtSelfContact.Text);
    }

    private void SendMessage(object sender, RoutedEventArgs e)
    {
      m_handler.SendMessage(txtContact.Text, txtSelfContact.Text + ":" + txtMessage.Text);
    }
  }
}

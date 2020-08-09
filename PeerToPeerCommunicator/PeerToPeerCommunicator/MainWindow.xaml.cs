using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessagingService;
using Microsoft.Win32;


namespace PeerToPeerCommunicator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MessageService myMessageService = new MessageService();

        public MainWindow()
        {
            InitializeComponent();
            TxtMessagePanel.Text = String.Empty;
            myMessageService.MessageReceived += OnMessageReceived;
            BtnSend.IsEnabled = false;
        }

        private void OnMessageReceived(object sender, Message e)
        {
            AddReceivingMessageToMessagePanel(e);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            myMessageService.StartListening(Convert.ToInt32(TxtLocalPort.Text));
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtLocalPort.IsEnabled = false;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            myMessageService.StopListening();
            BtnStop.IsEnabled = false;
            BtnStart.IsEnabled = true;
            TxtLocalPort.IsEnabled = true;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void AddReceivingMessageToMessagePanel(Message message)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                string receivedFileMessage = String.Empty;
                if (!string.IsNullOrEmpty(message.FileName))
                {
                    receivedFileMessage = $"\n You received new file: {message.FileName}";
                }

                TxtMessagePanel.Text += $" [{message.SendingTime}] \n {message.SenderName}:  {message.MessageText} {receivedFileMessage}\n\n";
            }));
        }

        private void AddSendingMessageToMessagePanel(Message message)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                string receivedFileMessage = String.Empty;
                if (!string.IsNullOrEmpty(message.FileName))
                {
                    receivedFileMessage = $"\n You sent file: {message.FileName}";
                }

                TxtMessagePanel.Text += $" [{message.SendingTime}] \n Me:  {message.MessageText} {receivedFileMessage}\n\n";
            }));
        }

        private void TxtNewMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(TxtNewMessage.Text) && string.IsNullOrWhiteSpace(TxtPathToFile.Text))
            {
                return;
            }

            Message newMessage = MessageService.CreateNewMessage(TxtUserName.Text, TxtNewMessage.Text, TxtPathToFile.Text);

            if (!myMessageService.SendMessage(newMessage, TxtRemoteIPAddress.Text, Convert.ToInt32(TxtRemotePort.Text)))
            {
                MessageBox.Show("Receiver is not available.\nThe message was not sent!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AddSendingMessageToMessagePanel(newMessage);
                TxtNewMessage.Text = String.Empty;
                TxtPathToFile.Text = String.Empty;
            }
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            myMessageService.StopListening();
        }

        private void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                TxtPathToFile.Text = openFileDialog.FileName;
            }
        }

        private void TxtNewMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetEnabledStateOfBtnSend();
        }

        private void TxtPathToFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetEnabledStateOfBtnSend();
        }

        private void SetEnabledStateOfBtnSend()
        {
            BtnSend.IsEnabled = !string.IsNullOrWhiteSpace(TxtNewMessage.Text) || !string.IsNullOrWhiteSpace(TxtPathToFile.Text);
        }
    }
}

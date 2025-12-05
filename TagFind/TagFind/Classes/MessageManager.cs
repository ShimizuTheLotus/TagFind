using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagFind.Interfaces;

namespace TagFind.Classes
{
    public class MessageManager
    {
        public MessageManager()
        {

        }

        ~MessageManager()
        {

        }

        public void PushMessage(MessageType messageType, string content)
        {
            //App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (App.Current.MainWindow is IMessagePushable messagePushable)
            //    {
            //        messagePushable.PushMessage(messageType, content);
            //        MessageBox.Show(content);
            //    }
            //}));
        }
    }
    public enum MessageType
    {
        Info,
        Warning,
        Error
    }
}

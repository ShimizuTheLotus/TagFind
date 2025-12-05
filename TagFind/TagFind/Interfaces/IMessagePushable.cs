using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes;

namespace TagFind.Interfaces
{
    public interface IMessagePushable
    {
        public void PushMessage(MessageType messageType, string content);
        public void ClearMessage();
        public void DeleteMessage(int messageId);
    }
}

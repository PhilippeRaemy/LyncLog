namespace LyncLog
{
    using System;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using Microsoft.Lync.Model.Conversation;

    class InstantMessage : ConversationItem
    {
        string _messageText;
        protected Participant Participant { private get; set; }

        protected string MessageText
        {
            private get
            {
                var rtf = new RichTextBox();
                try
                {
                    rtf.Rtf = _messageText;
                    return rtf.Text;
                }
                catch
                {
                    return _messageText;
                }
            }
            set { _messageText = value; }
        }

        public InstantMessage(Conversation conversation, XElement xel) : base(conversation, xel)
        {
            Participant=new Participant(conversation, xel, GetAttributeValue(".", "displayName"));
            MessageText = GetText("messageInfo");
        }

        public override string ToString()
        {
            return $"{Environment.NewLine}{Participant.ToString().Replace(".", "").Replace(",", "")} said:{Environment.NewLine}{MessageText}";
        }

        public override string ToShortString()
        {
            return string.IsNullOrWhiteSpace(MessageText) 
                ? null
                : $"{Environment.NewLine}{ItemTimeStamp:yyyyMMdd-HH:mm:ss} {Participant.ToShortString().Replace(".", "").Replace(",", "")}:{Environment.NewLine}{MessageText}";
        }
    }
}
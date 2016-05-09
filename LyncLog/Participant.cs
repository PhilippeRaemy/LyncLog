namespace LyncLog
{
    using System.Xml.Linq;
    using Microsoft.Lync.Model.Conversation;

    class Participant : ConversationItem
    {
        string DisplayName { get; }

        public Participant(Conversation conversation, XElement xel, string displayName) : base(conversation, xel)
        {
            DisplayName = displayName;
        }
        public Participant(Conversation conversation, XElement xel) : base(conversation, xel)
        {
            DisplayName = GetAttributeValue("participant", "displayName");
        }
        public override string ToString()
        {
            return $"{base.ToString()}, {DisplayName}.";
        }

        public override string ToShortString()
        {
            return DisplayName??string.Empty;
        }
    }
}
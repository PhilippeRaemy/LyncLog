namespace LyncLog
{
    using System.Xml.Linq;
    using Microsoft.Lync.Model.Conversation;

    class SessionInvite : InstantMessage
    {
        public SessionInvite(Conversation conversation, XElement xel) : base(conversation, xel)
        {
            Participant = new Participant(conversation, xel, GetAttributeValue("remoteParticipant/deviceInfo", "displayString"));
            MessageText = GetText("inviteMessage");
        }
    }
}
namespace LyncLog
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.Lync.Model.Conversation;
    using MoreLinq;

    abstract class ConversationItem
    {
        Conversation Conversation { get; }
        static readonly XmlNamespaceManager NsManager;

        static ConversationItem()
        {
            NsManager = new XmlNamespaceManager(new NameTable());
            NsManager.AddNamespace("ns", "http://schemas.microsoft.com/2008/10/sip/convItems");
        }

        readonly XElement _xel;
        public DateTime ItemTimeStamp { get; }

        protected ConversationItem(Conversation conversation, XElement xel)
        {
            Conversation = conversation;
            _xel = xel;
            ItemTimeStamp = xel.Attributes().Where(a => a.Name == "ts").Select(a => DateTime.Parse(a.Value)).FirstOrDefault();
        }

        string Pathify(string nodeName)
        {
            return nodeName == "."
                ? nodeName
                : nodeName.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => "ns:" + n)
                    .ToDelimitedString("/");
        }
        protected string GetAttributeValue(string nodeName, string attributeName)
            => _xel.XPathSelectElement(Pathify(nodeName), NsManager)
                ?.Attributes()
                .Where(a => a.Name == attributeName)
                .Select(a => a.Value)
                .FirstOrDefault();

        protected string GetText(string nodeName)
            => _xel.XPathSelectElement(Pathify(nodeName), NsManager)?.Value;

        public override string ToString()
        {
            return $"At {ItemTimeStamp}, in conversation \"{Conversation.Properties[ConversationProperty.Subject]}\"";
        }

        public abstract string ToShortString();
    }
}
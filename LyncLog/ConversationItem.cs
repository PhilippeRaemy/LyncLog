#region License, Terms and Author(s)
//
// Lynclog, raw logging for Lync and Skype for business conversations
// Copyright (c) 2016 Philippe Raemy. All rights reserved.
//
//  Author(s):
//
//      Philippe Raemy
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

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
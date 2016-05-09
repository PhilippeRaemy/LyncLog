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
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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Lync.Model.Conversation;
    using MoreLinq;

    class ConversationContainer
    {
        public Conversation Conversation     { get; set; }
        public DateTime ConversationCreated  { get; set; }
        DateTime ConversationLastTime { get; set; }

        FileInfo _logFileInfo;
        FileInfo _nextLogFileInfo;

        void PropertiesChanged()
        {
            var path = Path.Combine(ConfigurationManager.AppSettings["ConversationLog"],
                new[]
                    {
                        $"{ConversationCreated:yyyyMMdd-HHmm}",
                        $"{ConversationLastTime:yyyyMMdd-HHmm}",
                        $@"{Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars())
                                .Aggregate(Conversation.Properties[ConversationProperty.Subject].ToString(), (n, c)=>n.Replace(c, '_'))}"
                    }.Concat(Conversation.ParticipantNames())
                .ToDelimitedString("-")
            );

            _nextLogFileInfo = new FileInfo(path + ".txt");
        }

        void CommitFileName()
        {
            if (_logFileInfo != null)
            {
                if (string.Equals(_nextLogFileInfo.FullName, _logFileInfo.FullName,
                    StringComparison.InvariantCultureIgnoreCase)) return;
                if(_logFileInfo.Exists) File.Move(_logFileInfo.FullName, _nextLogFileInfo.FullName);
                Trace.WriteLine($"{_logFileInfo.FullName} has been renamed {_nextLogFileInfo.FullName}");
            }
            _logFileInfo = _nextLogFileInfo;
            _logFileInfo.Directory?.Create();
        }

        public void DumpConversation()
        {
            DumpConversation(DateTime.MinValue);
        }

        public void DumpConversation(DateTime fromDateTime)
        {
            var dump = DumpConversationImpl(fromDateTime)
                .Pipe(d=> Trace.TraceInformation(d.ToString()))
                .ToArray();
            CommitFileName();
            using (var sr = File.AppendText(_logFileInfo.FullName))
            {
                // sr.Write(_logFileInfo.Name);
                // ReSharper disable once AccessToDisposedClosure
                dump.Select(d=>d.ToShortString())
                    .Where(d=>!string.IsNullOrWhiteSpace(d))
                    .Pipe(Console.WriteLine)
                    .ForEach(d=>sr.WriteLine(d));
            }
        }

        IEnumerable<ConversationItem> DumpConversationImpl(DateTime fromDateTime)
        {
            foreach (var xElement in Conversation.History())
            {
                ConversationItem conversationItem = null;
                switch (xElement.Name.LocalName)
                {
                    case "participantAdded":
                        var participant = conversationItem = new Participant(Conversation, xElement);
                        if (participant.ItemTimeStamp >= fromDateTime)
                        {
                            yield return participant;
                            PropertiesChanged();
                        }
                        break;
                    case "sessionInvite":
                        var invite = conversationItem = new SessionInvite(Conversation, xElement);
                        if (invite.ItemTimeStamp >= fromDateTime)
                            yield return invite;
                        break;
                    case "imReceived":
                        var im = conversationItem = new InstantMessage(Conversation, xElement);
                        if (im.ItemTimeStamp >= fromDateTime)
                            yield return im;
                        break;
                }
                if (conversationItem != null)
                {
                    if (ConversationCreated > conversationItem.ItemTimeStamp)
                    {
                        ConversationCreated = conversationItem.ItemTimeStamp;
                        PropertiesChanged();
                    }
                    if (ConversationLastTime < conversationItem.ItemTimeStamp)
                    {
                        ConversationLastTime = conversationItem.ItemTimeStamp;
                        PropertiesChanged();
                    }
                }
            }
        }
    }

    public static class Extentions
    {
        public static IEnumerable<XElement> History(this Conversation conversation)
        {
            dynamic history;
            try
            {
                history = ((dynamic)conversation.InnerObject).History.CurrentHistory;
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }
            foreach (string text in history)
            {
                yield return XElement.Parse(text);
            }
        }

        public static IEnumerable<string> ParticipantNames(this Conversation conversation)
            => from   p    in conversation.Participants
               from   prop in p.Properties
               where  prop.Key.ToString() == "Name"
               select prop.Value.ToString();
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MargieBot.ExampleResponseProcessors.Models;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.ExampleResponseProcessors.ResponseProcessors
{
    public class ScoreboardRequestResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, @"\bscore\b", RegexOptions.IgnoreCase);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            IReadOnlyDictionary<string, int> scores = context.Get<Scorebook>().GetScores();

            if (scores.Count > 0) {
                StringBuilder builder = new StringBuilder(context.Get<Phrasebook>().GetScoreboardHype());
                builder.Append("\n```");

                // add the scores to a list for sorting. while we do, figure out who has the longest name for the pseudo table formatting
                List<KeyValuePair<string, int>> sortedScores = new List<KeyValuePair<string, int>>();
                string longestName = string.Empty;

                foreach (string key in scores.Keys) {
                    KeyValuePair<string, int> newScore = new KeyValuePair<string, int>(context.UserNameCache[key], scores[key]);
                    
                    if(newScore.Key.Length > longestName.Length) {
                        longestName = newScore.Key;
                    }

                    sortedScores.Add(newScore);
                }
                sortedScores.Sort((x, y) => { return y.Value.CompareTo(x.Value); });

                foreach(KeyValuePair<string, int> userScore in sortedScores)  {
                    StringBuilder nameString = new StringBuilder(userScore.Key);
                    while(nameString.Length < longestName.Length) {
                        nameString.Append(" ");
                    }

                    builder.Append(nameString.ToString() + " | " + userScore.Value.ToString() + "\n");
                }

                DateTime lastDayOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                DateTime firstOfNextMonth = lastDayOfThisMonth.AddDays(1);
                builder.Append("```\n");

                builder.Append("Better git on it. Imma reset the scoreboard in " + (firstOfNextMonth - DateTime.Now).Days + " days!");

                return new BotMessage() {
                    Text = builder.ToString()
                };
            }
            else { return new BotMessage() { Text = "Not a one-of-ya has scored yet. Come on, sleepyheads!" }; }
        }
    }
}
using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordUtils
{
    public static class Utils
    {
        /// <summary>
        /// Check if an URL is valid or not
        /// </summary>
        /// <param name="url">The URL to check</param>
        public static bool IsLinkValid(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Method = "HEAD";
                    request.GetResponse();
                    return (true);
                }
                catch (WebException)
                {
                    return (false);
                }
            }
            return (false);
        }

        /// <summary>
        /// Get a user by his username/nickname/id
        /// </summary>
        /// <param name="name">The name/id of the user</param>
        /// <param name="guild">The guild the user is in</param>
        /// <returns></returns>
        public static async Task<IGuildUser> GetUser(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<@[!]?[0-9]{18}>");
            if (match.Success)
            {
                if (ulong.TryParse(string.Concat(match.Value.Where(x => char.IsNumber(x))), out ulong id))
                {
                    IGuildUser user = await guild.GetUserAsync(id);
                    if (user != null)
                        return (user);
                }
            }
            if (ulong.TryParse(name, out ulong id2))
            {
                IGuildUser user = await guild.GetUserAsync(id2);
                if (user != null)
                    return (user);
            }
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname == name || user.Username == name)
                    return (user);
            }
            return (null);
        }

        /// <summary>
        /// Return a string given a TimeSpan
        /// </summary>
        /// <param name="ts">The TimeSpan to transform</param>
        /// <returns>The string wanted</returns>
        public static string TimeSpanToString(TimeSpan ts)
        {
            string finalStr = ts.Seconds.ToString() + " seconds";
            if (ts.Days > 0)
                finalStr = ts.Days.ToString() + " days, " + ts.Hours.ToString() + " hours, " + ts.Minutes.ToString() + " minutes and " + finalStr;
            else if (ts.Hours > 0)
                finalStr = ts.Hours.ToString() + " hours, " + ts.Minutes.ToString() + " minutes and " + finalStr;
            else if (ts.Minutes > 0)
                finalStr = ts.Minutes.ToString() + " minutes and " + finalStr;
            return (finalStr);
        }

        /// <summary>
        /// Get basic informations about the bot
        /// </summary>
        /// <param name="startTime">Time when the bot was launched</param>
        /// <param name="botName">Name of the bot (GitHub repository)</param>
        /// <param name="me">Bot user</param>
        /// <returns>Embed containing bot info</returns>
        public static Embed GetBotInfo(DateTime startTime, string botName, SocketSelfUser me)
        {
            return (new EmbedBuilder()
            {
                Color = Color.Purple,
                Fields = new System.Collections.Generic.List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Uptime",
                        Value = TimeSpanToString(DateTime.Now.Subtract(startTime))
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Creator",
                        Value = "Zirk#0001"
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Account creation",
                        Value = me.CreatedAt.ToString("HH:mm:ss dd:MM:yy")
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Last version",
                        Value = new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString("HH:mm:ss dd:MM:yy")
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "GitHub",
                        Value = "https://github.com/Xwilarg/" + botName
                    }
                }
            }.Build());
        }

        /// <summary>
        /// Check if file extension is the one of an image displayable per Discord
        /// </summary>
        public static bool IsImage(string extension)
        {
            return (extension.StartsWith("gif") || extension.StartsWith("png") || extension.StartsWith("jpg")
                || extension.StartsWith("jpeg"));
        }
    }
}

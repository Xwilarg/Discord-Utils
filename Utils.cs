using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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

        public static IRole GetRole(string name, IGuild guild)
        {
            if (ulong.TryParse(name, out ulong id2))
            {
                IRole role = guild.GetRole(id2);
                if (role != null)
                    return (role);
            }
            string lowerName = CleanWord(name);
            foreach (IRole role in guild.Roles)
            {
                if (CleanWord(role.Name) == lowerName)
                    return (role);
            }
            return (null);
        }

        public static string CleanWord(string word)
        {
            StringBuilder finalStr = new StringBuilder();
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                    finalStr.Append(char.ToLower(c));
            }
            return (finalStr.ToString());
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
                        Value = me.CreatedAt.ToString("HH:mm:ss dd/MM/yy")
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Last version",
                        Value = new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString("HH:mm:ss dd/MM/yy")
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

        /// <summary>
        /// Display a message in chanel when an exception occured
        /// Callback from BaseDiscordClient.Lot
        /// </summary>
        public static Task LogError(LogMessage msg)
        {
            Log(msg);
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = msg.Exception.InnerException.GetType().ToString(),
                    Description = "An error occured while executing last command.\nHere are some details about it: " + msg.Exception.InnerException.Message
                }.Build());
            }
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Display a message in the console
        /// Callback from CommandService.Log
        /// </summary>
        public static Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine(msg);
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Escape dangerous characters in string
        /// </summary>
        public static string EscapeString(string msg)
        {
            return (msg.Replace("\\", "\\\\").Replace("\"", "\\\""));
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        public static async Task<bool> IsLinkValid(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Add("User-Agent", "Sanara");
                    var response = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    return response.IsSuccessStatusCode;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a user by his username/nickname/id
        /// </summary>
        /// <param name="name">The name/id of the user</param>
        /// <param name="guild">The guild the user is in</param>
        /// <returns></returns>
        public static async Task<IGuildUser> GetUser(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<@[!]?([0-9]{18})>");
            if (match.Success)
            {
                IGuildUser user = await guild.GetUserAsync(ulong.Parse(match.Groups[1].Value));
                if (user != null)
                    return (user);
            }
            if (ulong.TryParse(name, out ulong id2))
            {
                IGuildUser user = await guild.GetUserAsync(id2);
                if (user != null)
                    return (user);
            }
            name = name.ToLower();
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname?.ToLower() == name || user.Username.ToLower() == name || user.ToString().ToLower() == name)
                    return (user);
            }
            return (null);
        }

        public static IRole GetRole(string name, IGuild guild)
        {
            if (guild == null)
                return (null);
            Match match = Regex.Match(name, "<@&([0-9]{18})>");
            if (match.Success)
            {
                IRole role = guild.GetRole(ulong.Parse(match.Groups[1].Value));
                if (role != null)
                    return (role);
            }
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

        public static async Task<ITextChannel> GetTextChannel(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<#([0-9]{18})>");
            if (match.Success)
            {
                ITextChannel chan = await guild.GetTextChannelAsync(ulong.Parse(match.Groups[1].Value));
                if (chan != null)
                    return (chan);
            }
            if (ulong.TryParse(name, out ulong id2))
            {
                ITextChannel chan = await guild.GetTextChannelAsync(id2);
                if (chan != null)
                    return (chan);
            }
            foreach (ITextChannel chan in await guild.GetTextChannelsAsync())
            {
                if (chan.Name == name)
                    return (chan);
            }
            return (null);
        }

        public static async Task<IMessage> GetMessage(string id, IMessageChannel chan)
        {
            ulong uid;
            if (!ulong.TryParse(id, out uid))
                return null;
            IMessage msg;
            if (uid != 0)
            {
                msg = await chan.GetMessageAsync(uid);
                if (msg != null)
                    return msg;
            }
            ITextChannel textChan = chan as ITextChannel;
            if (textChan == null || uid == 0)
                return null;
            foreach (ITextChannel c in await textChan.Guild.GetTextChannelsAsync())
            {
                try
                {
                    msg = await c.GetMessageAsync(uid);
                    if (msg != null)
                        return msg;
                }
                catch (Discord.Net.HttpException)
                { }
            }
            return null;
        }

        public static string CleanWord(string word)
        {
            StringBuilder finalStr = new StringBuilder();
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                    finalStr.Append(char.ToLower(c));
            }
            var result = finalStr.ToString();
            return result.Length == 0 ? word : result;
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
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Uptime",
                Value = TimeSpanToString(DateTime.Now.Subtract(startTime))
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Creator",
                Value = "Zirk#0001"
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Account creation",
                Value = me.CreatedAt.ToString("HH:mm:ss dd/MM/yy")
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Last version",
                Value = new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString("HH:mm:ss dd/MM/yy")
            });
            if (botName != null)
                fields.Add(new EmbedFieldBuilder()
                {
                    Name = "GitHub",
                    Value = "https://github.com/Xwilarg/" + botName
                });
            return (new EmbedBuilder()
            {
                Color = Color.Purple,
                Fields = fields
            }.Build());
        }

        /// <summary>
        /// Check if file extension is the one of an image displayable per Discord
        /// </summary>
        public static bool IsImage(string extension)
        {
            extension = extension.ToLowerInvariant();
            if (extension[0] == '.') extension = extension[1..];
            return (extension.StartsWith("gif") || extension.StartsWith("png") || extension.StartsWith("jpg")
                || extension.StartsWith("jpeg"));
        }

        public static string GetExtension(string file)
            => Path.GetExtension(file);

        /// <summary>
        /// Display a message in chanel when an exception occured
        /// Callback from BaseDiscordClient.Lot
        /// </summary>
        public static async Task LogErrorAsync(LogMessage msg)
        {
            await Log(msg);
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                await ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = msg.Exception.InnerException.GetType().ToString(),
                    Description = "An error occured while executing last command.\nHere are some details about it: " + msg.Exception.InnerException.Message
                }.Build());
            }
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
            return msg.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// Clean HTML by removing useless balises and formatting the msg for Discord
        /// </summary>
        public static string CleanHtml(string msg)
        {
            msg = Regex.Replace(msg, "<br *\\/>", "\n");
            msg = Regex.Replace(msg, "<\\/br>", "\n");
            msg = Regex.Replace(msg, "<b>([^<]+)<\\/b>", "**$1**");
            msg = Regex.Replace(msg, "<strong>([^<]+)<\\/strong>", "**$1**");
            msg = Regex.Replace(msg, "<a href=\"([^\"]+)\">([^<]+)<\\/a>", "[$2]($1)");
            msg = Regex.Replace(msg, "<[^>]+>([^<]+)<\\/[^>]+>", "$1");
            msg = Regex.Replace(msg, "<\\/?[^>]+>", "");
            return msg;
        }

        /// <summary>
        /// Send informations to website
        /// </summary>
        /// <param name="name">Name of the bot</param>
        /// <param name="website">Website to send information about</param>
        /// <param name="token">Authentification token</param>
        /// <param name="elem1">Element to send (name)</param>
        /// <param name="elem2">Element to send (value)</param>
        public static async Task WebsiteUpdate(string name, string website, string token, string elem1, string elem2)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string> {
                { "token", token },
                { "name", name },
                { elem1, elem2 }
            };
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, website);
            msg.Content = new FormUrlEncodedContent(values);
            try
            {
                await httpClient.SendAsync(msg);
            }
            catch (HttpRequestException)
            { }
            catch (TaskCanceledException)
            { }
        }

        /// <summary>
        /// FeatureRequest is a struct returned by functions.
        /// Mostly used to abstract commands to separate modules that interract with Discord and function that return features
        /// T is a struct containing the answer of the function
        /// U is an enum containing if an error occured or not
        /// </summary>
        public struct FeatureRequest<T, U>
        where U : Enum
        {
            public FeatureRequest(T answer, U error)
            {
                this.answer = answer;
                this.error = error;
            }

            public T answer;
            public U error;
        }

        /// <summary>
        /// Init translations
        /// </summary>
        /// <param name="translations">key is language name (en, fr, etc...), value is translations key/value</param>
        /// <param name="translationKeyAlternate">Contains alternative name for a translation language, for example for 'fr' you will have 'french' and 'fran�ais'</param>
        /// <param name="translationFolder">The folder containing all translations</param>
        public static void InitTranslations(Dictionary<string, Dictionary<string, string>> translations,
            Dictionary<string, List<string>> translationKeyAlternate,
            string translationFolder)
        {
            if (!Directory.Exists("Translations"))
                Directory.CreateDirectory("Translations");
            if (Directory.Exists(translationFolder))
            {
                foreach (string dir in Directory.GetDirectories(translationFolder))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        FileInfo fi = new FileInfo(file);
                        File.Copy(file, "Translations/" + di.Name + "-" + fi.Name, true);
                    }
                }
            }
            foreach (string file in Directory.GetFiles("Translations"))
            {
                FileInfo fi = new FileInfo(file);
                Match match = Regex.Match(fi.Name, "([a-z]+)-(infos|terms).json");
                if (match.Groups.Count < 3)
                    continue;
                string key = match.Groups[1].Value;
                if (match.Groups[2].Value == "infos")
                {
                    dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
                    translationKeyAlternate.Add(key, new List<string>()
                    {
                        json.nameEnglish.ToString(),
                        json.nameLanguage.ToString()
                    });
                }
                else
                {
                    translations.Add(key, new Dictionary<string, string>());
                    foreach (Match m in Regex.Matches(File.ReadAllText(file), "\"([a-zA-Z0-9]+)\" ?: ?\"([^\"]+)\""))
                        translations[key].Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }
        }

        /// <summary>
        /// Get sentence in guild language, if can't then fallback on english
        /// </summary>
        /// <param name="translations">Dictionnary containing all sentences</param>
        /// <param name="guildLanguage">Language to search in</param>
        /// <param name="word">Translation key</param>
        /// <param name="args">Optionnal argument for translation</param>
        /// <returns></returns>
        public static string Translate(Dictionary<string, Dictionary<string, string>> translations,
            string guildLanguage, string word, params string[] args)
        {
            string sentence;
            if (guildLanguage != null && translations[guildLanguage].ContainsKey(word))
                sentence = translations[guildLanguage][word];
            else if (translations["en"].ContainsKey(word))
                sentence = translations["en"][word];
            else
                return (Translate(translations, guildLanguage != null ? guildLanguage : "en", "invalidKey", word));
            sentence = sentence.Replace("\\n", "\n");
            for (int i = 0; i < args.Length; i++)
                sentence = sentence.Replace("{" + i + "}", args[i]);
            return (sentence);
        }

        public static bool CanModify(IUser user, ulong ownerId)
        {
            if (user.Id == ownerId)
                return true;
            IGuildUser guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        public static string ToWordCase(string word)
            => char.ToUpper(word[0]) + string.Join("", word.Skip(1)).ToLower();

        public static bool CanSendNsfw(IMessageChannel chan)
            => !(chan is ITextChannel) || ((ITextChannel)chan).IsNsfw;

        /// <summary>
        /// Greatest Common Divisor
        /// </summary>
        public static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a == 0 ? b : a;
        }

        /// <summary>
        /// 10000000 -> 10 000k
        /// </summary>
        public static string MakeNumberReadable(string str)
        {
            if (str.Length < 4)
                return str;
            str = str[0..^3];
            StringBuilder finalStr = new StringBuilder();
            str = string.Join("", str.Reverse());
            while (str.Length > 3)
            {
                finalStr.Append(str[0..3] + " ");
                str = str.Substring(3);
            }
            if (str.Length == 0)
                finalStr.Remove(0, finalStr.Length - 1);
            else
                finalStr.Append(str);
            return string.Join("", finalStr.ToString().Reverse()) + "k";
        }
    }
}

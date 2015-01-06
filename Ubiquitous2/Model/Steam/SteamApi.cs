using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
using System.Threading;
using UB.Model;

namespace UB.SteamApi
{
    /// <summary>
    /// Class allowing you to use the Steam Web API to log in and use Steam Friends functionality.
    /// </summary>
    /// 
    public class SteamAPISession
    {
        public String accessToken;
        private String umqid;
        private String steamid;
        private int message = 0;
        private object loginLock = new object();
        public LoginStatus loginStatus = LoginStatus.LoginFailed;
        public event EventHandler<SteamEvent> Logon;
        public event EventHandler<SteamEvent> NewMessage;
        public event EventHandler<SteamEvent> Typing;
        public event EventHandler<SteamEvent> FriendStateChange;
        public event EventHandler<SteamEvent> SteamGuard;

        #region "Events"
        public class SteamEvent : EventArgs
        {
            public SteamEvent(Update u = null)
            {
                update = u;
            }
            public Update update;
        }
        private void DefaultEvent(EventHandler<SteamEvent> steamEvent, SteamEvent e)
        {
            EventHandler<SteamEvent> handler = steamEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnNewMessage(SteamEvent e)
        {
            DefaultEvent(NewMessage, e);
        }
        protected virtual void OnLogon(SteamEvent e)
        {
            DefaultEvent(Logon, e);
        }
        protected virtual void OnTyping(SteamEvent e)
        {
            DefaultEvent(Typing, e);
        }
        protected virtual void OnFriendStateChange(SteamEvent e)
        {
            DefaultEvent(FriendStateChange, e);
        }
        protected virtual void OnSteamGuard(SteamEvent e)
        {
            DefaultEvent(SteamGuard, e);
        }
        #endregion
        /// <summary>
        /// Enumeration of possible authentication results.
        /// </summary>
        public enum LoginStatus
        {
            LoginFailed,
            LoginSuccessful,
            SteamGuard
        }

        /// <summary>
        /// Status of a user.
        /// </summary>
        public enum UserStatus
        {
            Offline = 0,
            Online = 1,
            Busy = 2,
            Away = 3,
            Snooze = 4
        }

        /// <summary>
        /// Visibility of a user's profile.
        /// </summary>
        public enum ProfileVisibility
        {
            Private = 1,
            Public = 3,
            FriendsOnly = 8
        }

        /// <summary>
        /// Available sizes of user avatars.
        /// </summary>
        public enum AvatarSize
        {
            Small,
            Medium,
            Large
        }

        /// <summary>
        /// Available update types.
        /// </summary>
        public enum UpdateType
        {
            UserUpdate,
            Message,
            Emote,
            TypingNotification
        }

        /// <summary>
        /// Structure containing basic friend info.
        /// </summary>
        public class Friend
        {
            public String steamid;
            public bool blocked;
            public DateTime friendSince;
        }

        /// <summary>
        /// Structure containing extensive user info.
        /// </summary>
        public class User
        {
            public String steamid;
            public ProfileVisibility profileVisibility;
            public int profileState;
            public String nickname;
            public DateTime lastLogoff;
            public String profileUrl;
            internal String avatarUrl;
            public UserStatus status;
            public String realName;
            public String primaryGroupId;
            public DateTime joinDate;
            public String locationCountryCode;
            public String locationStateCode;
            public int locationCityId;
        }

        /// <summary>
        /// Basic group info.
        /// </summary>
        public class Group
        {
            public String steamid;
            public bool inviteonly;
        }

        /// <summary>
        /// Structure containing extensive group info.
        /// </summary>
        public class GroupInfo
        {
            public String steamid;
            public DateTime creationDate;
            public String name;
            public String headline;
            public String summary;
            public String abbreviation;
            public String profileUrl;
            internal String avatarUrl;
            public String locationCountryCode;
            public String locationStateCode;
            public int locationCityId;
            public int favoriteAppId;
            public int members;
            public int usersOnline;
            public int usersInChat;
            public int usersInGame;
            public String owner;
        }

        /// <summary>
        /// Structure containing information about a single update.
        /// </summary>
        public class Update
        {
            public DateTime timestamp;
            public String origin;
            public bool localMessage;
            public UpdateType type;
            public String message;
            public UserStatus status;
            public String nick;
        }

        /// <summary>
        /// Structure containing server info.
        /// </summary>
        public class ServerInfo
        {
            public DateTime serverTime;
            public String serverTimeString;
        }
        public static string Fetch(string url, string method, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = true)
        {
            HttpWebResponse response = Request(url, method, data, cookies, ajax);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }
        public String SteamGuardKey
        {
            get;
            set;
        }
        public static HttpWebResponse Request(string url, string method, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = true)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = method;

            request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Host = "steamcommunity.com";
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.47 Safari/536.11";
            request.Referer = "http://steamcommunity.com/trade/1";

            if (ajax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("X-Prototype-Version", "1.7");
            }

            // Cookies
            request.CookieContainer = cookies ?? new CookieContainer();

            // Request data
            if (data != null)
            {
                string dataString = String.Join("&", Array.ConvertAll(data.AllKeys, key =>
                    String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))
                )
                );

                byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
                request.ContentLength = dataBytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            // Get the response
            return request.GetResponse() as HttpWebResponse;
        }
        public String RSALogin(string username, string password)
        {
            Log.WriteInfo("RSALogin called");
            lock(loginLock)
            {
                var data = new NameValueCollection();
                data.Add("username", username);

                String response = Fetch("https://steamcommunity.com/login/getrsakey", "POST", data, null, false);

                JObject json = JObject.Parse(response);
                //GetRsaKey rsaJSON = JsonConvert.DeserializeObject<GetRsaKey>(response);


                // Validate
                if ((bool)json["success"] != true)
                {
                    return null;
                }

                //RSA Encryption
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters rsaParameters = new RSAParameters();

                rsaParameters.Exponent = HexToByte((String)json["publickey_exp"]);
                rsaParameters.Modulus = HexToByte((String)json["publickey_mod"]);

                rsa.ImportParameters(rsaParameters);

                byte[] bytePassword = Encoding.ASCII.GetBytes(password);
                byte[] encodedPassword = rsa.Encrypt(bytePassword, false);
                string encryptedBase64Password = Convert.ToBase64String(encodedPassword);

                JToken token = null;
                JObject loginJson = null;
                //CookieCollection cookies;
                string steamGuardText = "";
                string steamGuardId = "";
                do
                {
                    Debug.Print("SteamWeb: Logging In...");

                    bool steamGuard = loginJson != null && (bool)loginJson["emailauth_needed"] == true;

                    string time = (String)json["timestamp"];

                    data = new NameValueCollection();
                    data.Add("password", encryptedBase64Password);
                    data.Add("username", username);

                    data.Add("captcha_gid", "");
                    data.Add("captcha_text", "");
                    // Captcha end

                    // SteamGuard
                    if (steamGuard  )
                    {                        
                        OnSteamGuard(new SteamEvent());
                        while( String.IsNullOrWhiteSpace(SteamGuardKey))
                        {
                            if (SteamGuardKey.Equals("Cancel"))
                                return null;

                            Thread.Sleep(16);
                        }
                        steamGuardText = Uri.EscapeDataString(SteamGuardKey);
                        steamGuardId = (String)loginJson["emailsteamid"];
                    }
                    data.Add("emailauth", steamGuardText);
                    data.Add("emailsteamid", steamGuardId);
                    // SteamGuard end

                    data.Add("oauth_client_id", "DE45CD61");
                    data.Add("oauth_scope", "read_profile write_profile read_client write_client");

                    data.Add("rsatimestamp", time);

                    HttpWebResponse webResponse = Request("https://steamcommunity.com/mobilelogin/dologin/", "POST", data, null, false);

                    StreamReader reader = new StreamReader(webResponse.GetResponseStream());
                    string response2 = reader.ReadToEnd();

                    token = JToken.Parse(response2);

                    if (!String.IsNullOrEmpty(response2))
                        loginJson = JObject.Parse(response2);

                    //cookies = webResponse.Cookies;

                    if (loginJson == null)
                        break;


                } while (loginJson["emailauth_needed"] != null && (bool)loginJson["emailauth_needed"] == true);

                if (loginJson["oauth"] != null)
                {
                    JObject oauth = JObject.Parse((String)loginJson["oauth"]);
                    Token = (String)oauth["oauth_token"];
                }
                return Token;

            }

        }
        public String Token
        {
            get;
            set;
        }
        /// <summary>
        /// Authenticate with a username and password.
        /// Sends the SteamGuard e-mail if it has been set up.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="emailauthcode">SteamGuard code sent by e-mail</param>
        /// <returns>Indication of the authentication status.</returns>
        public LoginStatus Authenticate( String username, String password, String emailauthcode = "" )
        {
            if (username == "")
                return LoginStatus.LoginFailed;

            String response = steamRequest("/GetTokenWithCredentials/v0001",
                "client_id=DE45CD61&grant_type=password&username=" + Uri.EscapeDataString( username ) + "&password=" + Uri.EscapeDataString( password ) + "&x_emailauthcode=" + emailauthcode + "&scope=read_profile%20write_profile%20read_client%20write_client" );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["access_token"] != null )
                {
                    accessToken = (String)data["access_token"];

                    return login() ? LoginStatus.LoginSuccessful : LoginStatus.LoginFailed;
                }
                else if ( ( (string)data["x_errorcode"] ).Equals( "steamguard_code_required" ) )
                    return LoginStatus.SteamGuard;
                else
                    return LoginStatus.LoginFailed;
            }
            else
            {
                return LoginStatus.LoginFailed;
            }
        }

        /// <summary>
        /// Authenticate with an access token previously retrieved with a username
        /// and password (and SteamGuard code).
        /// </summary>
        /// <param name="accessToken">Access token retrieved with credentials</param>
        /// <returns>Indication of the authentication status.</returns>
        public LoginStatus Authenticate( String accessToken )
        {
            this.accessToken = accessToken;
            return login() ? LoginStatus.LoginSuccessful : LoginStatus.LoginFailed;
        }

        /// <summary>
        /// Fetch all friends of a given user.
        /// </summary>
        /// <remarks>This function does not provide detailed information.</remarks>
        /// <param name="steamid">steamid of target user or self</param>
        /// <returns>List of friends or null on failure.</returns>
        public List<Friend> GetFriends( String steamid = null )
        {
            if ( umqid == null ) return null;
            if ( steamid == null ) steamid = this.steamid;

            String response = steamRequest( "ISteamUserOAuth/GetFriendList/v0001?access_token=" + accessToken + "&steamid=" + steamid );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["friends"] != null )
                {
                    List<Friend> friends = new List<Friend>();

                    foreach ( JObject friend in data["friends"] )
                    {
                        Friend f = new Friend();
                        f.steamid = (String)friend["steamid"];
                        f.blocked = ( (String)friend["relationship"] ).Equals( "ignored" );
                        f.friendSince = unixTimestamp( (long)friend["friend_since"] );
                        friends.Add( f );
                    }

                    return friends;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve information about the specified users.
        /// </summary>
        /// <remarks>This function doesn't have the 100 users limit the original API has.</remarks>
        /// <param name="steamids">64-bit SteamIDs of users</param>
        /// <returns>Information about the specified users</returns>
        public List<User> GetUserInfo( List<String> steamids )
        {
            if ( umqid == null ) return null;

            String response = steamRequest( "ISteamUserOAuth/GetUserSummaries/v0001?access_token=" + accessToken + "&steamids=" + String.Join( ",", steamids.GetRange( 0, Math.Min( steamids.Count, 100 ) ).ToArray() ) );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["players"] != null )
                {
                    List<User> users = new List<User>();

                    foreach ( JObject info in data["players"] )
                    {
                        User user = new User();

                        user.steamid = (String)info["steamid"];
                        user.profileVisibility = (ProfileVisibility)(int)info["communityvisibilitystate"];
                        user.profileState = (int)info["profilestate"];
                        user.nickname = (String)info["personaname"];
                        user.lastLogoff = unixTimestamp( (long)info["lastlogoff"] );
                        user.profileUrl = (String)info["profileurl"];
                        user.status = (UserStatus)(int)info["personastate"];

                        user.avatarUrl = info["avatar"] != null ? (String)info["avatar"] : "";
                        if ( user.avatarUrl != null ) user.avatarUrl = user.avatarUrl.Substring( 0, user.avatarUrl.Length - 4 );

                        user.joinDate = unixTimestamp( info["timecreated"] != null ? (long)info["timecreated"] : 0 );
                        user.primaryGroupId = info["primaryclanid"] != null ? (String)info["primaryclanid"] : "";
                        user.realName = info["realname"] != null ? (String)info["realname"] : "";
                        user.locationCountryCode = info["loccountrycode"] != null ? (String)info["loccountrycode"] : "";
                        user.locationStateCode = info["locstatecode"] != null ? (String)info["locstatecode"] : "";
                        user.locationCityId = info["loccityid"] != null ? (int)info["loccityid"] : -1;

                        users.Add( user );
                    }

                    // Requests are limited to 100 steamids, so issue multiple requests
                    if ( steamids.Count > 100 )
                        users.AddRange( GetUserInfo( steamids.GetRange( 100, Math.Min( steamids.Count - 100, 100 ) ) ) );

                    return users;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<User> GetUserInfo( List<Friend> friends )
        {
            List<String> steamids = new List<String>( friends.Count );
            foreach ( Friend f in friends ) steamids.Add( f.steamid );
            return GetUserInfo( steamids );
        }

        public User GetUserInfo( String steamid = null )
        {
            if ( steamid == null ) steamid = this.steamid;
            return GetUserInfo( new List<String>( new String[] { steamid } ) )[0];
        }

        /// <summary>
        /// Retrieve the avatar of the specified user in the specified format.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="size">Requested avatar size</param>
        /// <returns>The avatar as bitmap on success or null on failure.</returns>
        public Bitmap GetUserAvatar( User user, AvatarSize size = AvatarSize.Small )
        {
            if ( user.avatarUrl.Length == 0 ) return null;

            try
            {
                WebClient client = new WebClient();

                Stream stream;
                if ( size == AvatarSize.Small )
                    stream = client.OpenRead( user.avatarUrl + ".jpg" );
                else if ( size == AvatarSize.Medium )
                    stream = client.OpenRead( user.avatarUrl + "_medium.jpg" );
                else
                    stream = client.OpenRead( user.avatarUrl + "_full.jpg" );

                Bitmap avatar = new Bitmap( stream );
                stream.Flush();
                stream.Close();

                return avatar;
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the avatar of the specified group in the specified format.
        /// </summary>
        /// <param name="group">Group</param>
        /// <param name="size">Requested avatar size</param>
        /// <returns>The avatar as bitmap on success or null on failure.</returns>
        public Bitmap GetGroupAvatar( GroupInfo group, AvatarSize size = AvatarSize.Small )
        {
            User user = new User();
            user.avatarUrl = group.avatarUrl;
            return GetUserAvatar( user, size );
        }

        /// <summary>
        /// Fetch all groups of a given user.
        /// </summary>
        /// <param name="steamid">SteamID</param>
        /// <returns>List of groups.</returns>
        public List<Group> GetGroups( String steamid = null )
        {
            if ( umqid == null ) return null;
            if ( steamid == null ) steamid = this.steamid;

            String response = steamRequest( "ISteamUserOAuth/GetGroupList/v0001?access_token=" + accessToken + "&steamid=" + steamid );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["groups"] != null )
                {
                    List<Group> groups = new List<Group>();

                    foreach ( JObject info in data["groups"] )
                    {
                        Group group = new Group();

                        group.steamid = (String)info["steamid"];
                        group.inviteonly = ( (String)info["permission"] ).Equals( "2" );

                        if ( ( (String)info["relationship"] ).Equals( "Member" ) )
                            groups.Add( group);
                    }

                    return groups;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve information about the specified groups.
        /// </summary>
        /// <param name="steamids">64-bit SteamIDs of groups</param>
        /// <returns>Information about the specified groups</returns>
        public List<GroupInfo> GetGroupInfo( List<String> steamids )
        {
            if ( umqid == null ) return null;

            String response = steamRequest( "ISteamUserOAuth/GetGroupSummaries/v0001?access_token=" + accessToken + "&steamids=" + String.Join( ",", steamids.GetRange( 0, Math.Min( steamids.Count, 100 ) ).ToArray() ) );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["groups"] != null )
                {
                    List<GroupInfo> groups = new List<GroupInfo>();

                    foreach ( JObject info in data["groups"] )
                    {
                        GroupInfo group = new GroupInfo();

                        group.steamid = (String)info["steamid"];
                        group.creationDate = unixTimestamp( (long)info["timecreated"] );
                        group.name = (String)info["name"];
                        group.profileUrl = "http://steamcommunity.com/groups/" + (String)info["profileurl"];
                        group.usersOnline = (int)info["usersonline"];
                        group.usersInChat = (int)info["usersinclanchat"];
                        group.usersInGame = (int)info["usersingame"];
                        group.owner = (String)info["ownerid"];
                        group.members = (int)info["users"];

                        group.avatarUrl = (String)info["avatar"];
                        if ( group.avatarUrl != null ) group.avatarUrl = group.avatarUrl.Substring( 0, group.avatarUrl.Length - 4 );

                        group.headline = info["headline"] != null ? (String)info["headline"] : "";
                        group.summary = info["summary"] != null ? (String)info["summary"] : "";
                        group.abbreviation = info["abbreviation"] != null ? (String)info["abbreviation"] : "";
                        group.locationCountryCode = info["loccountrycode"] != null ? (String)info["loccountrycode"] : "";
                        group.locationStateCode = info["locstatecode"] != null ? (String)info["locstatecode"] : "";
                        group.locationCityId = info["loccityid"] != null ? (int)info["loccityid"] : -1;
                        group.favoriteAppId = info["favoriteappid"] != null ? (int)info["favoriteappid"] : -1;

                        groups.Add( group);
                    }

                    // Requests are limited to 100 steamids, so issue multiple requests
                    if ( steamids.Count > 100 )
                        groups.AddRange( GetGroupInfo( steamids.GetRange( 100, Math.Min( steamids.Count - 100, 100 ) ) ) );

                    return groups;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<GroupInfo> GetGroupInfo( List<Group> groups )
        {
            List<String> steamids = new List<String>( groups.Count );
            foreach ( Group g in groups ) steamids.Add( g.steamid );
            return GetGroupInfo( steamids );
        }

        public GroupInfo GetGroupInfo( String steamid )
        {
            return GetGroupInfo( new List<String>( new String[] { steamid } ) )[0];
        }

        /// <summary>
        /// Let a user know you're typing a message. Should be called periodically.
        /// </summary>
        /// <param name="steamid">Recipient of notification</param>
        /// <returns>Returns a boolean indicating success of the request.</returns>
        public bool SendTypingNotification( User user )
        {
            if ( umqid == null ) return false;

            String response = steamRequest( "ISteamWebUserPresenceOAuth/Message/v0001", "?access_token=" + accessToken + "&umqid=" + umqid + "&type=typing&steamid_dst=" + user.steamid );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                return data["error"] != null && ( (String)data["error"] ).Equals( "OK" );
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Send a text message to the specified user.
        /// </summary>
        /// <param name="steamid">Recipient of message</param>
        /// <param name="message">Message contents</param>
        /// <returns>Returns a boolean indicating success of the request.</returns>
        public bool SendMessage( User user, String message )
        {
            if ( umqid == null ) return false;

            String response = steamRequest( "ISteamWebUserPresenceOAuth/Message/v0001", "?access_token=" + accessToken + "&umqid=" + umqid + "&type=saytext&text=" + Uri.EscapeDataString( message ) + "&steamid_dst=" + user.steamid );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                return data["error"] != null && ( (String)data["error"] ).Equals( "OK" );
            }
            else
            {
                return false;
            }
        }
        public bool SendMessage( String steamid, String message )
        {
            User user = new User();
            user.steamid = steamid;
            return SendMessage( user, message );
        }
        
        /// <summary>
        /// Check for updates and new messages.
        /// </summary>
        /// <returns>A list of updates.</returns>
        public List<Update> Poll()
        {
            if ( umqid == null ) return null;

            String response = steamRequest( "ISteamWebUserPresenceOAuth/Poll/v0001", "?access_token=" + accessToken + "&umqid=" + umqid + "&message=" + message );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( ( (String)data["error"] ).Equals( "OK" ) )
                {
                    message = (int)data["messagelast"];

                    List<Update> updates = new List<Update>();

                    foreach ( JObject info in data["messages"] )
                    {
                        Update update = new Update();

                        update.timestamp = unixTimestamp( (long)info["timestamp"] );
                        update.origin = (String)info["steamid_from"];

                        String type = (String)info["type"];
                        if ( type.Equals( "saytext" ) || type.Equals( "my_saytext" ) || type.Equals( "emote" ) )
                        {
                            update.type = type.Equals( "emote" ) ? UpdateType.Emote : UpdateType.Message;
                            update.message = (String)info["text"];
                            update.localMessage = type.Equals( "my_saytext" );
                            OnNewMessage(new SteamEvent(update));
                        }
                        else if ( type.Equals( "typing" ) )
                        {
                            update.type = UpdateType.TypingNotification;
                            update.message = (String)info["text"]; // Not sure if this is useful
                            OnTyping(new SteamEvent(update));
                        }
                        else if ( type.Equals( "personastate" ) )
                        {
                            update.type = UpdateType.UserUpdate;
                            update.status = (UserStatus)(int)info["persona_state"];
                            update.nick = (String)info["persona_name"];
                            OnFriendStateChange(new SteamEvent(update));
                        }
                        else
                        {
                            continue;
                        }

                        updates.Add( update );
                    }

                    return updates;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves information about the server.
        /// </summary>
        /// <returns>Returns a structure with the information.</returns>
        public ServerInfo GetServerInfo()
        {
            String response = steamRequest( "ISteamWebAPIUtil/GetServerInfo/v0001" );

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["servertime"] != null )
                {
                    ServerInfo info = new ServerInfo();
                    info.serverTime = unixTimestamp( (long)data["servertime"] );
                    info.serverTimeString = (String)data["servertimestring"];
                    return info;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Helper function to complete the login procedure and check the
        /// credentials.
        /// </summary>
        /// <returns>Whether the login was successful or not.</returns>
        private bool login()
        {
            loginStatus = LoginStatus.LoginFailed;

            String response = steamRequest("ISteamWebUserPresenceOAuth/Logon/v0001",
                "?access_token=" + accessToken );
            

            if ( response != null )
            {
                JObject data = JObject.Parse( response );

                if ( data["umqid"] != null )
                {
                    steamid = (String)data["steamid"];
                    umqid = (String)data["umqid"];
                    message = (int)data["message"];
                    OnLogon(new SteamEvent());
                    loginStatus = LoginStatus.LoginSuccessful;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Helper function to perform Steam API requests.
        /// </summary>
        /// <param name="get">Path URI</param>
        /// <param name="post">Post data</param>
        /// <returns>Web response info</returns>
        private String steamRequest( String get, String post = null )
        {
            System.Net.ServicePointManager.Expect100Continue = false;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://63.228.223.110/" + get);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com:443/" + get);
            request.Host = "api.steampowered.com:443";
            request.ProtocolVersion = HttpVersion.Version11;
			request.Accept = "*/*";
			request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
			request.Headers[HttpRequestHeader.AcceptLanguage] = "en-us";
			request.UserAgent = "Steam 1291812 / iPhone";

            if ( post != null )
            {
                request.Method = "POST";
                byte[] postBytes = Encoding.ASCII.GetBytes( post );
                if (postBytes.Length == 0)
                    throw new Exception("Request body is null");
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;
                try
                {
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();
                }
                catch {
                    
                }

                message++;
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if ( (int)response.StatusCode != 200 ) return null;

                String src = new StreamReader( response.GetResponseStream() ).ReadToEnd();
                response.Close();
                return src;
            }
            catch
            {
                return null;
            }
        }

        private DateTime unixTimestamp( long timestamp )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return origin.AddSeconds( timestamp );
        }
        static byte[] HexToByte(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];
            int l = hex.Length;

            for (int i = 0; i < (l >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }

        public static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // allow all certificates
            return true;
        }
    }
}

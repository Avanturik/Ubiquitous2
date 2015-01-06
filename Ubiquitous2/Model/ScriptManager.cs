using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Properties;
using CSScriptLibrary;

namespace UB.Model
{
    public class ScriptManager
    {

        private const string exampleSubFolder = @"\Example";
        private const string scriptsSubFolder = @"\Scripts";
        private const string defaultScriptPath = scriptsSubFolder + exampleSubFolder;
        private static Random random = new Random();

        public void LoadScripts()
        {
            InitializeChatSettings();

            string folder = AppDomain.CurrentDomain.GetData("DataDirectory") + scriptsSubFolder;
            if( !Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                    Directory.CreateDirectory(folder + exampleSubFolder);
                }
                catch {
                    Log.WriteError("Unable to create scripts folder!");
                    return;
                }
            }

            try
            {
                var defaultScriptFiles = Directory.GetFiles(@".\Scripts\Example", "*.*", SearchOption.AllDirectories);

                foreach (var scriptFilePath in defaultScriptFiles)
                    File.Copy(scriptFilePath, folder + exampleSubFolder + @"\" + Path.GetFileName(scriptFilePath), true);
            }
            catch (Exception e)
            {
                Log.WriteError("Unable to copy default scripts: {0}", e.Message);
            }

            var scriptFiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
            foreach( var scriptFileName in scriptFiles)
            {
                //Ignore example scripts
                if (scriptFileName.Contains(exampleSubFolder))
                    continue;

                try
                {
                    var scriptObject = CSScript.Evaluator.LoadFile<IScript>(scriptFileName);

                    //Is known script ?
                    if (scriptObject is IScript)
                    {
                        var config = (scriptObject as IScript).OnConfigRequest();
                        if( config is ChatConfig )
                        {
                            var defaultConfig = config as ChatConfig;
                            if (config != null)
                            {
                                if( !String.IsNullOrWhiteSpace( defaultConfig.ChatName ) )
                                    if (defaultConfig.Parameters == null)
                                        defaultConfig.Parameters = new List<ConfigField>();

                                //Add default parameters
                                defaultConfig.Parameters.AddRange(
                                    new List<ConfigField>() {
                                        new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "username" },
                                        new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = String.Empty },
                                        new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "#blah, blah2" },
                                        new ConfigField("Info1", "Enter Username and fill Channels to get readonly access", "Info", true, null),
                                        new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                                        new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty }
                                    });

                                RefreshChatSettings(defaultConfig);
                                SettingsRegistry.ChatFactory.Add(defaultConfig.ChatName, (chatConfig) =>
                                            {
                                                return (scriptObject as IScript).OnObjectRequest(chatConfig) as IChat;
                                            });
                                            
                            }
                            Log.WriteInfo("Chat script loaded: {0}", scriptFileName);
                        }
                    }
                }
                catch( Exception e )
                {
                    Log.WriteError("Script {0} load error {1}", scriptFileName, e.Message);
                }
            }
        }
        private void RefreshChatSettings(ChatConfig defaultChatConfig)
        {
            var savedConfig = Ubiquitous.Default.Config.ChatConfigs.Where(config => config.ChatName == defaultChatConfig.ChatName).FirstOrDefault();
            //Chat config is missing
            if (savedConfig == null)
            {
                Ubiquitous.Default.Config.ChatConfigs.Add(defaultChatConfig);
            }
            else
            {
                var backupParameters = savedConfig.Parameters.ToList();
                savedConfig.Parameters.Clear();
                foreach (var parameter in defaultChatConfig.Parameters.ToList())
                {
                    if (backupParameters.Any(p => p.Name.Equals(parameter.Name, StringComparison.InvariantCulture)))
                    {
                        parameter.Value = backupParameters.FirstOrDefault(p => p.Name.Equals(parameter.Name, StringComparison.InvariantCulture)).Value;
                    }
                    savedConfig.Parameters.Add(parameter);
                }
            }
        }
        private void InitializeChatSettings()
        {
            if (Ubiquitous.Default.Config.ChatConfigs == null)
            {
                Ubiquitous.Default.Config.ChatConfigs = SettingsRegistry.DefaultChatSettings.ToList();
                Ubiquitous.Default.Save();
            }
        }
    }
}

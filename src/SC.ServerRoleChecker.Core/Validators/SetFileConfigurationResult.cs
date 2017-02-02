using System.Collections.Generic;
using System.IO;
using SC.ServerRoleChecker.Core.Enums;
using SC.ServerRoleChecker.Core.Extensions;

namespace SC.ServerRoleChecker.Core.Validators
{
    public class SetFileConfigurationResult
    {
        public static void Process(FileInfo configFile, ConfigItem configurationItem,
            IEnumerable<ServerRoleType> serverRoles, SearchProviderType searchProviderType)
        {
            var configFileStatus = configurationItem.HasToBeEnabledOrDisabled(serverRoles, searchProviderType);

            if (configFile == null)
            {
                if (configFileStatus == ConfigFileStatus.HasToBeEnabled)
                {
                    configurationItem.SetResult(ConfigFileResult.NotValidFileNotFound);
                }
                return;
            }

            var configFileName = configFile.Name.ToSanitizedConfigFileName();
            var configurationItemFileName = configurationItem.FileName.ToSanitizedConfigFileName();
            if (configFileName == configurationItemFileName)
            {
                
                if (configFileStatus == ConfigFileStatus.HasToBeEnabled)
                {
                    if (!configFile.Exists)
                    {
                        configurationItem.SetResult(ConfigFileResult.NotValidFileNotFound);
                    }
                    else
                    {
                        configurationItem.FileName = configFile.Name;
                        if (configFile.Extension == ".config")
                            configurationItem.SetResult(ConfigFileResult.IsValid);
                        else
                            configurationItem.SetResult(ConfigFileResult.NotValid);
                    }
                }
                else
                {
                    if (!configFile.Exists)
                    {
                        configurationItem.SetResult(ConfigFileResult.IsValid);
                    }
                    else if (configFile.Extension != ".config")
                    {
                        configurationItem.FileName = configFile.Name;
                        configurationItem.SetResult(ConfigFileResult.IsValid);
                    }
                    else
                    {
                        configurationItem.FileName = configFile.Name;
                        configurationItem.SetResult(ConfigFileResult.NotValid);
                    }
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using SC.ServerRoleChecker.Core.Enums;
using SC.ServerRoleChecker.Core.Extensions;
using SC.ServerRoleChecker.Core.Models;

namespace SC.ServerRoleChecker.Core
{
    public class ConfigFileChecker
    {
        private readonly FileInfo _configFile;
        private readonly ConfigItem _configItem;

        public ConfigFileChecker(FileInfo configFile, ConfigItem configItem)
        {
            _configFile = configFile;
            _configItem = configItem;
        }

        public void Validate(IEnumerable<ServerRoleType> serverRoles, SearchProviderType searchProviderType)
        {
            var configFileStatus = _configItem.HasToBeEnabledOrDisabled(serverRoles, searchProviderType);

            if (_configFile == null)
            {
                if (configFileStatus == ConfigFileStatus.HasToBeEnabled)
                    _configItem.SetResult(ConfigFileResult.NotValidFileNotFound);
                else if(configFileStatus == ConfigFileStatus.HasToBeDisabled)
                    _configItem.SetResult(ConfigFileResult.IsValidFileNotFound);
                return;
            }

            var configFileName = _configFile.Name.ToSanitizedConfigFileName();
            var configurationItemFileName = _configItem.FileName.ToSanitizedConfigFileName();
            if (configFileName != configurationItemFileName)
                return;

            if (configFileStatus == ConfigFileStatus.HasToBeEnabled)
                if (!_configFile.Exists)
                {
                    _configItem.SetResult(ConfigFileResult.NotValidFileNotFound);
                }
                else
                {
                    _configItem.FileName = _configFile.Name;
                    if (_configFile.IsConfigFileEnabled())
                        _configItem.SetResult(ConfigFileResult.IsValid);
                    else
                        _configItem.SetResult(ConfigFileResult.NotValid);
                }
            else if (configFileStatus == ConfigFileStatus.HasToBeDisabled)
                if (!_configFile.Exists)
                {
                    _configItem.SetResult(ConfigFileResult.IsValidFileNotFound);
                }
                else if (!_configFile.IsConfigFileEnabled())
                {                    
                    _configItem.SetResult(ConfigFileResult.IsValid);
                }
                else
                {
                    _configItem.SetResult(ConfigFileResult.NotValid);
                }
            else if (configFileStatus == ConfigFileStatus.NotApplicable)
                _configItem.SetResult(ConfigFileResult.IsValid);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using SC.ServerRoleChecker.Core.Enums;

namespace SC.ServerRoleChecker.Core.Models
{
    public class ConfigItem
    {
        private const string ENABLE_TEXT = "Enable";
        private const string DISABLE_TEXT = "Disable";
        private const string NOTAPPLICABLE_TEXT = "n/a";

        public ConfigItem()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; private set; }
        public string ProductName { get; set; }
        public string DirectoryPath { get; set; }
        public string FileName { get; set; }

        public string FileFullPath
        {
            get { return DirectoryPath.TrimEnd('\\') + "\\" + FileName; }
        }

        public string Type { get; set; }
        public string SearchProviderUsed { get; set; }
        public string ContentDelivery { get; set; }
        public string ContentManagement { get; set; }
        public string Processing { get; set; }
        public string RemoteReportingServer { get; set; }
        public string RemoteReportingClient { get; set; }

        public ConfigFileResult Result { get; private set; }

        public ConfigFileStatus HasToBeEnabledOrDisabled(IEnumerable<ServerRoleType> roles,
            SearchProviderType searchProvider)
        {
            if (!string.IsNullOrWhiteSpace(SearchProviderUsed) &&
                (SearchProviderUsed.ToLower().Contains("lucene") || SearchProviderUsed.ToLower().Contains("solr") ||
                 SearchProviderUsed.ToLower().Contains("azure")))
            {
                if (searchProvider == SearchProviderType.Lucene)
                {
                    if (SearchProviderUsed.ToLower().Contains("solr") || SearchProviderUsed.ToLower().Contains("azure"))
                        return ConfigFileStatus.HasToBeDisabled;

                    return GetConfigFileStatusBasedOnRoles(roles);
                }

                if (searchProvider == SearchProviderType.SOLR)
                {
                    if (SearchProviderUsed.ToLower().Contains("lucene") ||
                        SearchProviderUsed.ToLower().Contains("azure"))
                        return ConfigFileStatus.HasToBeDisabled;

                    return GetConfigFileStatusBasedOnRoles(roles);
                }

                if (searchProvider == SearchProviderType.Azure)
                {
                    if (SearchProviderUsed.ToLower().Contains("lucene") || SearchProviderUsed.ToLower().Contains("solr"))
                        return ConfigFileStatus.HasToBeDisabled;

                    return GetConfigFileStatusBasedOnRoles(roles);
                }
            }

            return GetConfigFileStatusBasedOnRoles(roles);
        }

        private ConfigFileStatus GetConfigFileStatusBasedOnRoles(IEnumerable<ServerRoleType> roles)
        {
            var result = ConfigFileStatus.NotApplicable;

            if (roles.Contains(ServerRoleType.CD))
                result = SetConfigFileStatus(result, GetConfigFileStatusBasedOnInstruction(ContentDelivery));
            if (roles.Contains(ServerRoleType.CM))
                result = SetConfigFileStatus(result, GetConfigFileStatusBasedOnInstruction(ContentManagement));
            if (roles.Contains(ServerRoleType.Processing))
                result = SetConfigFileStatus(result, GetConfigFileStatusBasedOnInstruction(Processing));
            if (roles.Contains(ServerRoleType.RemoteReportingServer))
                result = SetConfigFileStatus(result, GetConfigFileStatusBasedOnInstruction(RemoteReportingServer));
            if (roles.Contains(ServerRoleType.RemoteReportingClient))
                result = SetConfigFileStatus(result, GetConfigFileStatusBasedOnInstruction(RemoteReportingClient));

            return result;
        }

        private ConfigFileStatus GetConfigFileStatusBasedOnInstruction(string instruction)
        {
            if (instruction.Equals(ENABLE_TEXT, StringComparison.InvariantCultureIgnoreCase))
                return ConfigFileStatus.HasToBeEnabled;
            if (instruction.Equals(DISABLE_TEXT, StringComparison.InvariantCultureIgnoreCase))
                return ConfigFileStatus.HasToBeDisabled;
            if (instruction.Equals(NOTAPPLICABLE_TEXT, StringComparison.InvariantCultureIgnoreCase))
                return ConfigFileStatus.NotApplicable;

            throw new ArgumentException(nameof(instruction));
        }

        private ConfigFileStatus SetConfigFileStatus(ConfigFileStatus currentStatus, ConfigFileStatus newStatus)
        {
            if (currentStatus == ConfigFileStatus.HasToBeEnabled)
                return currentStatus;

            if (currentStatus == ConfigFileStatus.HasToBeDisabled)
                if (newStatus == ConfigFileStatus.HasToBeEnabled)
                    return newStatus;
                else
                    return currentStatus;

            if (currentStatus == ConfigFileStatus.NotApplicable)
                if ((newStatus == ConfigFileStatus.HasToBeDisabled) || (newStatus == ConfigFileStatus.HasToBeEnabled))
                    return newStatus;
                else
                    return currentStatus;

            throw new ArgumentException(nameof(newStatus));
        }

        public void SetResult(ConfigFileResult result)
        {
            Result = result;
        }
    }
}
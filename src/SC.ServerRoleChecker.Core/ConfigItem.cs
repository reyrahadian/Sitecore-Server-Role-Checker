using System.Collections.Generic;
using System.Linq;
using SC.ServerRoleChecker.Core.Enums;

namespace SC.ServerRoleChecker.Core
{
	public class ConfigItem
	{
		private const string ENABLE_TEXT = "Enable";
		public string ProductName { get; set; }
		public string FilePath { get; set; }
		public string ConfigFileName { get; set; }
		public string Type { get; set; }
		public string SearchProviderUsed { get; set; }
		public string ContentDelivery { get; set; }
		public string ContentManagement { get; set; }
		public string Processing { get; set; }
		public string ReportingService { get; set; }

		public ConfigFileResult Result { get; private set; }

		public ConfigFileStatus HasToBeEnabledOrDisabled(IEnumerable<ServerRoleType> roles, SearchProviderType searchProvider)
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
					if (SearchProviderUsed.ToLower().Contains("lucene") || SearchProviderUsed.ToLower().Contains("azure"))
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
			if (roles.Contains(ServerRoleType.CD))
				if (ContentDelivery == ENABLE_TEXT)
					return ConfigFileStatus.HasToBeEnabled;
			if (roles.Contains(ServerRoleType.CM))
				if (ContentManagement == ENABLE_TEXT)
					return ConfigFileStatus.HasToBeEnabled;
			if (roles.Contains(ServerRoleType.Processing))
				if (Processing == ENABLE_TEXT)
					return ConfigFileStatus.HasToBeEnabled;
			if (roles.Contains(ServerRoleType.ReportingService))
				if (ReportingService == ENABLE_TEXT)
					return ConfigFileStatus.HasToBeEnabled;

			return ConfigFileStatus.HasToBeDisabled;
		}

		public void SetResult(ConfigFileResult result)
		{
			Result = result;
		}
	}
}
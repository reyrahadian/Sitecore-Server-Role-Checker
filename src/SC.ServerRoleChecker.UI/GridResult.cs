using SC.ServerRoleChecker.Core.Enums;

namespace SC.ServerRoleChecker.UI
{
	internal class GridResult
	{
		public string ConfigFileName { get; set; }
		public ConfigFileStatus Status { get; set; }
		public ConfigFileResult IsValid { get; set; }
	}
}
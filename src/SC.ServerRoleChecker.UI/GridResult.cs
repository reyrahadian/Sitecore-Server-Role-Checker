using SC.ServerRoleChecker.Core.Enums;

namespace SC.ServerRoleChecker.UI
{
	public class GridResult
	{
		public string ConfigFileName { get; set; }
		public ConfigFileStatus Status { get; set; }

		public string StatusText
		{
			get
			{
				if (Status == ConfigFileStatus.HasToBeDisabled)
				{
					return "Need to be disabled";
				}

				return "Need to be enabled";
			}
		}

		public ConfigFileResult IsValid { get; set; }

		public string ConfigFileResultImage
		{
			get
			{
				if (IsValid == ConfigFileResult.IsValid)
				{
					return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/tick.png";
				}
				if (IsValid == ConfigFileResult.NotValid)
				{
					return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/close.png";
				}

				return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/missing-file.png";
			}
		}

		public string ConfigResultImageToolTip
		{
			get
			{
				if (IsValid == ConfigFileResult.NotValidFileNotFound)
				{
					return "Configuration file was not found";
				}

				return string.Empty;
			}
		}

		public string ButtonToggleText
		{
			get
			{
				if (Status == ConfigFileStatus.HasToBeDisabled)
				{
					return "Disable";
				}

				return "Enable";
			}
		}
	}
}
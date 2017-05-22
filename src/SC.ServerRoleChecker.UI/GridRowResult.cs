using System;
using System.Windows;
using SC.ServerRoleChecker.Core.Enums;

namespace SC.ServerRoleChecker.UI
{
    public class GridRowResult
    {
        public GridRowResult(Guid id)
        {
            ID = id;
        }

        public Guid ID { get; private set; }
        public string ConfigFileName { get; set; }
        public string ConfigFileFullPath { get; set; }
        public ConfigFileStatus Status { get; set; }

        public string StatusText
        {
            get
            {
                if (Status == ConfigFileStatus.HasToBeDisabled)
                    return "Need to be disabled";
                if (Status == ConfigFileStatus.NotApplicable)
                    return "Not Applicable";
                if (Status == ConfigFileStatus.HasToBeEnabled)
                    return "Need to be enabled";

                return "Unrecognized status!";
            }
        }

        public ConfigFileResult Result { get; set; }

        public string ConfigFileResultImage
        {
            get
            {
                if (Result == ConfigFileResult.IsValid || Result == ConfigFileResult.IsValidFileNotFound)
                    return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/tick.png";
                if (Result == ConfigFileResult.NotValid)
                    return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/close.png";

                return "pack://application:,,,/SC.ServerRoleChecker.UI;component/Images/missing-file.png";
            }
        }

        public string ConfigResultImageToolTip
        {
            get
            {
                if (Result == ConfigFileResult.NotValidFileNotFound)
                    return "The configuration file was not found, the file should exist and " + StatusText.ToLower();
                if (Result == ConfigFileResult.IsValid)
                    return "The current configuration is correct";
                if (Result == ConfigFileResult.NotValid)
                    return "The current configuration is not correct";
                if (Result == ConfigFileResult.IsValidFileNotFound)
                    return "The configuration file was not found but this configuration is correct";

                return string.Empty;
            }
        }

        public string ButtonToggleText
        {
            get
            {
                if (IsEnabled())
                    return "Disable";

                return "Enable";
            }
        }

        public Visibility ToggleButtonVisible
        {
            get
            {
                if (Result == ConfigFileResult.NotValidFileNotFound)
                    return Visibility.Hidden;

                return Visibility.Visible;
            }
        }

        public string SearchProvider { get; set; }

        private bool IsEnabled()
        {
            return ConfigFileName.EndsWith(".config");
        }

        public string Note { get; set; }
    }
}
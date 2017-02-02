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

                return "Need to be enabled";
            }
        }

        public ConfigFileResult Result { get; set; }

        public string ConfigFileResultImage
        {
            get
            {
                if (Result == ConfigFileResult.IsValid)
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
                    return "Configuration file was not found";
                if (Result == ConfigFileResult.IsValid)
                    return "Configuration file is valid";
                if (Result == ConfigFileResult.NotValid)
                    return "Configuration file is not valid";

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

        private bool IsEnabled()
        {
            return ConfigFileName.EndsWith(".config");
        }

        public Visibility ToggleButtonVisible
        {
            get
            {
                if(Result == ConfigFileResult.NotValidFileNotFound)
                    return Visibility.Hidden;

                return Visibility.Visible;
            }
        }       
    }
}
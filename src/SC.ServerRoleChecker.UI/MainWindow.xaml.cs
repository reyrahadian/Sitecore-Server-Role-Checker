using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using SC.ServerRoleChecker.Core;
using SC.ServerRoleChecker.Core.Enums;
using SC.ServerRoleChecker.Core.Extensions;
using SC.ServerRoleChecker.Core.Validators;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using RadioButton = System.Windows.Controls.RadioButton;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SC.ServerRoleChecker.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<ConfigItem> _configurationItems;
        private DirectoryInfo _websiteFolderDirectoryInfo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private List<ServerRoleType> SelectedRoles
        {
            get
            {
                var roles = new List<ServerRoleType>();
                if (cbCD.IsChecked.GetValueOrDefault())
                    roles.Add(ServerRoleType.CD);
                if (cbCM.IsChecked.GetValueOrDefault())
                    roles.Add(ServerRoleType.CM);
                if (cbProcessing.IsChecked.GetValueOrDefault())
                    roles.Add(ServerRoleType.Processing);
                if (cbRemoteReportingServer.IsChecked.GetValueOrDefault())
                    roles.Add(ServerRoleType.RemoteReportingServer);
                if(cbRemoteReportingClient.IsChecked.GetValueOrDefault())
                    roles.Add(ServerRoleType.RemoteReportingClient);

                return roles;
            }
        }

        private SearchProviderType SelectedSearchProviderType
        {
            get
            {
                if (rbLucene.IsChecked.GetValueOrDefault())
                    return SearchProviderType.Lucene;

                if (rbSOLR.IsChecked.GetValueOrDefault())
                    return SearchProviderType.SOLR;

                if (rbAzure.IsChecked.GetValueOrDefault())
                    return SearchProviderType.Azure;

                return SearchProviderType.Lucene;
            }
        }

        private void btnBrowseWebsiteFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _websiteFolderDirectoryInfo = new DirectoryInfo(dialog.SelectedPath);
                textWebsiteFolderPath.Text = dialog.SelectedPath;
            }
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (_websiteFolderDirectoryInfo == null)
            {
                MessageBox.Show(this, "Please select the website folder");
                return;
            }

            ParseCsv();
            _websiteFolderDirectoryInfo.Refresh();
            if (_websiteFolderDirectoryInfo.Exists)
                try
                {
                    if (!SelectedRoles.Any())
                    {
                        MessageBox.Show(this, "Please select a role");
                        return;
                    }
                    foreach (var configurationItem in _configurationItems)
                    {
                        var filePath = StripWebsitePath(configurationItem.FileFullPath).ToSanitizedConfigFileName();
                        FileInfo file = null;
                        try
                        {
                            var files = _websiteFolderDirectoryInfo.GetFiles(filePath + "*");
                            file = files.FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            //suppress file not found                            
                        }


                        CheckFileConfiguration(file, configurationItem);
                    }

                    DisplayResultInGridView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                }
            else
                MessageBox.Show(this, "Invalid website folder path ");
        }

        private void ParseCsv()
        {
            var csvFilePath = GetCsvFilePath();
            var sr = new StreamReader(csvFilePath);
            var csv = new CsvReader(sr);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.RegisterClassMap<ConfigItemClassMap>();
            _configurationItems = csv.GetRecords<ConfigItem>().ToList();
        }

        private string GetCsvFilePath()
        {
            if (rb81u3.IsChecked.GetValueOrDefault())
                return AppDomain.CurrentDomain.BaseDirectory +
                       "configurations/Config_Enable-Disable_Sitecore_8.1_upd3.csv";

            if (rb82.IsChecked.GetValueOrDefault())
                return AppDomain.CurrentDomain.BaseDirectory +
                       "configurations/Config Enable-Disable Sitecore_8.2-160906.csv";

            if (rb82u1.IsChecked.GetValueOrDefault())
                return AppDomain.CurrentDomain.BaseDirectory +
                       "configurations/Config Enable-Disable Sitecore_8.2 Update1.csv";

            if (rb82u2.IsChecked.GetValueOrDefault())
                return AppDomain.CurrentDomain.BaseDirectory +
                       "configurations/Config Enable-Disable Sitecore_8.2 Update2.csv";

            throw new Exception("There's no Sitecore version selected");
        }

        private FileInfo FindFile(DirectoryInfo rootFolder, string fileName)
        {
            var fileInfo = rootFolder.GetFiles(fileName + "*").FirstOrDefault();
            if (fileInfo != null)
                return fileInfo;

            var subDirectories = rootFolder.GetDirectories();
            if (subDirectories.Any())
                foreach (var directoryInfo in subDirectories)
                {
                    var file = FindFile(directoryInfo, fileName);
                    if (file != null)
                        return file;
                }

            return null;
        }

        private void CheckFileConfiguration(FileInfo configFile, ConfigItem configurationItem)
        {
            SetFileConfigurationResult.Process(configFile, configurationItem, SelectedRoles, SelectedSearchProviderType);
        }

        private void DisplayResultInGridView()
        {
            var gridRows = new List<GridRowResult>();

            foreach (var configurationItem in _configurationItems)
                gridRows.Add(new GridRowResult(configurationItem.ID)
                {
                    ConfigFileName = configurationItem.FileName,
                    ConfigFileFullPath =
                        configurationItem.DirectoryPath.TrimEnd('\\') + "\\" + configurationItem.FileName,
                    Status =
                        configurationItem.HasToBeEnabledOrDisabled(SelectedRoles, SelectedSearchProviderType),
                    SearchProvider = configurationItem.SearchProviderUsed,
                    Result = configurationItem.Result
                });

            dataGrid.ItemsSource = gridRows;
        }

        private void ButtonToggle_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var gridRow = (GridRowResult)button.CommandParameter;
            var tempConfigFileFullPath = StripWebsitePath(gridRow.ConfigFileFullPath);
            var file = _websiteFolderDirectoryInfo.GetFiles(tempConfigFileFullPath).SingleOrDefault();
            if (file == null)
            {
                MessageBox.Show("Cannot found the specified file");
                return;
            }

            if (button.Content.ToString() == "Enable")
            {
                tempConfigFileFullPath = file.FullName.ToSanitizedConfigFileName();
                file.MoveTo(tempConfigFileFullPath);
                CheckFileConfiguration(file, _configurationItems.Single(x => x.ID == gridRow.ID));
            }
            else
            {
                file.MoveTo(file.FullName + ".disabled");
                CheckFileConfiguration(file, _configurationItems.Single(x => x.ID == gridRow.ID));
            }

            DisplayResultInGridView();
        }

        private string StripWebsitePath(string filePath)
        {
            if (filePath.StartsWith("\\website\\"))
                return filePath.Replace("\\website\\", string.Empty);

            return filePath;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var sourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "exports/results.csv";

            var writer = new CsvWriter(new StreamWriter(sourceFilePath));
            writer.Configuration.HasHeaderRecord = true;
            writer.Configuration.RegisterClassMap<ConfigItemClassResultMap>();
            writer.WriteRecords(_configurationItems);
            var dlg = new SaveFileDialog();
            dlg.FileName = "results";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV documents (.csv)|*.csv";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                var destinationFilePath = dlg.FileName;
                File.Copy(sourceFilePath, destinationFilePath, true);
            }
        }             
    }
}
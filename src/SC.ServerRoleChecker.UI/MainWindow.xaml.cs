using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CsvHelper;
using SC.ServerRoleChecker.Core;
using SC.ServerRoleChecker.Core.Enums;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SC.ServerRoleChecker.UI
{
	/// <summary>
	///   Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<ConfigItem> _configurationItems;
		private DirectoryInfo _websiteFolderDirectoryInfo;

		public MainWindow()
		{
			InitializeComponent();
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
					var selectedRoles = GetSelectedRoles();
					foreach (var configurationItem in _configurationItems)
					{
						var file = _websiteFolderDirectoryInfo.GetFiles(configurationItem.ConfigFileName + "*").FirstOrDefault();
						if (file == null)
						{
							var configFileName = SanitizeConfigFileName(configurationItem.ConfigFileName);
							file = FindFile(_websiteFolderDirectoryInfo.GetDirectories("App_Config/include").Single(),
								configFileName + "*");
							var searchProviderType = GetSelectedSearchProviderType();
							CheckConfiguration(file, configurationItem, selectedRoles, searchProviderType);
						}
					}

					DisplayResultInGridView(selectedRoles);
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, ex.Message);
				}
			else
				MessageBox.Show(this, "Website folder path invalid");
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
				return AppDomain.CurrentDomain.BaseDirectory + "configurations/Config_Enable-Disable_Sitecore_8.1_upd3.csv";

			if (rb82.IsChecked.GetValueOrDefault())
				return AppDomain.CurrentDomain.BaseDirectory + "configurations/Config Enable-Disable Sitecore_8.2-160906.csv";

			if (rb82u1.IsChecked.GetValueOrDefault())
				return AppDomain.CurrentDomain.BaseDirectory + "configurations/Config Enable-Disable Sitecore_8.2 Update1.csv";

			if (rb82u2.IsChecked.GetValueOrDefault())
				return AppDomain.CurrentDomain.BaseDirectory + "configurations/Config Enable-Disable Sitecore_8.2 Update2.csv";

			throw new Exception("There's no Sitecore version selected");
		}

		private List<ServerRoleType> GetSelectedRoles()
		{
			var roles = new List<ServerRoleType>();
			if (cbCD.IsChecked.GetValueOrDefault())
				roles.Add(ServerRoleType.CD);
			if (cbCM.IsChecked.GetValueOrDefault())
				roles.Add(ServerRoleType.CM);
			if (cbProcessing.IsChecked.GetValueOrDefault())
				roles.Add(ServerRoleType.Processing);
			if (cbReportingService.IsChecked.GetValueOrDefault())
				roles.Add(ServerRoleType.ReportingService);

			return roles;
		}

		private SearchProviderType GetSelectedSearchProviderType()
		{
			if (rbLucene.IsChecked.GetValueOrDefault())
				return SearchProviderType.Lucene;

			if (rbSOLR.IsChecked.GetValueOrDefault())
				return SearchProviderType.SOLR;

			if (rbAzure.IsChecked.GetValueOrDefault())
				return SearchProviderType.Azure;

			return SearchProviderType.Lucene;
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

		private static void CheckConfiguration(FileInfo configFile, ConfigItem configurationItem,
			IEnumerable<ServerRoleType> selectedRoles, SearchProviderType searchProviderType)
		{
			if (configFile == null)
				return;

			var configFileName = SanitizeConfigFileName(configFile.Name);
			var configurationItemFileName = SanitizeConfigFileName(configurationItem.ConfigFileName);
			if (configFileName == configurationItemFileName)
			{
				var configFileStatus = configurationItem.HasToBeEnabledOrDisabled(selectedRoles, searchProviderType);
				if (configFileStatus == ConfigFileStatus.HasToBeEnabled)
				{
					if (!configFile.Exists)
					{
						configurationItem.SetResult(ConfigFileResult.NotValidFileNotFound);
					}
					else
					{
						configurationItem.ConfigFileName = configFile.Name;
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
						configurationItem.ConfigFileName = configFile.Name;
						configurationItem.SetResult(ConfigFileResult.IsValid);
					}
					else
					{
						configurationItem.ConfigFileName = configFile.Name;
						configurationItem.SetResult(ConfigFileResult.NotValid);
					}
				}
			}
		}

		private static string SanitizeConfigFileName(string configFileName)
		{
			if (string.IsNullOrWhiteSpace(configFileName))
				return string.Empty;

			var pos = configFileName.IndexOf(".config", StringComparison.Ordinal);
			return configFileName.Substring(0, pos + 7);
		}

		private void DisplayResultInGridView(List<ServerRoleType> selectedRoles)
		{
			var gridRows = new List<GridResult>();

			foreach (var configurationItem in _configurationItems)
				gridRows.Add(new GridResult
				{
					ConfigFileName = configurationItem.ConfigFileName,
					Status = configurationItem.HasToBeEnabledOrDisabled(selectedRoles, GetSelectedSearchProviderType()),
					IsValid = configurationItem.Result
				});

			dataGrid.ItemsSource = gridRows.OrderByDescending(x => x.IsValid);
		}

		private void ButtonToggle_OnClick(object sender, RoutedEventArgs e)
		{
			var button = (Button) sender;
			var configFileName = button.CommandParameter;
			var file = FindFile(_websiteFolderDirectoryInfo.GetDirectories("App_Config/include").Single(), configFileName + "*");
			if (button.Content.ToString() == "Enable")
				file.MoveTo(SanitizeConfigFileName(file.FullName));
			else
				file.MoveTo(file.FullName + ".disabled");

			btnAnalyze_Click(sender, e);
		}
	}
}
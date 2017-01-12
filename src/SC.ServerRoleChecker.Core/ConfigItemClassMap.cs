using CsvHelper.Configuration;

namespace SC.ServerRoleChecker.Core
{
	public class ConfigItemClassMap : CsvClassMap<ConfigItem>
	{
		public ConfigItemClassMap()
		{
			Map(x => x.ProductName).Index(0);
			Map(x => x.FilePath).Index(1);
			Map(x => x.ConfigFileName).Index(2);
			Map(x => x.Type).Index(3);
			Map(x => x.SearchProviderUsed).Index(4);
			Map(x => x.ContentDelivery).Index(5);
			Map(x => x.ContentManagement).Index(6);
			Map(x => x.Processing).Index(7);
			Map(x => x.ReportingService).Index(8);
		}
	}
}
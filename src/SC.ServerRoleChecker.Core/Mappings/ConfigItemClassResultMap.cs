namespace SC.ServerRoleChecker.Core.Mappings
{
    public class ConfigItemClassResultMap : ConfigItemClassMap
    {
        public ConfigItemClassResultMap()
        {
            Map(x => x.Result).Index(10);
        }
    }
}
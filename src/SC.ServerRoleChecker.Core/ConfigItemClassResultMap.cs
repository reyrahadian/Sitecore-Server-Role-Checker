namespace SC.ServerRoleChecker.Core
{
    public class ConfigItemClassResultMap : ConfigItemClassMap
    {
        public ConfigItemClassResultMap()
        {
            Map(x => x.Result).Index(10);
        }
    }
}
using System.IO;

namespace SC.ServerRoleChecker.Core.Extensions
{
	internal static class FileInfoExtensions
	{
		public static bool IsConfigFileEnabled(this FileInfo fileInfo)
		{
			return fileInfo.Extension.ToLower() == ".config";
		}
	}
}
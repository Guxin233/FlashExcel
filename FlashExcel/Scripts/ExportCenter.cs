using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ExportCenter
{
	public static void Export(List<string> fileList)
	{
		// 清空多语言管理器的缓存数据
		LanguageMgr.Instance.ClearCacheLanguage();
		// 加载多语言总表
		LanguageMgr.Instance.LoadAutoGenerateLanguageToCache();

		// 加载选择的Excel文件列表
		for (int i = 0; i < fileList.Count; i++)
		{
			string filePath = fileList[i];
			ExcelData excelFile = new ExcelData(filePath);
			if (excelFile.Load())
			{
				if (excelFile.Export())
				{
					// 导出成功后，我们解析表格的多语言数据
					var data = LanguageMgr.ParseLanguage(excelFile);
					LanguageMgr.Instance.CacheLanguage(data);
				}
			}
			excelFile.Dispose();
		}

		// 创建新的多语言总表文件
		LanguageMgr.Instance.CreateAutoGenerateLanguageFile();
		// 导出多语言总表文件
		LanguageMgr.Instance.ExportAutoGenerateLanguageFile();
	}

	public static void RunCommand(string[] args)
	{
		// 初始化配置
		ExportConfig.Instance.Init();
		SettingConfig.Instance.Init();

		// 读取配置
		ExportConfig.Instance.ReadConfig();
		SettingConfig.Instance.ReadConfig();

		// 导出Excel文件列表
		Export(args.ToList());
	}
}
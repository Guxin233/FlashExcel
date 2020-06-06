//**************************************************
// Copyright©2019 何冠峰
// Licensed under the MIT license
//**************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

public class SettingConfig
{
	public static readonly SettingConfig Instance = new SettingConfig();

	/// <summary>
	/// 存储在本地的配置文件名称
	/// </summary>
	private const string StrConfigFileName = "ExcelSetting.data";

	/// <summary>
	/// 是否开启数值单元格自动补全
	/// </summary>
	public bool EnableAutoCompleteCell = false;

	/// <summary>
	/// 数值单元格自动补全的内容
	/// </summary>
	public string AutoCompleteCellContent = "0";

	/// <summary>
	/// 命名空间内容
	/// </summary>
	public string NamespaceContent = string.Empty;



	private SettingConfig()
	{
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
	}

	/// <summary>
	/// 读取配置文件
	/// </summary>
	public void ReadConfig()
	{
		string appPath = Application.StartupPath;
		string configPath = Path.Combine(appPath, StrConfigFileName);

		// 如果配置文件不存在
		if (!File.Exists(configPath))
			return;

		FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read);
		try
		{
			StreamReader sr = new StreamReader(fs);

			EnableAutoCompleteCell = sr.ReadLine() == "true";
			AutoCompleteCellContent = sr.ReadLine();
			NamespaceContent = sr.ReadLine();
			ParseNamespaceContent(NamespaceContent);

			sr.Dispose();
			sr.Close();
		}
		catch (Exception e)
		{
			throw e;
		}
		finally
		{
			fs.Dispose();
			fs.Close();
		}
	}

	/// <summary>
	/// 存储配置文件
	/// </summary>
	public void SaveConfig()
	{
		string appPath = Application.StartupPath;
		string configPath = Path.Combine(appPath, StrConfigFileName);

		// 删除旧文件
		if (File.Exists(configPath))
			File.Delete(configPath);

		FileStream fs = new FileStream(configPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		try
		{
			StreamWriter sw = new StreamWriter(fs);
			sw.Flush();

			if (AutoCompleteCellContent == null)
				AutoCompleteCellContent = string.Empty;
			if (NamespaceContent == null)
				NamespaceContent = string.Empty;

			sw.WriteLine(EnableAutoCompleteCell ? "true" : "false");
			sw.WriteLine(AutoCompleteCellContent);
			sw.WriteLine(NamespaceContent);

			sw.Flush();
			sw.Dispose();
			sw.Close();
		}
		catch (Exception e)
		{
			throw e;
		}
		finally
		{
			fs.Dispose();
			fs.Close();
		}
	}

	/// <summary>
	/// 当关闭设置窗体的时候
	/// </summary>
	public void OnCloseSettingForm()
	{
		// 我们需要重新分析预设的命名空间
		ParseNamespaceContent(NamespaceContent);
	}


	#region CS脚本命名空间相关
	private readonly Dictionary<string, string> _namespace = new Dictionary<string, string>();

	/// <summary>
	/// 分析命名空间预设内容
	/// </summary>
	private void ParseNamespaceContent(string content)
	{
		// 清空缓存数据
		_namespace.Clear();

		if (string.IsNullOrEmpty(content))
			return;

		string[] splits = content.Split(';');
		for (int i = 0; i < splits.Length; i++)
		{
			string temp = splits[i];
			if (string.IsNullOrEmpty(temp))
				continue;
			string namepsace = StringHelper.GetNamespace(temp);
			string exportName = StringHelper.GetExportName(temp);
			if (string.IsNullOrEmpty(namepsace) == false && string.IsNullOrEmpty(exportName) == false)
			{
				namepsace = namepsace.Trim();
				exportName = exportName.Trim();
				_namespace.Add(exportName, namepsace);
			}
		}
	}

	/// <summary>
	/// 获取导出器的命名空间名称，如果没有返回空
	/// </summary>
	public string GetNamespace(string exporterName)
	{
		string name = string.Empty;
		_namespace.TryGetValue(exporterName, out name);
		return name;
	}
	#endregion
}
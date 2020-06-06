using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

[ExportAttribute("导出CS脚本")]
public class CSExporter : BaseExporter
{
	public CSExporter(SheetData sheetData)
		: base(sheetData)
	{
	}

	public override void ExportFile(string path, string createLogo)
	{
		string filePath = StringHelper.MakeSaveFullPath(path, $"Cfg{_sheet.FileName}.cs");
		using (FileStream fs = new FileStream(filePath, FileMode.Create))
		using (StreamWriter sw = new StreamWriter(fs))
		{
			WriteNamespace(sw);

			// Table类
			WriteTabCalss(sw);
			sw.WriteLine("\t{");
			WriteTabClassMember(sw, createLogo);
			sw.WriteLine();
			WriteTabClassFunction(sw, createLogo);
			sw.WriteLine("\t}");
			sw.WriteLine();

			// Config类
			WriteCfgAttribute(sw);
			WriteCfgClass(sw);
			sw.WriteLine("\t{");
			WriteCfgClassFunction(sw);
			sw.WriteLine("\t}");

			WriteNamespaceEnd(sw);
		}
	}
	private void WriteNamespace(StreamWriter sw)
	{
		sw.WriteLine("//--------------------------------------------------");
		sw.WriteLine("// 自动生成  请勿修改");
		sw.WriteLine("// 研发人员实现LANG多语言接口");
		sw.WriteLine("//--------------------------------------------------");

		sw.WriteLine("using MotionFramework.IO;");
		sw.WriteLine("using MotionFramework.Config;");	
		sw.WriteLine("using System.Collections.Generic;");
		sw.WriteLine();

		string namespaceName = SettingConfig.Instance.GetNamespace(nameof(CSExporter));
		if (string.IsNullOrEmpty(namespaceName) == false)
		{
			sw.WriteLine($"namespace {namespaceName}");
			sw.WriteLine("{");
		}
	}
	private void WriteTabCalss(StreamWriter sw)
	{
		string tChar = "\t";
		sw.WriteLine(tChar + $"public class Cfg{_sheet.FileName}Table : ConfigTable");
	}
	private void WriteTabClassMember(StreamWriter sw, string createLogo)
	{
		string ttChar = "\t\t";
		string protectedChar = " { protected set; get; }";

		for (int i = 0; i < _sheet.Heads.Count; i++)
		{
			HeadWrapper head = _sheet.Heads[i];

			if (head.IsNotes || head.Logo.Contains(createLogo) == false)
				continue;

			// 跳过ID
			if (head.Name == ConstDefine.StrHeadId)
				continue;

			// 变量名称首字母大写
			string headName = StringHelper.ToUpperFirstChar(head.Name);

			if (head.Type == "int" || head.Type == "List<int>" ||
				head.Type == "long" || head.Type == "List<long>" ||
				head.Type == "float" || head.Type == "List<float>" ||
				head.Type == "double" || head.Type == "List<double>" ||
				head.Type == "string" || head.Type == "List<string>" ||
				head.Type == "bool")
			{
				sw.WriteLine(ttChar + $"public {head.Type} " + headName + protectedChar);
			}
			else if (head.Type == "language")
			{
				sw.WriteLine(ttChar + $"public string " + headName + protectedChar);
			}
			else if (head.Type == "List<language>")
			{
				sw.WriteLine(ttChar + $"public List<string> " + headName + protectedChar);
			}
			else if (head.Type.Contains("enum") || head.Type.Contains("class"))
			{
				string extendType = StringHelper.GetExtendType(head.Type);
				sw.WriteLine(ttChar + $"public {extendType} " + headName + protectedChar);
			}
			else
			{
				throw new Exception($"Not support head type {head.Type}");
			}
		}
	}
	private void WriteTabClassFunction(StreamWriter sw, string createLogo)
	{
		string ttChar = "\t\t";
		string tttChar = "\t\t\t";

		sw.WriteLine(ttChar + "public override void ReadByte(ByteBuffer byteBuf)");
		sw.WriteLine(ttChar + "{");

		for (int i = 0; i < _sheet.Heads.Count; i++)
		{
			HeadWrapper head = _sheet.Heads[i];

			if (head.IsNotes || head.Logo.Contains(createLogo) == false)
				continue;

			// 变量名称首字母大写
			string headName = StringHelper.ToUpperFirstChar(head.Name);

			// HashCode
			if (head.Name == ConstDefine.StrHeadId && head.Type == "string")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadUTF().GetHashCode();");
				continue;
			}

			if (head.Type == "bool")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadBool();");
			}
			else if(head.Type == "int")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadInt();");
			}
			else if (head.Type == "long")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadLong();");
			}
			else if (head.Type == "float")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadFloat();");
			}
			else if (head.Type == "double")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadDouble();");
			}

			else if (head.Type == "List<int>")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadListInt();");
			}
			else if (head.Type == "List<long>")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadListLong();");
			}
			else if (head.Type == "List<float>")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadListFloat();");
			}	
			else if (head.Type == "List<double>")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadListDouble();");
			}

			else if (head.Type == "string")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadUTF();");
			}
			else if (head.Type == "List<string>")
			{
				sw.WriteLine(tttChar + $"{headName} = byteBuf.ReadListUTF();");
			}

			// NOTE：多语言在字节流会是哈希值
			else if (head.Type == "language")
			{
				sw.WriteLine(tttChar + $"{headName} = LANG.Convert(byteBuf.ReadInt());");
			}
			else if (head.Type == "List<language>")
			{
				sw.WriteLine(tttChar + $"{headName} = LANG.Convert(byteBuf.ReadListInt());");
			}

			else if (head.Type.Contains("enum"))
			{
				string extendType = StringHelper.GetExtendType(head.Type);
				sw.WriteLine(tttChar + $"{headName} = StringConvert.IndexToEnum<{extendType}>(byteBuf.ReadInt());");
			}
			else if (head.Type.Contains("class"))
			{
				string extendType = StringHelper.GetExtendType(head.Type);
				sw.WriteLine(tttChar + $"{headName} = {extendType}.Parse(byteBuf);");
			}
			else
			{
				throw new Exception($"Not support head type {head.Type}");
			}
		}

		sw.WriteLine(ttChar + "}");
	}
	private void WriteCfgAttribute(StreamWriter sw)
	{
		string tChar = "\t";
		sw.WriteLine(tChar + $"[ConfigAttribute(nameof(EConfigType.{_sheet.FileName}))]");
	}
	private void WriteCfgClass(StreamWriter sw)
	{
		string tChar = "\t";
		sw.WriteLine(tChar + $"public partial class Cfg{_sheet.FileName} : AssetConfig");
	}
	private void WriteCfgClassFunction(StreamWriter sw)
	{
		string ttChar = "\t\t";
		string tttChar = "\t\t\t";

		sw.WriteLine(ttChar + "protected override ConfigTable ReadTable(ByteBuffer byteBuffer)");
		sw.WriteLine(ttChar + "{");
		sw.WriteLine(tttChar + $"Cfg{_sheet.FileName}Table table = new Cfg{_sheet.FileName}Table" + "();");
		sw.WriteLine(tttChar + "table.ReadByte(byteBuffer);");
		sw.WriteLine(tttChar + "return table;");
		sw.WriteLine(ttChar + "}");
	}
	private void WriteNamespaceEnd(StreamWriter sw)
	{
		string namespaceName = SettingConfig.Instance.GetNamespace(nameof(CSExporter));
		if (string.IsNullOrEmpty(namespaceName) == false)
		{
			sw.WriteLine("}");
		}
	}
}
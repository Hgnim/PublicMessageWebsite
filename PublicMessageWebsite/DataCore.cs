using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static PublicMessageWebsite.DataCore;

namespace PublicMessageWebsite {
	internal struct PInfo {
		internal const string version = "1.4.3.20250101";
		internal const string version_addV = $"V{version}";
		internal const string copyright = "Copyright (C) 2024-2025 Hgnim, All rights reserved.";
		internal const string githubUrl = "https://github.com/Hgnim/PublicMessageWebsite";
		internal const string githubUrl_addHead = $"Github: {githubUrl}";
	}
#pragma warning disable IDE0079
#pragma warning disable CA2211 // 非常量字段应当不可见
#pragma warning restore IDE0079
	public struct DataCore {
		public struct DataFiles {
			public static DataFile.ConfigFile config=new() {
				Config = new() {
					WebTitle= "公共留言页面",
					WebIcon="/img/icon.png",
					Title= "欢迎来到公共留言页面，在此留下你想说的话:",
					BottomText =$@"请勿发送任何违法内容！\\\n{PInfo.githubUrl_addHead}",
					OneIpAddMessageFrequency =5,
					ApiOutputMsgDay = 1
				},
				Website = new() {
					Addr = "*",
					UrlRoot = "/",
					Port = 80,
					UseHttps = false,
					UseXFFRequestHeader = false
				},
				DebugOutput = false,
				UpdateConfig = false
			};
		}

		public struct UseUrlValue {
			public static bool IsHttps => DataFiles.config.Website.UseHttps;
			public static string Addr => DataFiles.config.Website.Addr;
			///<summary>
			///urlRoot不能只包含单独的斜杠，这里只是起到占位的作用，到引用的时候单独的斜杠会被去掉。<br/>
			///在包含内容的时候，urlRoot前面必须包含斜杠，末尾不能含有斜杠<br/>
			///在读取文件时，UrlRoot的值就已经被格式化了
			///</summary>
			public static string UrlRoot => DataFiles.config.Website.UrlRoot;
			public static int Port => DataFiles.config.Website.Port;
			public static string Get() {
				string head;

				if (IsHttps)
					head = "https";
				else
					head = "http";


				return $"{head}://{Addr}:{Port}";
			}
		}
	}
	public struct DataFile {
		/// <summary>
		/// 将配置数据保存至配置文件中
		/// </summary>
		internal static void SaveData() {
			ISerializer yamlS = new SerializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();
			File.WriteAllText(FilePath.configFile, yamlS.Serialize(DataCore.DataFiles.config));
		}
		/// <summary>
		/// 读取数据文件并将数据写入实例中
		/// </summary>
		internal static void ReadData() {
			IDeserializer yamlD = new DeserializerBuilder()
				.IgnoreUnmatchedProperties()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.Build();

			DataCore.DataFiles.config = yamlD.Deserialize<ConfigFile>(File.ReadAllText(FilePath.configFile));
		}
		public struct ConfigFile {
			public struct ConfigModel {
				public required string WebTitle { get; set; }
				public required string WebIcon { get; set; }
				public required string Title { get; set; }
				public required string BottomText { get; set; }
				/// <summary>
				/// 每一个ip在当前周期内可添加留言的次数
				/// </summary>
				public required int OneIpAddMessageFrequency { get; set; }
				/// <summary>
				/// API输出留言包含的天数，可输出历史多少天的留言
				/// </summary>
				public required int ApiOutputMsgDay { get; set; }
			}
			public required ConfigModel Config { get; set; }	
			public struct WebsiteModel {
				public required string Addr { get; set; }
				private string urlRoot;
				public required string UrlRoot {
					readonly get =>urlRoot; 
					//对UrlRoot的值进行格式化
					set {
						string urlRoot_;
						if (value == "/") urlRoot_ = "";//urlRoot不能只包含单独的斜杠
						else if (value == "") { urlRoot_ = value; }//如果为空则直接输出
						else {
							if (value[..1] == "/")
								urlRoot_ = value;
							else//如果开头没有斜杠则加上斜杠
								urlRoot_ = "/" + value;
							if (urlRoot_.Substring(urlRoot_.Length - 1, 1) == "/")//如果末尾包含斜杠则去掉
								urlRoot_ = urlRoot_[..(urlRoot_.Length - 1)];
						}
						urlRoot= urlRoot_;
					} }
				public required int Port { get; set; }
				public required bool UseHttps { get; set; }
				/// <summary>
				/// 是否启用 X-Forwarded-For(XFF)请求标头
				/// </summary>
				public required bool UseXFFRequestHeader { get; set; }
			}
			public required WebsiteModel Website { get; set; }
			public required bool DebugOutput { get; set; }
			public required bool UpdateConfig { get; set; }
		}
	}
	internal struct UrlPath {
		internal static string RootUrl => UseUrlValue.UrlRoot + "/";
		internal static string SubmitMsg => UseUrlValue.UrlRoot + "/InputMsg/SubmitMsg";
		internal static string BackMain => $"{UseUrlValue.UrlRoot}/InputMsg/BackMain";
		internal static string SendSucceed => $"{UseUrlValue.UrlRoot}/InputMsg/SendSucceed";
		internal struct Img {
			internal static string Icon { 
				get {
					if (DataFiles.config.Config.WebIcon[..4] == "http") 
						return DataFiles.config.Config.WebIcon;
					else
					return $"{UseUrlValue.UrlRoot}{DataFiles.config.Config.WebIcon}"; 
				} 
			}
		}
		internal struct Css {
			private static string RootDir => $"{UseUrlValue.UrlRoot}/css/";
			internal struct InputMsg {
				private static string Dir => $"{RootDir}InputMsg/";
				internal static string Index => $"{Dir}Index.css";
			}
		}
		internal struct Js {
			private static string RootDir => $"{UseUrlValue.UrlRoot}/js/";
			internal struct InputMsg {
				private static string Dir => $"{RootDir}InputMsg/";
				internal static string Index => $"{Dir}Index.js";
			}
		}
	}

	internal struct FilePath {
		internal const string dataDir = "pmw_data/";
		internal const string logDir = dataDir + "logs/";
		internal const string messageDir = dataDir + "messages/";

		internal const string configFile_Obsolete = dataDir + "config.xml";
		internal const string configFile = dataDir + "config.yml";
		internal static string LogFile{
			get=> $"{logDir}{DateTime.Now:yyyy-MM-dd}.log";
		}
		internal static string MessageFile(DateTime? date = null) {
			if (date == null) date = DateTime.Now;
			return $"{messageDir}{date:yyyy-MM-dd}.xml";
		}
	}
}

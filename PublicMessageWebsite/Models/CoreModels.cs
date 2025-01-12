using System.Xml;
using static PublicMessageWebsite.FilePath;
using static PublicMessageWebsite.DataCore;
using System.Diagnostics.CodeAnalysis;

namespace PublicMessageWebsite.Models
{
    public class CoreModel
    {
		public struct FileEditer
		{
			static string GetNowTime()
			{
				return DateTime.Now.ToString("MM/dd HH:mm:ss:fff");
			}
			static StreamWriter? logWriter;
			/// <summary>
			/// 用来表示当前文件的编号，按日期中的“日”来进行编号，一个文件记录一天的信息
			/// </summary>
			static int logFile_time = -1;
			public static void LogWriter_Reload()
			{
				logFile_time = DateTime.Now.Day;
				logWriter?.Close();
				logWriter = new(LogFile,true);
			}
			public async static void LogAddAsync(string log)
			{
				if(DateTime.Now.Day!=logFile_time) LogWriter_Reload();//如果当前日期不等于日志的日期编号，则新建日志
				string outputMsg = $"[{GetNowTime()}] {log}";				
				if(DataCore.DataFiles.config.DebugOutput)
					Console.WriteLine(outputMsg);
				await logWriter?.WriteLineAsync(outputMsg)!;
				await logWriter?.FlushAsync()!;
			}
			static XmlDocument? msgfile_xmlDoc;
			static	XmlNode? msgfile_message_xmlRoot;
			static XmlNode? msgfile_ip_xmlRoot;
			static string? openedXmlFile;
			/// <summary>
			/// 用来表示当前文件的编号，按日期中的“日”来进行编号，一个文件记录一天的信息
			/// </summary>
			static int msgFile_time= -1;
			public static void MsgFileWriter_Reload()
			{
				msgFile_time = DateTime.Now.Day;
				openedXmlFile=MessageFile();
				if (!File.Exists(openedXmlFile))
				{
					XmlTextWriter xmlWriter = new(openedXmlFile, System.Text.Encoding.GetEncoding("utf-8")) { Formatting = System.Xml.Formatting.Indented };
					xmlWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
					xmlWriter.WriteStartElement("PublicMessageWebsite");
					xmlWriter.WriteStartElement("Message");
					xmlWriter.WriteEndElement();
					xmlWriter.WriteStartElement("IP");
					xmlWriter.WriteEndElement();
					xmlWriter.WriteFullEndElement();
					xmlWriter.Close();
				}
				msgfile_xmlDoc = new();
				msgfile_xmlDoc.Load(openedXmlFile);
				msgfile_message_xmlRoot = msgfile_xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Message")!;
				msgfile_ip_xmlRoot= msgfile_xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("IP")!;
			}
			/// <summary>
			/// 留言增加到数据文件中
			/// </summary>
			/// <param name="message"></param>
			/// <param name="name"></param>
			/// <param name="ip"></param>
			/// <returns>
			/// 返回值代表执行状况<br/>
			/// 0: 添加成功
			/// 1: 当前ip的添加次数已达上限
			/// </returns>
			public static int MessageAdd(string message,string name, string ip)
			{
				int ipAddMsgNum =0;//每个ip一个周期内留言的次数
			 	XmlNodeList msgfile_ip_xmlNL = msgfile_ip_xmlRoot!.ChildNodes;
				XmlElement xmlEle;
				foreach (XmlNode xn in msgfile_ip_xmlNL)
				{
					xmlEle = (XmlElement)xn;
					if (ip == xmlEle.GetAttribute("ip"))
						ipAddMsgNum =int.Parse(xmlEle.GetAttribute("num"));
				}
				if (ipAddMsgNum < DataFiles.config.Config.OneIpAddMessageFrequency)
				{
					if (ipAddMsgNum == 0)
					{
						xmlEle = msgfile_xmlDoc!.CreateElement("item");
						xmlEle.SetAttribute("ip", ip);
						xmlEle.SetAttribute("num", "1");
						msgfile_ip_xmlRoot.AppendChild(xmlEle);
					}
					else
					{
						ipAddMsgNum++;
						foreach (XmlNode xn in msgfile_ip_xmlNL)
						{
							xmlEle = (XmlElement)xn;
							if (ip == xmlEle.GetAttribute("ip"))
								xmlEle.SetAttribute("num",ipAddMsgNum.ToString());
						}
					}

					if (DateTime.Now.Day != msgFile_time) MsgFileWriter_Reload();
					xmlEle = msgfile_xmlDoc!.CreateElement("item");
					xmlEle.SetAttribute("message", message);
					xmlEle.SetAttribute("name", name);
					xmlEle.SetAttribute("ip", ip);
					xmlEle.SetAttribute("time", GetNowTime());
					msgfile_message_xmlRoot?.AppendChild(xmlEle);
					msgfile_xmlDoc?.Save(openedXmlFile!);
					return 0;
				}
				else return 1;
			}
			/// <summary>
			/// 随机获取留言文本信息
			/// </summary>
			/// <param name="format">api输出格式</param>
			/// <returns></returns>
			public static string MessageRandomGet(string format = "来自\"{0}\"({1})的留言: {2}")
			{
				XmlDocument msgReadDoc;
				XmlNode msgReadRoot;
				XmlNodeList msgfile_msg_xmlNL;
				XmlElement xmlEle=null!;

				if (DataFiles.config.Config.ApiOutputMsgDay != 1)
				{
					List<int> randomFileIndex = [];
					while (!(randomFileIndex.Count >= DataFiles.config.Config.ApiOutputMsgDay))
					{
						int randomNum = new Random().Next(DataFiles.config.Config.ApiOutputMsgDay);
						bool isHave = false;//表示是否已包含
						foreach (int i in randomFileIndex)
						{
							if (i == randomNum)
							{
								isHave = true;
								break;
							}
						}
						if (isHave == false)//如果不重复，则将该数字添加至数组中
							randomFileIndex.Add(randomNum);
					}
					bool isFind = false;//是否已找到可用留言
					foreach (int fileIndex in randomFileIndex)
					{
						try
						{
							DateTime nowDt = DateTime.Now.AddDays(-fileIndex);
							msgReadDoc = new();
							msgReadDoc.Load(MessageFile(nowDt));
							msgReadRoot = msgReadDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Message")!;

							if( RunRandomItem()==0)
							{
								isFind=true;
								break;
							}
						}
						catch { }
					}
					if (!isFind) goto nothing;
				}
				else
				{
					msgReadRoot = msgfile_message_xmlRoot!;

					if(RunRandomItem()==1)
						goto nothing;
				}
				int RunRandomItem()
				{
					msgfile_msg_xmlNL = msgReadRoot!.ChildNodes;
					if (msgfile_msg_xmlNL.Count > 0)
					{
						xmlEle = (XmlElement)msgfile_msg_xmlNL.Item(new Random().Next(msgfile_msg_xmlNL.Count))!;
						return 0;
					}else return 1;
				}
				if(xmlEle!=null)
					return string.Format(format,
						xmlEle.GetAttribute("name"),
						DateTime.ParseExact(xmlEle.GetAttribute("time"), "MM/dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture).ToString("MM-dd HH:mm:ss"),
						xmlEle.GetAttribute("message")
						);
			nothing:;
				return "(未找到留言)";
			}

			[method: SetsRequiredMembers]
			internal struct MessageInfo() {
				internal required string? Message { get; set; } = null;
				internal required string? Name { get; set; } = null;
				internal required DateTime? Time { get; set; } = null;
				internal required string? Ip { get; set; } = null;
			}
			/// <summary>
			/// 获取所有有效期内的留言信息
			/// </summary>
			/// <param name="getIp">是否获取IP信息</param>
			/// <returns></returns>
			internal static MessageInfo[] GetAllMessage(bool getIp=false) {
				XmlDocument xmlDoc;
				XmlNode xmlRoot;
				XmlNodeList xmlNL;
				List<MessageInfo> miList = [];

				for(int i=0;i< DataFiles.config.Config.ApiOutputMsgDay; i++) {
					try {
						xmlDoc = new();
						xmlDoc.Load(MessageFile(DateTime.Now.AddDays(-i)));
						xmlRoot = xmlDoc.SelectSingleNode("PublicMessageWebsite")?.SelectSingleNode("Message")!;
						xmlNL = xmlRoot.ChildNodes;
						XmlElement xmlE;
						foreach (XmlNode xn in xmlNL) {
							xmlE = (XmlElement)xn;
							MessageInfo mi = new() {
								Message = xmlE.GetAttribute("message"),
								Name = xmlE.GetAttribute("name"),
								Time = DateTime.ParseExact(xmlE.GetAttribute("time"), "MM/dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture),
							};
							if (getIp)
								mi.Ip = xmlE.GetAttribute("ip");
							miList.Add(mi);
						}
					} catch { }
				}
				return [.. miList];
			}
		}

			
    }
}
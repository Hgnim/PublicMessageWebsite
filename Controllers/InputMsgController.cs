using Microsoft.AspNetCore.Mvc;
using static PublicMessageWebsite.Models.CoreModel;

namespace PublicMessageWebsite.Controllers
{
    public class InputMsgController : Controller
	{

        public IActionResult Index()
        {
			FileEditer.LogAdd($"[{GetClientIP()}]获取[InputMsg/Index]页面");            
            return View("Index");
        }

        [Route("InputMsg/SendSucceed")]
        public IActionResult SendSucceed()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SubmitMsg(string inputBox,string nameBox)
        {
            int msgaddBackValue = -1;
			if (inputBox == null ||inputBox=="")
            {
				ViewBag.outputText = "发送失败，留言不能为空";
            }
            else if (nameBox == null || nameBox=="")
            {
				ViewBag.outputText = "发送失败，署名不能为空";
            }
            else
            {
               msgaddBackValue=  FileEditer.MessageAdd(inputBox, nameBox, GetClientIP());
				ViewBag.outputText = msgaddBackValue switch
				{
					0 => "发送成功",
					1 => "发送失败，已达到留言发送上限",
					_ => "发生未知错误",
				};
			}
            if (msgaddBackValue!=0)
            {
                ViewBag.inputBoxValue = inputBox;
                ViewBag.nameBoxValue = nameBox;
            }
            FileEditer.LogAdd
            ($"[{GetClientIP()}]点击了[InputMsg/Index]页面的发送按钮。发送内容: {{留言: {inputBox}; 署名: {nameBox}}}。返回代码: {msgaddBackValue}");
            if(msgaddBackValue==0)
                return Redirect(UseUrlValue.GetUrlRoot()+"/InputMsg/SendSucceed");
            else
            return View("Index");            
        }
        [HttpPost]
        public IActionResult BackMain()
        {
            return Redirect(UseUrlValue.GetUrlRoot() + "/");
        }
        [Route("api")]
        public string Api()
        {
            return FileEditer.MessageGet();
        }

        string GetClientIP()
        {
            if (Config.useXFFRequestHeader)
                return Request.Headers["X-Forwarded-For"].ToString();
            else
                return HttpContext.Connection.RemoteIpAddress!.ToString();
		}
    }
}

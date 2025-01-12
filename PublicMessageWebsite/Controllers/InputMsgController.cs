using Microsoft.AspNetCore.Mvc;
using static PublicMessageWebsite.Models.CoreModel;
using static PublicMessageWebsite.DataCore;
using PublicMessageWebsite.Models;
using static PublicMessageWebsite.Models.InputMsgModels;

namespace PublicMessageWebsite.Controllers
{
    public class InputMsgController : Controller
	{

        public IActionResult Index()
        {
			FileEditer.LogAddAsync($"[{GetClientIP()}]获取[InputMsg/Index]页面");            
            return View("Index");
        }
        [HttpPost]
        public IActionResult SubmitMsg([FromBody] SubmitMsgModel data)
        {
            int retValue=-1;
			if (data.InputBoxValue is null or "")
            {
                retValue = 2;//发送失败，留言不能为空
				goto ret;
			}
            else if (data.NameBoxValue is null or "")
            {
				retValue = 3;//发送失败，署名不能为空
                goto ret;
            }
            else
            {
               switch( FileEditer.MessageAdd(data.InputBoxValue, data.NameBoxValue, GetClientIP())) {
                    case 0:
						retValue = 0 ;//发送成功
                        goto ret;
					case 1:
						retValue = 1;//发送失败，已达到留言发送上限
                        goto ret;
                }
			}

        ret:;
            FileEditer.LogAddAsync
            ($"[{GetClientIP()}]点击了[InputMsg/Index]页面的发送按钮。发送内容: {{留言: {data.InputBoxValue}; 署名: {data.NameBoxValue}}}。返回代码: {retValue}");

			return Json(new { value = retValue});//未知错误
		}
        [Route("api")]
        public string Api()
        {
			FileEditer.LogAddAsync($"[{GetClientIP()}]调用了留言信息获取的api");
			return FileEditer.MessageGet();
        }

		string GetClientIP() => 
            DataFiles.config.Website.UseXFFRequestHeader
				? Request.Headers["X-Forwarded-For"].ToString()
				: HttpContext.Connection.RemoteIpAddress!.ToString();
	}
}

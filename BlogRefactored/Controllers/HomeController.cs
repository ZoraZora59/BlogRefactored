﻿using System;
using System.Collections;
using System.Web.Mvc;
using BlogBLL.App_Code;
using BlogBLL.ViewModels;
using BlogModel;

namespace BlogRefactored.Controllers
{
	public class HomeController : Controller
	{
        private BlogBLL.BlogGuests home;
        public HomeController(BlogBLL.BlogGuests blogGuests)
        {
            this.home = blogGuests;
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //任何home界面的share都要看到Config的信息、Session中的用户信息
            var currentLoginUser = Session["loginuser"] == null ? null : (BlogUser)Session["loginuser"];
            ViewBag.currentLoginInfo = currentLoginUser;

            base.OnActionExecuting(filterContext);
            var model = new SerializeTool().DeSerialize<BlogConfig>();
            ViewBag.Config = model;
        }
        //page分页Num
        #region 主界面
        public ActionResult Index(int? page)
        {
            
            return View(home.GetIndex(page));
            
        }
        public ActionResult MIndex(int? page)
		{
			return View(home.GetIndex(page));
		}
		public ActionResult MBlog(int? id)
		{
			var currentLoginUser = Session["loginuser"] == null ? null : (BlogUser)Session["loginuser"];
			ViewBag.currentLoginInfo = currentLoginUser;
			int textID;
			if (id == null)
				return Redirect("/home/mindex");
			else
			{
				textID = (int)id;
			}
			try
			{
				var model = home.GetBlog(textID);
				var cmt = home.GetBlogComment(textID);
				ViewBag.CmtList = cmt;
				return View(model);
			}
			catch (Exception)
			{
				return Redirect("/home/mindex");
			}
		}
        #endregion

        #region 用户相关
        [HttpGet]
        public ActionResult Register()//注册的页面显示
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterUser model)//注册信息提交
        {
            if (ModelState.IsValid)
            //判断是否验证通过
            {
                string sessionValidCode = Session["validatecode"] == null ? string.Empty : Session["validatecode"].ToString();
                if (!model.Code.Equals(sessionValidCode))
                {
                    return RedirectToAction("Register", "Home", new { msg = "验证码错误！请重新输入" });
                }
                try
                {
                    if (!home.Regist(model))
                    {
                        return RedirectToAction("Register", "Home", new { msg = "注册失败！可能已存在用户" });
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("Register", "Home", new { msg = "注册失败!您输入的格式有误" });
                }
            }
            return RedirectToAction("Login", "home", new { msg = "注册成功！请登录！" });
        }

        [HttpGet]
        public ActionResult Login()//登录的页面显示
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUser model)//登录信息提交
        {
            if (ModelState.IsValid)
            {
                string sessionValidCode = Session["validatecode"] == null ? string.Empty : Session["validatecode"].ToString();
                if (!model.Code.Equals(sessionValidCode))
                {
                    return RedirectToAction("Login", "Home", new { msg = "验证码错误！请重新输入" });
                }
                BlogUser LoginModel = home.Login(model);
                if (LoginModel == null)
                {
                    return RedirectToAction("Login", "Home", new { msg = "账号或密码不正确，是否重新登陆？" });

                }
                else
                {
                    Session["loginuser"] = LoginModel;
                    return Redirect("/");
                }
            }
            return View();
        }


        [HttpGet]
        public ActionResult ChangeInfo()
        {
            if (Session["loginuser"] == null)
                return Redirect("/home");
            try
            {
                var currentLoginUser = (BlogUser)Session["loginuser"];
                ViewBag.currentLoginInfo = currentLoginUser;
                return View();
            }
            catch (Exception)
            {
                return Redirect("/home");
            }

        }
        [HttpPost]
        public ActionResult ChangeInfo(ChangeUserInfo model)
        {
            if (ModelState.IsValid)
            //判断是否验证通过
            {
                string sessionValidCode = Session["validatecode"] == null ? string.Empty : Session["validatecode"].ToString();
                var currentLoginUser = Session["loginuser"] == null ? null : (BlogUser)Session["loginuser"];
                if (!model.Code.Equals(sessionValidCode))
                {
                    return RedirectToAction("ChangeInfo", "Home", new { msg = "验证码错误！请重新输入" });
                }
                model.Account = currentLoginUser.Account;
                home.ChangeInfo(model);//完成数据修改
                BlogUser LoginModel = home.Login(new LoginUser { Account = model.Account,Password=model.Password});
                Session["loginuser"] = LoginModel;
            }
            return RedirectToAction("index", "Home", new { Cmsg = "修改成功！" });
        }


        #endregion

        #region 搜索相关
        public ActionResult SearchResult(string searchthing)
        {

            //搜索
            //var search_list = new List<TextIndex>();
            var search_list = home.SearchBlog(searchthing);
            ViewBag.searching = searchthing;
            ViewBag.searchRes = search_list;
            return View(search_list);
        }
        public ActionResult CategroyBlog(string categroyname)
        {

            //搜索
            //var search_list = new List<TextIndex>();
            var search_list = home.SearchBlogBycate(categroyname);
            ViewBag.searchRes = search_list;
            return View(search_list);
        }
        #endregion
        
        #region 博文相关


        [HttpGet]
        public ActionResult Blog(int? id)
        {
            var currentLoginUser = Session["loginuser"] == null ? null : (BlogUser)Session["loginuser"];
            ViewBag.currentLoginInfo = currentLoginUser;
			int tid;
			if (id == null)
			{
				return Redirect("/home");
			}
			else
			{
				tid = (int)id;
			}
			try
			{
				var model = home.GetBlog(tid);
				var cmt = home.GetBlogComment(tid);
				var viewmodel = new ViewBlog
				{
					TextID = model.TextID,
					TextTitle = model.TextTitle,
					TextChangeDate = model.TextChangeDate,
					Hot=model.Hot,
					Text = model.Text,
					PreID = model.PreID,
					NexID = model.NexID,
					FirstView = model.FirstView,
					CategoryName = model.CategoryName
				};
				//此处参数length用于修改博文显示时前后文标题的长度限制
				int length = 18;
				if (viewmodel.PreID != 0)
				{
					viewmodel.PreTitle = home.GetBlogFree(model.PreID).TextTitle;
					if (viewmodel.PreTitle.Length > length)
						viewmodel.PreTitle = viewmodel.PreTitle.Substring(0, length) + "...";
				}
				if (viewmodel.NexID != 0)
				{
					viewmodel.NexTitle = home.GetBlogFree(model.NexID).TextTitle;
					if (viewmodel.NexTitle.Length > length)
						viewmodel.NexTitle = viewmodel.NexTitle.Substring(0, length) + "...";
				}
				ViewBag.CmtList = cmt;
				return View(viewmodel);
			}
			catch (Exception)
			{
				return Redirect("/home");
			}
        }
		

        [HttpPost]
        public JsonResult AddComment()//新增评论
        {
			try
			{
				int sTextID = int.Parse(Request["TextID"]);
				string sAccount = Request["Account"].ToString();
				string sContent = Request["Content"].ToString();
				#region 屏蔽词
				string[] badwords = { "你妈逼", "操你妈", "傻逼", "臭傻逼", "滚你妈的", "扯犊子","你他妈","FUCK","FUCKYOU" };
				#endregion
                for(int i = 0; i < badwords.Length; i++)
                {
                    sContent = sContent.Replace(badwords[i], "喵喵喵");
                }
                home.AddComment(sTextID, sAccount, sContent);
            }
			catch (Exception)
			{
				return Json(null);
			}
			return Json(0);
		}

        public JsonResult DeleteComment()
        {
			try
			{
				int cmtId = int.Parse(Request["CommitID"]);
				home.DelComment(cmtId);
				return Json(0);
			}
			catch (Exception)
			{
				return Json(null);
			}
            
        }
        #endregion

        




        public FileResult ValidateCode()
        {
            ValidateCode vc = new ValidateCode();
            string code = vc.CreateValidateCode(4);
            Session["validatecode"] = code;//把数字保存在session中
            byte[] bytes = vc.CreateValidateGraphic(code);//根据数字转成二进制图片
            return File(bytes, @"image/jpeg");//返回一个图片jpg
        }

        // 退出登陆
        public ActionResult ExitLogin()
        {
            Session["loginuser"] = null;

            return Redirect("/");
        }

		public ActionResult Http404()
		{
			return View();
		}
	}
}
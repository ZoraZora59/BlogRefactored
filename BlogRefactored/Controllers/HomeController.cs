﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlogBLL;
using BlogBLL.App_Code;
using BlogBLL.ViewModels;
using BlogModel;
using BlogRefactored.Models;

namespace BlogRefactored.Controllers
{
	public class HomeController : Controller
	{
        private BlogBLL.BlogGuests home;
        public HomeController(BlogBLL.BlogGuests blogGuests)
        {
            this.home = blogGuests;
        }
        public ActionResult Index()
		{
			return View();
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
        public ActionResult Index(int? page)
        {

            return View(home.GetIndex(page));
            
        }


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
            return RedirectToAction("index", "home", new { msg = "注册成功！请登录！" });
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
                if (home.Login(model) == null)
                {
                    return RedirectToAction("Login", "Home", new { msg = "账号或密码不正确，是否重新登陆？" });

                }
                else
                {
                    Session["loginuser"] = home.Login(model);
                    return Redirect("/");
                }
            }
            return View();
        }

        public FileResult ValidateCode()
        {
            ValidateCode vc = new ValidateCode();
            string code = vc.CreateValidateCode(4);
            Session["validatecode"] = code;//把数字保存在session中
            byte[] bytes = vc.CreateValidateGraphic(code);//根据数字转成二进制图片
            return File(bytes, @"image/jpeg");//返回一个图片jpg
        }

        public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";
			//TODO:设置每页显示的文章数量
			int ViewCount = 3;
			var manager = new BlogBLL.BlogBLL();
			var Texts = manager.GetAllTexts().Select(Text => new TextDetailViewModel()
			{
				TextID = Text.TextID,
				TextTitle = Text.TextTitle,
				TextChangeDate=Text.TextChangeDate,
				CategoryName=Text.CategoryName,
				Hot=Text.Hot,
				Text=Text.Text
			}
			).ToList();
			var TextsList = new TextsListViewModel()
			{
				TextsCount = Texts.Count,
				//TODO:添加显示方案
				PageCount = 1,
				Pages = 1,
				Texts = Texts
			};
			return View(TextsList);
		}
	}
}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using BlogBLL.ViewModels;
using BlogBLL.App_Code;

namespace BlogBLL
{
	public class BlogManager : IBLL
	{
		private BlogDAL.BlogDAL repository = new BlogDAL.BlogDAL();

		public ManageMain GetManageIndex()//获取管理主页数据
		{
			int hot = 0;
			foreach (var item in repository.GetTextsAll())//统计点击量
			{
				hot += item.Hot;
			}
			var Mmain = new ManageMain//汇总其他数据
			{
				UserCount = repository.GetUsersAll().Count(),
				TextCount = repository.GetTextsAll().Count(),
				CommentCount = repository.GetCommentsAll().Count(),
				HotCount = hot
			};
			return Mmain;
		}
		public List<ManageComment> GetManageComments()//获取评论列表数据
		{
			List<ManageComment> CommentsList = new List<ManageComment>();
			List<BlogModel.BlogComment> trans = repository.GetCommentsAll();
			List<BlogModel.BlogUser> tempUsersList = repository.GetUsersAll();
			foreach (var item in trans)
			{
				ManageComment temp = new ManageComment
				{
					Account = item.Account,
					Id = item.CommmentID,
					Name = tempUsersList.Find(c => c.Account == item.Account).Name,
					TextId = item.TextID,
					Content = item.CommentText,
					Date = item.CommentChangeDate.ToString()
				};
				CommentsList.Add(temp);
			}
			return CommentsList;
		}
		public List<ManageCategory> GetManageCategories()//获取分类列表数据
		{
			List<ManageCategory> Categories = new List<ManageCategory>();
			List<BlogModel.BlogText> blogTexts = repository.GetTextsAll();
			foreach(var item in blogTexts)
			{
				if(item.CategoryName==null)
				{
					item.CategoryName = "未分类";
				}
				ManageCategory addOne = new ManageCategory
				{
					CategoryName = item.CategoryName,
					CategoryHot = item.Hot,
					TextCount = 1
				};
				if(!Categories.Exists(c=>c.CategoryName==addOne.CategoryName))
				{
					Categories.Add(addOne);
				}
				else
				{
					var temp = Categories.Find(c => c.CategoryName == addOne.CategoryName);
					temp.CategoryHot += addOne.CategoryHot;
					temp.TextCount += addOne.TextCount;
				}
			}
			Categories = Categories.OrderByDescending(c => c.CategoryHot).ToList();
			return Categories;
		}
		public List<ManageText> GetManageTexts()//获取文章列表数据
		{
			List<ManageText> manageTexts = new List<ManageText>();
			var trans = repository.GetTextsAll();
			foreach (var item in trans)
			{
				ManageText temp = new ManageText
				{
					TextID = item.TextID,
					TextTitle = item.TextTitle,
					CategoryName = item.CategoryName,
					TextChangeDate = item.TextChangeDate.ToString(),
					Hot = item.Hot
				};
				manageTexts.Add(temp);
			}
			return manageTexts;
		}
		public List<ManageUser> GetManageUsers()//获取用户列表数据
		{
			List<ManageUser> manageUsers = new List<ManageUser>();
			var trans = repository.GetUsersAll();
			trans.Remove(trans.Find(c => c.Account == "admin123"));
			foreach(var item in trans)
			{
				ManageUser temp = new ManageUser
				{
					Account = item.Account,
					Name = item.Name,
					CommmentCount = 0
				};
				var cmtlist = repository.GetCommentsAll().Where(c=>c.Account==temp.Account).ToList();
				foreach(var cmt in cmtlist)
				{
					temp.CommmentCount++;
				}
				manageUsers.Add(temp);
			}
			return manageUsers;
		}
		public UpdateText GetTextInUpdate(int tid)//获取待编辑博文的内容
		{
			try
			{
				var uTxt = repository.GetTextsAll().Find(c => c.TextID == tid);
				UpdateText updateText = new UpdateText
				{
					Id = uTxt.TextID,
					Category = uTxt.CategoryName,
					Text = uTxt.Text,
					Title = uTxt.TextTitle
				};
				return updateText;
			}
			catch (System.ArgumentNullException)//前后端不同步时返回空
			{
				return null;
			}
		}
		public DetailCategory GetCategoryDetail(string categoryName)//获取分类详情
		{
			DetailCategory category = new DetailCategory();
			category.Texts = new List<TextsBelone>();
			category.Category = GetManageCategories().Find(c => c.CategoryName == categoryName);
			var texts = repository.GetTextsAll().Where(c=>c.CategoryName==categoryName);
			foreach(var item in texts)
			{
				var textsBelong = new TextsBelone
				{
					TextID = item.TextID,
					TextTitle = item.TextTitle,
					Hot = item.Hot,
					ChangeTime = item.TextChangeDate.ToString()
				};
				category.Texts.Add(textsBelong);
			}
			return category;
		}
		public BlogModel.BlogText GetTextInDetail(int tid)//获取文章详情
		{
			try
			{
				return repository.GetTextByID(tid);
			}
			catch
			{
				return null;
			}
		}
		public BlogConfig GetBlogConfig()//获取博客配置信息
		{
			return new SerializeTool().DeSerialize<BlogConfig>();
		}

		public bool SetBlogConfig(BlogConfig cfg)//设定博客配置文件
		{
			bool isSuccess = false;
			try
			{
				new SerializeTool().Serialize<BlogConfig>(cfg);
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}
		public bool RenameCategory(string oldName,string  newName)//分类重命名
		{
			bool isSuccess = false;
			//if (oldName == string.Empty)
			//	oldName = null;
			try
			{
				var txtList = repository.GetTextsAll().Where(c => c.CategoryName == oldName).ToList();
				foreach (var item in txtList)
				{
					item.CategoryName = newName;
					repository.UpdateText(item.TextID, item);
				}
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}
		public bool RemoveCategory(string name)//删除分类
		{
			return RenameCategory(null, name);
		}
		public bool RemoveText(int tid)//删除博文
		{
			bool isSuccess = false;
			try
			{
				repository.DelText(tid);
				repository.DelCommentByTextID(tid);
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}
		public bool RemoveUser(string account)//删除用户
		{
			bool isSuccess = false;
			try
			{
				repository.DelCommentByAccount(account);
				repository.DelUser(account);
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}
		public bool UpdateText(UpdateText blogText)//更新文章
		{
			bool isSuccess = false;
			try
			{
				BlogModel.BlogText blog = new BlogModel.BlogText
				{
					Text = blogText.Text,
					FirstView = getFirstView(blogText.Text),
					TextTitle = blogText.Title,
					CategoryName = blogText.Category
				};
				if (blogText.Id==0)//新增文章
				{
					repository.AddText(blog);
				}
				else//修改文章
				{
					blog.TextID = blogText.Id;
					repository.UpdateText(blog.TextID, blog);
				}
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}
		public bool RemoveComment(int cid)//删除评论
		{
			bool isSuccess = false;
			try
			{
				repository.DelCommentByID(cid);
				isSuccess = true;
			}
			catch
			{
			}
			return isSuccess;
		}

		private string getFirstView(string content)//获取摘要
		{
			content = Regex.Replace(content, "<[^>]+>", "");
			content = Regex.Replace(content, "&[^;]+;", "");
			if (content.Length < 201)
				return content;
			else
				content = content.Substring(0, 200);
			return content;
		}

		#region KindEditor文件上传
		public bool UploadFiles(HttpPostedFileBase imgFile, string dirPath , string dirName)//文件上传
		{
			bool isSuccess = false;
			//文件保存目录路径
			//通过Controller传入		String savePath = "/attached/";  TODO:在Controller中附加到dirPath字符串里
			//文件保存目录URL
			string saveUrl = "/attached/";
			//定义允许上传的文件扩展名
			Hashtable extTable = new Hashtable
			{
				{ "image", "gif,jpg,jpeg,png,bmp" },
				{ "flash", "swf,flv" },
				{ "media", "swf,flv,mp3,wav,wma,wmv,mid,avi,mpg,asf,rm,rmvb" },
				{ "file", "doc,docx,xls,xlsx,ppt,htm,html,txt,zip,rar,gz,bz2" }
			};
			//最大文件大小
			int maxSize = 1000000;
			//通过Controller传入		HttpPostedFileBase imgFile = Request.Files["imgFile"];
			if (imgFile == null)
			{
				return isSuccess;
			}
			//通过Controller传入		String dirPath = Server.MapPath(savePath);
			if (!Directory.Exists(dirPath))
			{
				return isSuccess;
			}
			//通过Controller传入		String dirName = Request.QueryString["dir"];
			if (string.IsNullOrEmpty(dirName))
			{
				dirName = "image";
			}
			if (!extTable.ContainsKey(dirName))
			{
				return isSuccess;
			}
			string fileName = imgFile.FileName;
			string fileExt = Path.GetExtension(fileName).ToLower();
			if (imgFile.InputStream == null || imgFile.InputStream.Length > maxSize)
			{
				return isSuccess;
			}
			if (string.IsNullOrEmpty(fileExt) || Array.IndexOf(((string)extTable[dirName]).Split(','), fileExt.Substring(1).ToLower()) == -1)
			{
				return isSuccess;
			}
			//创建文件夹
			dirPath += dirName + "/";
			saveUrl += dirName + "/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
			string ymd = DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
			dirPath += ymd + "/";
			saveUrl += ymd + "/";
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
			string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
			string filePath = dirPath + newFileName;
			imgFile.SaveAs(filePath);
			string fileUrl = saveUrl + newFileName;
			Hashtable hash = new Hashtable
			{
				["error"] = 0,
				["url"] = fileUrl
			};
			return isSuccess;
		}
		#endregion
	}
}
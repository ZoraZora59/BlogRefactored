﻿using BlogBLL.ViewModels;
using BlogBLL.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;
using BlogModel;

namespace BlogBLL
{
	public class BlogGuests:IBLL
	{
		private BlogDAL.BlogDAL repository = new BlogDAL.BlogDAL();
        public bool Regist (ViewModels.RegisterUser addOne)
        {
            var isRegist = false;
            var res = repository.GetUserByAccount(addOne.Account);
            if (res == null)
            {
				BlogUser NewBlog = new BlogUser
				{
					Account = addOne.Account,
					Password = md5tool.GetMD5(addOne.Password),
					Name = addOne.Name
				};
				repository.AddUser(NewBlog);
                isRegist = true;
                return isRegist;
            }
            return isRegist;
            
        }//注册
        public BlogModel.BlogUser Login (ViewModels.LoginUser LogOne)
        {

            
            LogOne.Password = md5tool.GetMD5(LogOne.Password);
			BlogUser temp = repository.GetUserByAccount(LogOne.Account);
            if (temp == null)
            {
                return temp;
            }
            if (temp.Password == LogOne.Password)
            {
                return temp;
            }
            return null;
        }//登录

        public object GetIndex(int? page)
        {
            var models = new List<TextIndex>();
            List<BlogModel.BlogText> TextList = repository.GetTextsAll();
            foreach (var item in TextList)
            {
                var temp = new TextIndex
                {
                    TextID = item.TextID,
                    CommitCount = repository.GetCommitsByTextID(item.TextID).Count(),
                    Text = item.Text,
                    FirstView = item.FirstView
                };
                if (item.CategoryName == null)
                    item.CategoryName = "未分类";
                temp.CategoryName = item.CategoryName;
                temp.TextTitle = item.TextTitle;
                temp.TextChangeDate = item.TextChangeDate;
                temp.Hot = item.Hot;
                models.Add(temp);
            }
            models.Reverse();
            int pageSize = 10;//每页显示的文章数
            int pageNumber = (page ?? 1);
            return (models.ToPagedList(pageNumber, pageSize));
        }//首页，page实现分页

        public List<ShowComment> GetBlogComment(int id)
        {
            //评论模块
            var temp = repository.GetCommitsByTextID(id);
            var cmt = new List<ShowComment>();
            int i = 1;
            foreach (var item in temp)
            {
                var tmp = new ShowComment();
                tmp.Name = repository.GetUserByAccount(item.Account).Name;
                tmp.Date = item.CommentChangeDate.ToString("yyyy-MM-dd") + "  " + item.CommentChangeDate.ToShortTimeString();
                tmp.Account = item.Account;
                tmp.Content = item.CommentText;
                tmp.Id = item.CommmentID;
                tmp.Num = i;
                cmt.Add(tmp);
                i++;
            }
            return cmt;
        }//根据文章id,获取文章的评论

        public  BlogText GetBlog(int id)
        {
            repository.AddTextHot(id);
            var model = repository.GetTextByID(id);
            return model;
        }
		public BlogText GetBlogFree(int id)
		{
			return repository.GetTextByID(id);
		}

        public List<TextIndex> SearchBlog(string searchthing)//搜索博文
        {
            var blog = repository.GetTextsAll();
            var search_list = new List<TextIndex>();
            if (!string.IsNullOrEmpty(searchthing))
            {
                var s_blogs = blog.Where(m => m.TextTitle.Contains(searchthing) || m.Text.Contains(searchthing)).ToList();
                foreach (var item in s_blogs)
                {
                    var temp = new TextIndex();
                    temp.TextID = item.TextID;
                    temp.CommitCount = repository.GetCommitsByTextID(item.TextID).Count();
                    temp.Text = item.Text;
                    if (item.CategoryName == null)
                        item.CategoryName = "未分类";
                    temp.CategoryName = item.CategoryName;
                    temp.FirstView = item.FirstView;
                    temp.TextTitle = item.TextTitle.Replace(searchthing, "<font color='red'>" + searchthing + "</font>");
                    temp.TextChangeDate = item.TextChangeDate;
                    temp.Hot = item.Hot;
                    search_list.Add(temp);
                }
            }
            return search_list;
        }

        public void ChangeInfo(ChangeUserInfo model)
        {
            BlogUser odlModel = repository.GetUserByAccount(model.Account);
            odlModel.Name = model.Name;
            odlModel.Account = model.Account;
            odlModel.Password = md5tool.GetMD5(model.Password);
            repository.UpdateUser(model.Account, odlModel);
        }

        public List<TextIndex> SearchBlogBycate(string categroyname)
        {
            var blog = repository.GetTextsAll();
            var search_list = new List<TextIndex>();
            if (!string.IsNullOrEmpty(categroyname))
            {
                var c_blogs = blog.Where(m => m.CategoryName.Equals(categroyname)).ToList();
                foreach (var item in c_blogs)
                {
					if (item.CategoryName == null)
						item.CategoryName = "未分类";
					var temp = new TextIndex
					{
						TextID = item.TextID,
						CommitCount = repository.GetCommitsByTextID(item.TextID).Count(),
						Text = item.Text,
						TextTitle = item.TextTitle,
						CategoryName = item.CategoryName,
						TextChangeDate = item.TextChangeDate,
						FirstView = item.FirstView,
						Hot = item.Hot
					};
					search_list.Add(temp);
                }
            }
            return search_list;
        }

        #region 评论相关
        public void AddComment(int id,string Account,string Content)
        {
            var NewComment = new BlogComment { TextID = id, Account = Account, CommentText = Content };
            repository.AddComment(NewComment);
        }

        public void DelComment(int cmtId)
        {
            repository.DelCommentByID(cmtId);
        }

        #endregion
    }
}

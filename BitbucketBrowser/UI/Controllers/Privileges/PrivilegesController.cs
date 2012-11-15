using System;
using CodeFramework.UI.Controllers;
using System.Collections.Generic;
using BitbucketSharp.Models;
using MonoTouch.Dialog;
using CodeFramework.UI.Elements;

namespace BitbucketBrowser.UI.Controllers.Privileges
{
    public class PrivilegesController : Controller<List<PrivilegeModel>>
    {
        public event Action<PrivilegeModel> SelectedItem;

        public string Username { get; set; }

        //Null if looking for user only privileges
        public string RepoSlug { get; set; }

        public UserModel Primary { get; set; }

        protected void OnSelectedItem(PrivilegeModel model)
        {
            var handler = SelectedItem;
            if (handler != null)
                handler(model);
        }

        public PrivilegesController()
            : base(true, true)
        {
            Style = MonoTouch.UIKit.UITableViewStyle.Plain;
            Title = "Privileges";
            EnableSearch = true;
            AutoHideSearch = true;
            SearchPlaceholder = "Search Users";
        }

        protected override void OnRefresh()
        {
            var sec = new Section();
            if (Primary != null)
            {
                StyledElement primaryElement = new UserElement(Primary.Username, Primary.FirstName, Primary.LastName, Primary.Avatar);
                primaryElement.Tapped += () => OnSelectedItem(new PrivilegeModel { Privilege = "admin", User = Primary });
                sec.Add(primaryElement);
            }

            Model.ForEach(s =>
            {
                StyledElement sse = new UserElement(s.User.Username, s.User.FirstName, s.User.LastName, s.User.Avatar);
                sse.Tapped += () => OnSelectedItem(s);
                sec.Add(sse);
            });

            if (sec.Count == 0)
                sec.Add(new NoItemsElement());

            InvokeOnMainThread(delegate
            {
                var root = new RootElement(Title) { sec };
                Root = root;
            });
        }

        protected override List<PrivilegeModel> OnUpdate(bool forced)
        {
            try
            {
                if (RepoSlug != null)
                    return Application.Client.Users[Username].Repositories[RepoSlug].Privileges.GetPrivileges(forced);
                return Application.Client.Users[Username].Privileges.GetPrivileges(forced);
            }
            catch (Exception)
            {
                return new List<PrivilegeModel>();
            }
        }
    }
}


﻿using System;
using UIKit;
using System.Collections.Generic;
using ReactiveUI;
using System.Threading.Tasks;
using System.Linq;
using Foundation;
using System.Diagnostics;
using CoreGraphics;
using CodeBucket.Core.Services;
using CodeBucket.Views;

namespace CodeBucket.Services
{
    public class ActionMenuService : IActionMenuService
    {
        public IActionMenu Create(string title = null)
        {
            return new ActionMenu(title);
        }

        public IPickerMenu CreatePicker()
        {
            return new PickerMenu();
        }

        public void OpenIn(object sender, string file)
        {
            var ctrl = new UIDocumentInteractionController();
            ctrl.Url = NSUrl.FromFilename(file);
            ctrl.PresentOpenInMenu(sender as UIBarButtonItem, true);
        }

        public void ShareUrl(object sender, Uri uri)
        {
            var item = new NSUrl(uri.AbsoluteUri);
            var activityItems = new NSObject[] { item };
            UIActivity[] applicationActivities = null;
            var activityController = new UIActivityViewController (activityItems, applicationActivities);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) 
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var pop = new UIPopoverController (activityController);

                var barButtonItem = sender as UIBarButtonItem;
                if (barButtonItem != null)
                {
                    pop.PresentFromBarButtonItem(barButtonItem, UIPopoverArrowDirection.Any, true);
                }
                else
                {
                    var rect = new CGRect(window.RootViewController.View.Frame.Width / 2, window.RootViewController.View.Frame.Height / 2, 0, 0);
                    pop.PresentFromRect (rect, window.RootViewController.View, UIPopoverArrowDirection.Any, true);
                }
            } 
            else 
            {
                var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                viewController.PresentViewController(activityController, true, null);
            }
        }

        public void SendToPasteBoard(string str)
        {
            UIPasteboard.General.String = str;
        }

        private class ActionMenu : IActionMenu
        {
            private readonly string _title;
            private readonly IList<Tuple<string, Action<object>>> _buttonActions = new List<Tuple<string, Action<object>>>();

            public ActionMenu(string title)
            {
                _title = title;
            }

            public void AddButton(string title, Action<object> command)
            {
                _buttonActions.Add(Tuple.Create(title, command));
            }

            public Task Show(object sender)
            {
                var a = new TaskCompletionSource<object>();
                var sheet =  UIAlertController.Create(_title, null, UIAlertControllerStyle.ActionSheet);
                var cleanup = new Action<UIAlertAction>(y => {
                    a.SetResult(true);
                    sheet.Dispose();
                });

                foreach (var b in _buttonActions)
                {
                    sheet.AddAction(UIAlertAction.Create(b.Item1, UIAlertActionStyle.Default, x => {
                        b.Item2.Invoke(sender);
                        cleanup(x);
                    }));
                }

                sheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, cleanup));

                var viewController = UIApplication.SharedApplication.KeyWindow.GetVisibleViewController();

                if (sheet.PopoverPresentationController != null)
                {
                    var uiBarButton = sender as UIBarButtonItem;
                    var view = sender as UIView;

                    if (uiBarButton != null)
                        sheet.PopoverPresentationController.BarButtonItem = uiBarButton;
                    else if (view != null)
                        sheet.PopoverPresentationController.SourceView = view;

                    // Last resort
                    if (sheet.PopoverPresentationController.SourceView == null
                        && sheet.PopoverPresentationController.BarButtonItem == null)
                    {
                        Debugger.Break();
                        sheet.PopoverPresentationController.SourceView = viewController.View;
                    }
                }

                viewController.PresentViewController(sheet, true, null);
                return a.Task;
            }
        }

        private class PickerMenu : IPickerMenu
        {
            private readonly LinkedList<string> _options = new LinkedList<string>();

            public ICollection<string> Options
            {
                get { return _options; }
            }

            public int SelectedOption { get; set; }

            public Task<int> Show(object sender)
            {
                var a = new TaskCompletionSource<int>();

                new PickerAlertView(_options.ToArray(), SelectedOption, x =>
                {
                    if (x < _options.Count)
                        a.SetResult((int)x);
                }).Show();

                return a.Task;
            }
        }
    }
}

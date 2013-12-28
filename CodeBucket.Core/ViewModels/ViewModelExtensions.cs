using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeBucket.Core.Services;
using System.Collections.Generic;

namespace CodeBucket.Core.ViewModels
{
    public static class ViewModelExtensions
    {
		public static async Task RequestModel<TRequest>(this MvxViewModel viewModel, Func<TRequest> request, Action<TRequest> update) where TRequest : new()
        {
			var data = await Task.Run(() => request());
			update(data);
//
//            if (forceDataRefresh)
//            {
//                request.CheckIfModified = false;
//                request.RequestFromCache = false;
//            }
//
//			var response = await Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(request);
//            update(response);
//
//            if (response.WasCached)
//            {
//				Task.Run(async () => {
//                    try
//                    {
//                        request.RequestFromCache = false;
//						var r = await Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(request);
//						update(r);
//                    }
//                    catch (NotModifiedException)
//                    {
//                        Console.WriteLine("Not modified: " + request.Url);
//                    }
//					catch (Exception)
//                    {
//                        Console.WriteLine("SHIT! " + request.Url);
//                    }
//                });
//            }
        }

		public static void CreateMore<T>(this MvxViewModel viewModel, BitbucketSharp.Models.V2.Collection<T> response, 
										 Action<Task> assignMore, Action<IEnumerable<T>> newDataAction)
        {
			if (string.IsNullOrEmpty(response.Next))
            {
                assignMore(null);
                return;
            }

			var task = new Task(async () => 
				{
					var moreResponse = await Task.Run(() => Mvx.Resolve<IApplicationService>().Client.Request2<BitbucketSharp.Models.V2.Collection<T>>(response.Next));
                    viewModel.CreateMore(moreResponse, assignMore, newDataAction);
					newDataAction(moreResponse.Values);
            	});

			assignMore(task);
        }

		public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, Func<List<T>> request)
        {
			return viewModel.RequestModel(request, response => {
				//viewModel.CreateMore(response, m => viewModel.MoreItems = m, viewModel.Items.AddRange);
                viewModel.Items.Reset(response);
            });
        }

		public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, Func<BitbucketSharp.Models.V2.Collection<T>> request)
		{
			return viewModel.RequestModel(request, response => {
				viewModel.CreateMore(response, m => viewModel.MoreItems = m, viewModel.Items.AddRange);
				viewModel.Items.Reset(response.Values);
			});
		}
    }
}


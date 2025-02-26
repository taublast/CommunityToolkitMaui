using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

namespace CommunityToolkit.Maui.Views;

/// <summary>
/// Extension methods for <see cref="Popup"/>.
/// </summary>
public static partial class PopupExtensions
{
	/// <summary>
	/// Displays a popup.
	/// </summary>
	/// <param name="page">The current <see cref="Page"/>.</param>
	/// <param name="popup">The <see cref="Popup"/> to display.</param>
	public static void ShowPopup<TPopup>(this Page page, TPopup popup) where TPopup : Popup
	{
#if WINDOWS
		// TODO: This is a workaround for https://github.com/dotnet/maui/issues/12970. Remove this `#if Windows` block when the issue is closed
		page.GetCurrentPage().Loaded += handler;

		void handler(object? sender, EventArgs args)
		{
			ArgumentNullException.ThrowIfNull(sender);

			var page = (Page)sender;

			page.GetCurrentPage().Loaded -= handler;

			CreateAndShowPopup(page, popup);
		}
#else
		if (page.IsPlatformEnabled)
		{
			CreateAndShowPopup(page, popup);
		}
		else
		{
			void handler(object? sender, EventArgs args)
			{
				page.Loaded -= handler;

				CreateAndShowPopup(page, popup);
			}

			page.Loaded += handler;
		}
#endif
	}

	/// <summary>
	/// Displays a popup and returns a result.
	/// </summary>
	/// <param name="page">The current <see cref="Page"/>.</param>
	/// <param name="popup">The <see cref="Popup"/> to display.</param>
	/// <param name="token"><see cref="CancellationToken"/></param>
	/// <returns>
	/// A task that will complete once the <see cref="Popup"/> is dismissed.
	/// </returns>
	public static Task<object?> ShowPopupAsync<TPopup>(this Page page, TPopup popup, CancellationToken token = default) where TPopup : Popup
	{
#if WINDOWS
		// TODO: This is a workaround for https://github.com/dotnet/maui/issues/12970. Remove this `#if Windows` block when the issue is closed   
		var taskCompletionSource = new TaskCompletionSource<object?>();

		page.GetCurrentPage().Unloaded += unloadedHandler;
		page.GetCurrentPage().Loaded += loadedHandler;

		return taskCompletionSource.Task.WaitAsync(token);

		static void unloadedHandler(object? sender, EventArgs args) { }

		async void loadedHandler(object? sender, EventArgs args)
		{
			ArgumentNullException.ThrowIfNull(sender);

			var page = (Page)sender;

			page.Unloaded -= unloadedHandler;
			page.Loaded -= loadedHandler;

			try
			{
				var result = await CreateAndShowPopupAsync(page, popup, token);

				taskCompletionSource.TrySetResult(result);
			}
			catch (Exception ex)
			{
				taskCompletionSource.TrySetException(ex);
			}
		}
#else
		if (page.IsPlatformEnabled)
		{
			return CreateAndShowPopupAsync(page, popup, token);
		}
		else
		{
			var taskCompletionSource = new TaskCompletionSource<object?>();

			async void handler(object? sender, EventArgs args)
			{
				page.Loaded -= handler;

				try
				{
					var result = await CreateAndShowPopupAsync(page, popup, token);

					taskCompletionSource.TrySetResult(result);
				}
				catch (Exception ex)
				{
					taskCompletionSource.TrySetException(ex);
				}
			}

			page.Loaded += handler;

			return taskCompletionSource.Task;
		}
#endif
	}

	static void CreatePopup(Page page, Popup popup)
	{
		var mauiContext = GetMauiContext(page);

		var parent = page.GetCurrentPage();
		parent?.AddLogicalChild(popup);

		var platformPopup = popup.ToHandler(mauiContext);
		platformPopup.Invoke(nameof(IPopup.OnOpened));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static IMauiContext GetMauiContext(Page page)
	{
		return page.Handler?.MauiContext ?? throw new InvalidOperationException("Could not locate MauiContext.");
	}

	static void CreateAndShowPopup<TPopup>(Page page, TPopup popup) where TPopup : Popup
	{
		CreatePopup(page, popup);
	}

	static Task<object?> CreateAndShowPopupAsync<TPopup>(this Page page, TPopup popup, CancellationToken token) where TPopup : Popup
	{
		CreatePopup(page, popup);

		return popup.Result.WaitAsync(token);
	}
}
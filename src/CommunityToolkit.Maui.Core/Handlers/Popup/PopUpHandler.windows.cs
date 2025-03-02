using CommunityToolkit.Maui.Core.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace CommunityToolkit.Maui.Core.Handlers;

public partial class PopupHandler : ElementHandler<IPopup, MauiPopup>
{

	/// <summary>
	/// Action that's triggered when the Popup is Dismissed.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">The result that should return from this Popup.</param>
	public static void MapOnClosed(PopupHandler handler, IPopup view, object? result)
	{
		var window = view.GetWindow();
		if (window.Overlays.FirstOrDefault() is IWindowOverlay popupOverlay)
		{
			window.RemoveOverlay(popupOverlay);
		}

		view.HandlerCompleteTCS.TrySetResult();
		handler.DisconnectHandler(handler.PlatformView);
	}

	/// <summary>
	/// Action that's triggered when the Popup is Opened.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">We don't need to provide the result parameter here.</param>
	public static void MapOnOpened(PopupHandler handler, IPopup view, object? result)
	{
		ArgumentNullException.ThrowIfNull(view.Parent);
		ArgumentNullException.ThrowIfNull(handler.MauiContext);

		var parent = view.Parent.ToPlatform(handler.MauiContext);
		parent.IsHitTestVisible = false;
		handler.PlatformView.XamlRoot = view.GetWindow().Content?.Handler?.MauiContext?.GetPlatformWindow().Content.XamlRoot ?? throw new InvalidOperationException("Window Content cannot be null");
		handler.PlatformView.IsHitTestVisible = true;

		handler.PlatformView.Show();
	}


	/// <summary>
	/// Action that's triggered when the Popup is dismissed by tapping outside of the Popup.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">The result that should return from this Popup.</param>
	public static void MapOnDismissedByTappingOutsideOfPopup(PopupHandler handler, IPopup view, object? result)
	{
		view.OnDismissedByTappingOutsideOfPopup();
		handler.DisconnectHandler(handler.PlatformView);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Anchor"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapAnchor(PopupHandler handler, IPopup view)
	{
		handler.PlatformView.SetAnchor(view, handler.MauiContext);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.CanBeDismissedByTappingOutsideOfPopup"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapCanBeDismissedByTappingOutsideOfPopup(PopupHandler handler, IPopup view)
	{
		handler.PlatformView.CanBeDismissedByTappingOutside = view.CanBeDismissedByTappingOutsideOfPopup;
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Color"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapColor(PopupHandler handler, IPopup view)
	{
		handler.PlatformView.SetColor(view);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.BackgroundColor"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapBackgroundColor(PopupHandler handler, IPopup view)
	{
		handler.PlatformView.SetBackgroundColor(view);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Size"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapSize(PopupHandler handler, IPopup view)
	{
		handler.PlatformView.Layout();
	}

	/// <inheritdoc/>
	protected override void DisconnectHandler(MauiPopup platformView)
	{
		if (VirtualView.Parent is null)
		{
			return;
		}

		platformView.SetElement(null);

		ArgumentNullException.ThrowIfNull(VirtualView.Handler?.MauiContext);
		var parent = VirtualView.Parent.ToPlatform(VirtualView.Handler.MauiContext);
		parent.IsHitTestVisible = true;
	}

	/// <inheritdoc/>
	protected override MauiPopup CreatePlatformElement()
	{
		_ = MauiContext ?? throw new InvalidOperationException("MauiContext is null, please check your MauiApplication.");

		return new MauiPopup(MauiContext);
	}

	/// <inheritdoc/>
	protected override void ConnectHandler(MauiPopup platformView)
	{
		_ = platformView.SetElement(VirtualView);

		base.ConnectHandler(platformView);
	}

}
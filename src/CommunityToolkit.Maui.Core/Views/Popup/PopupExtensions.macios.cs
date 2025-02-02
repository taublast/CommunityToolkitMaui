using Microsoft.Maui.Platform;
using ObjCRuntime;

namespace CommunityToolkit.Maui.Core.Views;
/// <summary>
/// Extension class where Helper methods for Popup lives.
/// </summary>
public static class PopupExtensions
{

#if MACCATALYST
	// https://github.com/CommunityToolkit/Maui/pull/1361#issuecomment-1736487174
	static nfloat popupMargin = 18f;
#endif

	/// <summary>
	/// Method to update the <see cref="IPopup.Size"/> of the Popup.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="MauiPopup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	public static void SetSize(this MauiPopup mauiPopup, in IPopup popup)
	{
		ArgumentNullException.ThrowIfNull(popup.Content);

		CGRect frame = UIScreen.MainScreen.Bounds;

		CGSize currentSize;

		if (popup.Size.IsZero)
		{
			if (double.IsNaN(popup.Content.Width) || double.IsNaN(popup.Content.Height))
			{
				var content = popup.Content.ToPlatform(popup.Handler?.MauiContext ?? throw new InvalidOperationException($"{nameof(popup.Handler.MauiContext)} Cannot Be Null"));
				var contentSize = content.SizeThatFits(new CGSize(double.IsNaN(popup.Content.Width) ? frame.Width : popup.Content.Width, double.IsNaN(popup.Content.Height) ? frame.Height : popup.Content.Height));
				var width = contentSize.Width;
				var height = contentSize.Height;

				if (double.IsNaN(popup.Content.Width))
				{
					width = popup.HorizontalOptions == Microsoft.Maui.Primitives.LayoutAlignment.Fill ? frame.Size.Width : width;
				}
				if (double.IsNaN(popup.Content.Height))
				{
					height = popup.VerticalOptions == Microsoft.Maui.Primitives.LayoutAlignment.Fill ? frame.Size.Height : height;
				}

				currentSize = new CGSize(width, height);
			}
			else
			{
				currentSize = new CGSize(popup.Content.Width, popup.Content.Height);
			}
		}
		else
		{
			currentSize = new CGSize(popup.Size.Width, popup.Size.Height);
		}

#if MACCATALYST
		currentSize.Width = NMath.Min(currentSize.Width, frame.Size.Width - defaultPopoverLayoutMargin * 2 - popupMargin * 2);
		currentSize.Height = NMath.Min(currentSize.Height, frame.Size.Height - defaultPopoverLayoutMargin * 2 - popupMargin * 2);
#else
		currentSize.Width = NMath.Min(currentSize.Width, frame.Size.Width);
		currentSize.Height = NMath.Min(currentSize.Height, frame.Size.Height);
#endif
		mauiPopup.PreferredContentSize = currentSize;
	}

	/// <summary>
	/// Method to update the <see cref="IPopup.Color"/> of the Popup.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="MauiPopup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	public static void SetBackgroundColor(this MauiPopup mauiPopup, in IPopup popup)
	{
		if (mauiPopup.Control is null)
		{
			return;
		}

		var color = popup.Color?.ToPlatform();
		mauiPopup.Control.PlatformView.BackgroundColor = color;

		if (mauiPopup.Control.ViewController?.View is UIView view)
		{
			view.BackgroundColor = color;
		}
	}

	/// <summary>
	/// Method to update the <see cref="IPopup.CanBeDismissedByTappingOutsideOfPopup"/> property of the Popup.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="MauiPopup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	public static void SetCanBeDismissedByTappingOutsideOfPopup(this MauiPopup mauiPopup, in IPopup popup)
	{
		mauiPopup.CanBeDismissedByTappingInternal = popup.CanBeDismissedByTappingOutsideOfPopup;
	}

	/// <summary>
	/// Method to update the layout of the Popup and <see cref="IPopup.Content"/>.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="MauiPopup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	public static void SetLayout(this MauiPopup mauiPopup, in IPopup popup)
	{
		if (mauiPopup.View is null || popup.Content is null)
		{
			return;
		}

		CGRect frame = UIScreen.MainScreen.Bounds;

#if MACCATALYST
    var titleBarHeight = mauiPopup.ViewController?.NavigationController?.NavigationBar.Frame.Y ?? 0;
    var navigationBarHeight = mauiPopup.ViewController?.NavigationController?.NavigationBar.Frame.Size.Height ?? 0;
#endif

		if (popup.Anchor is null)
		{
			// Calculate intrinsic content size.
			CGSize contentSize = mauiPopup.PreferredContentSize;
			if (!double.IsNaN(popup.Content.Width) && popup.HorizontalOptions != Microsoft.Maui.Primitives.LayoutAlignment.Fill)
			{
				contentSize.Width = (nfloat)popup.Content.Width;
			}
			if (!double.IsNaN(popup.Content.Height) && popup.VerticalOptions != Microsoft.Maui.Primitives.LayoutAlignment.Fill)
			{
				contentSize.Height = (nfloat)popup.Content.Height;
			}

#if MACCATALYST
        bool isPortrait = frame.Height >= frame.Width;
        nfloat additionalVerticalOffset = isPortrait
            ? (titleBarHeight + navigationBarHeight)
            : (titleBarHeight + navigationBarHeight) / 2;
#else
			nfloat additionalVerticalOffset = 0;
#endif

			// Compute safe area margins for each side.
			nfloat marginLeft = 0, marginTop = 0, marginRight = 0, marginBottom = 0;
			if (!popup.IgnoreSafeArea)
			{
				if (mauiPopup.View?.Window is UIWindow window)
				{
					marginLeft = window.SafeAreaInsets.Left;
					marginTop = window.SafeAreaInsets.Top;
					marginRight = window.SafeAreaInsets.Right;
					marginBottom = window.SafeAreaInsets.Bottom;
				}
			}

#if MACCATALYST
        marginLeft += popupMargin;
        marginTop += popupMargin;
        marginRight += popupMargin;
        marginBottom += popupMargin;
#endif

			// Compute the horizontal position based on the HorizontalOptions.
			nfloat x = popup.HorizontalOptions switch
			{
				Microsoft.Maui.Primitives.LayoutAlignment.Start => marginLeft,
				Microsoft.Maui.Primitives.LayoutAlignment.End => frame.Width - contentSize.Width - marginRight,
				Microsoft.Maui.Primitives.LayoutAlignment.Center or Microsoft.Maui.Primitives.LayoutAlignment.Fill => (frame.Width - contentSize.Width) / 2,
				_ => (frame.Width - contentSize.Width) / 2,
			};

			// Compute the vertical position based on the VerticalOptions.
			nfloat y = popup.VerticalOptions switch
			{
				Microsoft.Maui.Primitives.LayoutAlignment.Start => marginTop + additionalVerticalOffset,
				Microsoft.Maui.Primitives.LayoutAlignment.End => frame.Height - contentSize.Height - marginBottom - additionalVerticalOffset,
				Microsoft.Maui.Primitives.LayoutAlignment.Center or Microsoft.Maui.Primitives.LayoutAlignment.Fill => (frame.Height - contentSize.Height) / 2 - additionalVerticalOffset,
				_ => (frame.Height - contentSize.Height) / 2,
			};

			if (mauiPopup.Control?.ViewController?.View is UIView contentView)
			{
				contentView.Frame = new CGRect(x, y, contentSize.Width, contentSize.Height);
			}
		}
		else
		{
			//todo test this all on mac

			/*
			var view = popup.Anchor.ToPlatform(popup.Handler?.MauiContext ??
				throw new InvalidOperationException($"{nameof(popup.Handler.MauiContext)} Cannot Be Null"));
			mauiPopup.PopoverPresentationController.SourceView = view;
			mauiPopup.PopoverPresentationController.SourceRect = view.Bounds;
			*/

			// If an anchor is provided, position the popup relative to the anchor.
			var anchorView = popup.Anchor.ToPlatform(popup.Handler?.MauiContext
														 ?? throw new InvalidOperationException($"{nameof(popup.Handler.MauiContext)} cannot be null"));

			// Convert the anchor's frame to the coordinate space of the popup's main view.
			if (anchorView.Superview != null)
			{
				var anchorFrame = anchorView.Superview.ConvertRectToView(anchorView.Frame, mauiPopup.View);

				if (mauiPopup.Control?.ViewController?.View is UIView contentView)
				{
					// Center the content view on the anchor's center.
					contentView.Center = new CoreGraphics.CGPoint(anchorFrame.GetMidX(), anchorFrame.GetMidY());
				}
			}
		}
	}



}
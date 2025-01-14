﻿using System.ComponentModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.UnitTests.Mocks;
using CommunityToolkit.Maui.Views;
using Xunit;

namespace CommunityToolkit.Maui.UnitTests;

public class PopupServiceTests : BaseHandlerTest
{
	public PopupServiceTests()
	{
		var appBuilder = MauiApp.CreateBuilder()
			.UseMauiCommunityToolkit()
			.UseMauiApp<MockApplication>();

		var mauiApp = appBuilder.Build();
		var application = mauiApp.Services.GetRequiredService<IApplication>();
		application.Handler = new ApplicationHandlerStub();
		application.Handler.SetMauiContext(new HandlersContextStub(mauiApp.Services));
	}

	static PopupServiceTests()
	{
		var serviceCollection = new ServiceCollection();
		PopupService.AddTransientPopup<MockPopup, MockPageViewModel>(serviceCollection);
	}

	[Fact]
	public async Task ShowPopupAsyncWithNullViewModelShouldThrowArgumentNullException()
	{
		var popupService = new PopupService(new MockServiceProvider());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			popupService.ShowPopupAsync<INotifyPropertyChanged>(viewModel: null, CancellationToken.None));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact]
	public async Task ShowPopupAsyncWithNullOnPresentingShouldThrowArgumentNullException()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			popupService.ShowPopupAsync<INotifyPropertyChanged>(onPresenting: null, token: CancellationToken.None));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsync_CancellationTokenExpired()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await Task.Delay(100, CancellationToken.None);

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync<MockPageViewModel>(token: cts.Token));
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsync_CancellationTokenCanceled()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await cts.CancelAsync();

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync<MockPageViewModel>(cts.Token));
	}

	[Fact]
	public async Task ShowPopupAsyncWithMismatchedViewModelTypeShouldThrowInvalidOperationException()
	{
		var popupService = new PopupService(new MockServiceProvider());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			popupService.ShowPopupAsync<INotifyPropertyChanged>(viewModel: null, CancellationToken.None));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsyncShouldThrowInvalidOperationExceptionWhenNoViewModelIsRegistered()
	{
		var popupInstance = new MockMismatchedPopup();
		var popupViewModel = new MockPageViewModel();

		SetupTest(popupInstance, () => popupViewModel, out var popupService);

		await Assert.ThrowsAsync<InvalidOperationException>(() => popupService.ShowPopupAsync<MockPageViewModel>(CancellationToken.None));
	}

	[Fact(Timeout = (int)TestDuration.Medium)]
	public async Task ShowPopupAsyncShouldValidateProperBindingContext()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();
		var popupInstance = ServiceProvider.GetRequiredService<MockSelfClosingPopup>();
		var popupViewModel = ServiceProvider.GetRequiredService<MockPageViewModel>();

		await popupService.ShowPopupAsync<MockPageViewModel>(token: CancellationToken.None);

		Assert.Same(popupInstance.BindingContext, popupViewModel);
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsyncWithOnPresenting_CancellationTokenExpired()
	{
		var popupViewModel = new MockPageViewModel();
		var popupInstance = new MockSelfClosingPopup(popupViewModel)
		{
			BindingContext = popupViewModel
		};

		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await Task.Delay(100, CancellationToken.None);

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync<MockPageViewModel>(viewModel => viewModel.HasLoaded = true, token: cts.Token));
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsyncWithOnPresenting_CancellationTokenCanceled()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await cts.CancelAsync();

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync<MockPageViewModel>(viewModel => viewModel.HasLoaded = true, token: cts.Token));
	}

	[Fact(Timeout = (int)TestDuration.Medium)]
	public async Task ShowPopupAsyncWithOnPresentingShouldBeInvoked()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();
		var popupViewModel = ServiceProvider.GetRequiredService<MockPageViewModel>();

		await popupService.ShowPopupAsync<MockPageViewModel>(onPresenting: viewModel => viewModel.HasLoaded = true, token: CancellationToken.None);

		Assert.True(popupViewModel.HasLoaded);
	}

	[Fact(Timeout = (int)TestDuration.Medium)]
	public async Task ShowPopupAsyncShouldReturnResultOnceClosed()
	{
		var mockPopup = ServiceProvider.GetRequiredService<MockSelfClosingPopup>();
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();

		var result = await popupService.ShowPopupAsync<MockPageViewModel>(token: CancellationToken.None);

		Assert.Same(expectedResult, result);
	}

	[Fact]
	public void ShowPopupWithNullViewModelShouldThrowArgumentNullException()
	{
		var popupService = new PopupService(new MockServiceProvider());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => popupService.ShowPopup<INotifyPropertyChanged>(viewModel: null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact]
	public void ShowPopupWithNullOnPresentingShouldThrowArgumentNullException()
	{
		var popupService = new PopupService(ServiceProvider, new MockDispatcherProvider());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => popupService.ShowPopup<INotifyPropertyChanged>(onPresenting: null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact]
	public void ShowPopupWithMismatchedViewModelTypeShouldThrowInvalidOperationException()
	{
		var popupService = new PopupService(new MockServiceProvider());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => popupService.ShowPopup<INotifyPropertyChanged>(viewModel: null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsyncWithViewModel_CancellationTokenExpired()
	{
		var popupViewModel = new MockPageViewModel();
		var popupInstance = new MockSelfClosingPopup(string.Empty)
		{
			BindingContext = popupViewModel
		};

		SetupTest(popupInstance, () => popupViewModel, out var popupService);

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await Task.Delay(100, CancellationToken.None);

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync(popupViewModel, cts.Token));
	}

	[Fact(Timeout = (int)TestDuration.Short)]
	public async Task ShowPopupAsyncWithViewModel_CancellationTokenCanceled()
	{
		var popupViewModel = new MockPageViewModel();
		var popupInstance = new MockSelfClosingPopup(string.Empty)
		{
			BindingContext = popupViewModel
		};

		SetupTest(popupInstance, () => popupViewModel, out var popupService);

		var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

		// Ensure CancellationToken has expired
		await cts.CancelAsync();

		await Assert.ThrowsAsync<TaskCanceledException>(() => popupService.ShowPopupAsync(popupViewModel, cts.Token));
	}

	[Fact]
	public void ShowPopupShouldThrowInvalidOperationExceptionWhenNoViewModelIsRegistered()
	{
		var popupInstance = new MockMismatchedPopup();
		var popupViewModel = new MockPageViewModel();

		SetupTest(popupInstance, () => popupViewModel, out var popupService);

		Assert.Throws<InvalidOperationException>(popupService.ShowPopup<MockPageViewModel>);
	}

	[Fact]
	public void ShowPopupWithOnPresentingShouldBeInvoked()
	{
		var popupService = ServiceProvider.GetRequiredService<IPopupService>();
		var popupViewModel = ServiceProvider.GetRequiredService<MockPageViewModel>();

		popupService.ShowPopup<MockPageViewModel>(onPresenting: viewModel => viewModel.HasLoaded = true);

		Assert.True(popupViewModel.HasLoaded);
	}

	sealed class UnregisteredViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
	}
}

sealed class MockSelfClosingPopup: ContentView
{
	public MockSelfClosingPopup(MockPageViewModel viewModel, object? result = null)
	{
		BindingContext = viewModel;
		Result = result;
	}

	public new object? Result { get; }

	internal override async void OnOpened()
	{
		base.OnOpened();

		await Task.Delay(TimeSpan.FromMilliseconds(500));

		Close(Result);
	}
}
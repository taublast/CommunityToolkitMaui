using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls;

namespace CommunityToolkit.Maui.Views;

/// <inheritdoc cref="IExpander"/>
[ContentProperty(nameof(Content))]
public class Expander : ContentView, IExpander
{
	readonly IGestureRecognizer tapGestureRecognizer;

	readonly WeakEventManager tappedEventManager = new();

	/// <summary>
	/// Initialize a new instance of <see cref="Expander"/>.
	/// </summary>
	public Expander()
	{
		tapGestureRecognizer = new TapGestureRecognizer()
		{
			Command = new Command(() =>
			{
				IsExpanded = !IsExpanded;
				((IExpander)this).ExpandedChanged(IsExpanded);
			})
		};
	}

	/// <inheritdoc cref="IExpander.ExpandedChanged"/>
	public event EventHandler<ExpandedChangedEventArgs> ExpandedChanged
	{
		add => tappedEventManager.AddEventHandler(value);
		remove => tappedEventManager.RemoveEventHandler(value);
	}

	/// <summary>
	/// Backing BindableProperty for the <see cref="Header"/> property.
	/// </summary>
	public static readonly BindableProperty HeaderProperty
		= BindableProperty.Create(nameof(Header), typeof(IView), typeof(Expander), propertyChanged: OnHeaderPropertyChanged);

	/// <summary>
	/// Backing BindableProperty for the <see cref="Content"/> property.
	/// </summary>
	public static new readonly BindableProperty ContentProperty
		= BindableProperty.Create(nameof(Content), typeof(IView), typeof(Expander), propertyChanged: OnContentPropertyChanged);

	/// <summary>
	/// Backing BindableProperty for the <see cref="IsExpanded"/> property.
	/// </summary>
	public static readonly BindableProperty IsExpandedProperty
		= BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(Expander), default(bool), BindingMode.TwoWay, propertyChanged: OnIsExpandedPropertyChanged);

	/// <summary>
	/// Backing BindableProperty for the <see cref="Direction"/> property.
	/// </summary>
	public static readonly BindableProperty DirectionProperty
		= BindableProperty.Create(nameof(Direction), typeof(ExpandDirection), typeof(Expander), default(ExpandDirection), propertyChanged: OnDirectionPropertyChanged);

	/// <summary>
	/// Backing BindableProperty for the <see cref="CommandParameter"/> property.
	/// </summary>
	public static readonly BindableProperty CommandParameterProperty
		= BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Expander));

	/// <summary>
	/// Backing BindableProperty for the <see cref="Command"/> property.
	/// </summary>
	public static readonly BindableProperty CommandProperty
		= BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Expander));

	/// <inheritdoc />
	public IView? Header
	{
		get => (IView?)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	/// <inheritdoc />
	public new IView? Content
	{
		get => (IView?)GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	/// <inheritdoc />
	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	/// <inheritdoc />
	public ExpandDirection Direction
	{
		get => (ExpandDirection)GetValue(DirectionProperty);
		set => SetValue(DirectionProperty, value);
	}

	/// <summary>
	/// Command parameter passed to the <see cref="Command"/>
	/// </summary>
	public object? CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	/// <summary>
	/// Command to execute when <see cref="IsExpanded"/> changed.
	/// </summary>
	public ICommand? Command
	{
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	static void OnHeaderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		=> ((Expander)bindable).Configure();

	static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		=> ((Expander)bindable).Configure();

	static void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		=> ((Expander)bindable).Configure();

	static void OnDirectionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		=> ((Expander)bindable).Configure();

	void IExpander.ExpandedChanged(bool isExpanded)
	{
		if (Command is not null && Command.CanExecute(CommandParameter))
		{
			Command.Execute(CommandParameter);
		}

		tappedEventManager.HandleEvent(this, new Core.ExpandedChangedEventArgs(isExpanded), nameof(ExpandedChanged));
	}

	void Configure()
	{
		if (Header is null || Content is null)
		{
			return;
		}

		SetGestures();
		Layout();
		UpdateSize();
	}

	void Layout()
	{
		var grid = new Grid();
		grid.Children.Add(Header);
		switch (Direction)
		{
			case ExpandDirection.Down:
				grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
				grid.SetRow(Header, 0);
				if (IsExpanded)
				{
					grid.Children.Add(Content);
					grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
					grid.SetRow(Content, 1);
				}
				break;
			case ExpandDirection.Up:
				if (IsExpanded)
				{
					grid.Children.Add(Content);
					grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
					grid.SetRow(Content, 0);
					grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
					grid.SetRow(Header, 1);
				}
				else
				{
					grid.RowDefinitions.Add(new RowDefinition());
					grid.SetRow(Header, 0);
				}
				break;
		}

		base.Content = grid;
	}

	void UpdateSize()
	{
		var parent = Parent;
		while (parent is not null)
		{
			switch (parent)
			{
				case ListView listView:
					var cells = listView.AllChildren.OfType<Cell>();
					foreach (var child in cells)
					{
						child.ForceUpdateSize();
					}

					break;
				case CollectionView collectionView:
					collectionView.InvalidateMeasureInternal(Microsoft.Maui.Controls.Internals.InvalidationTrigger.MeasureChanged);
					break;
			}

			parent = parent.Parent;
		}
	}

	void SetGestures()
	{
		var header = Header as View;
		header?.GestureRecognizers.Remove(tapGestureRecognizer);
		header?.GestureRecognizers.Add(tapGestureRecognizer);
	}
}
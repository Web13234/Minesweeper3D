using Godot;
using System;

namespace Minesweeper3D.UI;
[Tool]
public partial class SliderWithBox : Control
{
    #region Property
    bool editable = true;
	[Export]
	public bool Editable
	{
		get => editable;
		set
		{
			editable = value;
			try
			{
				Slider.Editable = SpinBox.Editable = editable;
			}
			catch (NullReferenceException) { }
		}
	}
	double _value = 0;
	[Export]
	public double Value
	{
		get => _value;
		set
		{
			_value = Math.Max(MinValue, Math.Min(MaxValue, value));
			try
			{
				Slider.Value = SpinBox.Value = _value;
			}
			catch (NullReferenceException) { }
		}
	}
	double maxValue = 100;
	[Export]
	public double MaxValue
	{
		get => maxValue;
		set
		{
			maxValue = value;
			Value = Math.Min(Value, maxValue);
			try
			{
				Slider.MaxValue = SpinBox.MaxValue = maxValue;
			}
			catch(NullReferenceException) { }
		}
	}
	double minValue = 0;
	[Export]
	public double MinValue
	{
		get => minValue;
		set
		{
			minValue = value;
			Value = Math.Max(Value, minValue);
			try
			{
				Slider.MinValue = SpinBox.MinValue = minValue;
			}
			catch (NullReferenceException) { }
		}
	}
	#endregion
	#region Node
	HSlider Slider;
	SpinBox SpinBox;
	#endregion
	[Signal]
	public delegate void ValueChangedEventHandler(double value);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Slider = GetNode<HSlider>("Slider");
		SpinBox = GetNode<SpinBox>("SpinBox");
		#pragma warning disable CA2245
		Editable = Editable;
		Value = Value;
		MaxValue = MaxValue;
		MinValue = MinValue;
		#pragma warning restore CA2245
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//Signal
	public void OnValueChanged(double value)
	{
		Value = value;
		EmitSignal(nameof(ValueChanged), value);
	}
}

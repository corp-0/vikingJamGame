using System;
using Godot;

namespace VikingJamGame.GameLogic.Nodes;

public partial class GodotEventOptionButton : VBoxContainer
{
    [Export] private Button OptionDescritionButton { get; set; } = null!;
    [Export] private Label OptionCostLabel { get; set; } = null!;
    [Export] private Panel CostPanel { get; set; } = null!;

    private Action? _onPressed;

    public void Initialize(string description, string? cost = null)
    {
        OptionDescritionButton.Text = description;
        bool hasCost = cost != null;
        CostPanel.Visible = hasCost;
        if (hasCost)
        {
            OptionCostLabel.Text = cost!;
        }
    }

    public void OnPressed(Action callback)
    {
        _onPressed = callback;
        OptionDescritionButton.Pressed += HandlePressed;
    }

    public void SetDisabled(bool disabled)
    {
        OptionDescritionButton.Disabled = disabled;
    }

    private void HandlePressed()
    {
        _onPressed?.Invoke();
    }



    public override void _ExitTree()
    {
        OptionDescritionButton.Pressed -= HandlePressed;
    }
}
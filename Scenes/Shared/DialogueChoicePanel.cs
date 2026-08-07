using Godot;
using System.Collections.Generic;

public partial class DialogueChoicePanel : PanelContainer
{
    [Signal]
    public delegate void ChoiceSelectedEventHandler(int choiceIndex);

    private VBoxContainer _choiceList = null!;

    public override void _Ready()
    {
        _choiceList = GetNode<VBoxContainer>("MarginContainer/Content/ChoiceList");
        Hide();
    }

    public void ShowChoices(IReadOnlyList<string> choices)
    {
        ClearChoices();

        for (int index = 0; index < choices.Count; index++)
        {
            int capturedIndex = index;
            var button = new Button
            {
                Text = choices[index],
                CustomMinimumSize = new Vector2(0.0f, 38.0f),
                Alignment = HorizontalAlignment.Left,
                Flat = true
            };

            button.Pressed += () => SelectChoice(capturedIndex);
            _choiceList.AddChild(button);

            if (index == 0)
            {
                button.GrabFocus();
            }
        }

        Show();
    }

    private void SelectChoice(int choiceIndex)
    {
        Hide();
        EmitSignal(SignalName.ChoiceSelected, choiceIndex);
    }

    private void ClearChoices()
    {
        foreach (Node child in _choiceList.GetChildren())
        {
            _choiceList.RemoveChild(child);
            child.QueueFree();
        }
    }
}

using Godot;
using System.Collections.Generic;

public partial class KrakerScene : SceneBase
{
    private sealed record DialogueOption(
        string PlayerLine,
        string AlienLine,
        bool EndsConversation = false
    );

    private static readonly DialogueOption[] DialogueOptions =
    [
        new(
            "Hello. Who are you?",
            "I'm Zik. I write down everything interesting that happens on Kraker."
        ),
        new(
            "What are you writing in that notebook?",
            "A very serious study of visitors. So far, you are the most confusing one."
        ),
        new(
            "Thanks, Zik. I should keep moving.",
            "Safe travels! If you find anything strange, come back so I can write it down.",
            EndsConversation: true
        )
    ];

    private SceneTransitionArea _toMap = null!;
    private Alien _alien = null!;
    private AlienHotspot _alienHotspot = null!;
    private DialogueChoicePanel _dialogueChoicePanel = null!;
    private bool _dialogueLinePlaying;

    protected override void OnRoomReady()
    {
        _toMap = GetNode<SceneTransitionArea>("Transitions/ToMap");
        _alien = GetNode<Alien>("Alien");
        _alienHotspot = GetNode<AlienHotspot>("Alien/Hotspot");
        _dialogueChoicePanel = GetNode<DialogueChoicePanel>("UI/DialogueChoicePanel");

        _toMap.TransitionRequested += OnMapTransitionRequested;
        _alienHotspot.ConversationRequested += OnConversationRequested;
        _dialogueChoicePanel.ChoiceSelected += OnDialogueChoiceSelected;
    }

    public override void _ExitTree()
    {
        _toMap.TransitionRequested -= OnMapTransitionRequested;
        _alienHotspot.ConversationRequested -= OnConversationRequested;
        _dialogueChoicePanel.ChoiceSelected -= OnDialogueChoiceSelected;
    }

    private void OnConversationRequested()
    {
        if (_dialogueLinePlaying)
        {
            return;
        }

		_alien.FaceTowards(Player.GlobalPosition);
        SetInteractionEnabled(false);
        ShowDialogueChoices();
    }

    private async void OnDialogueChoiceSelected(int choiceIndex)
    {
        if (_dialogueLinePlaying || choiceIndex < 0 || choiceIndex >= DialogueOptions.Length)
        {
            return;
        }

        _dialogueLinePlaying = true;
        DialogueOption option = DialogueOptions[choiceIndex];

        Player.ShowThought(option.PlayerLine, 2.6);
        await WaitForDialogueAsync(2.8);

        if (!IsInsideTree())
        {
            return;
        }

        _alien.ShowThought(option.AlienLine, 3.5);
        await WaitForDialogueAsync(3.7);

        if (!IsInsideTree())
        {
            return;
        }

        _dialogueLinePlaying = false;

        if (option.EndsConversation)
        {
            SetInteractionEnabled(true);
            return;
        }

        ShowDialogueChoices();
    }

    private void ShowDialogueChoices()
    {
        var choices = new List<string>(DialogueOptions.Length);

        foreach (DialogueOption option in DialogueOptions)
        {
            choices.Add(option.PlayerLine);
        }

        _dialogueChoicePanel.ShowChoices(choices);
    }

    private async System.Threading.Tasks.Task WaitForDialogueAsync(double seconds)
    {
        await ToSignal(
            GetTree().CreateTimer(seconds),
            SceneTreeTimer.SignalName.Timeout
        );
    }

    private void OnMapTransitionRequested()
    {
        Callable.From(ChangeToNovaMap)
            .CallDeferred();
    }

    private void ChangeToNovaMap()
    {
        Error result = GetTree().ChangeSceneToFile(Consts.NovaMapScenePath);

        if (result != Error.Ok)
        {
            GD.PushError($"Could not change scene: {result}");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TutorialManager;

public class TutorialManager : MonoBehaviour, ITutorialManagerCallbacks
{
    public GameObject BlockPanel;
    public GameObject TutorialPanel;

    [SerializeField]
    private int TutorialTotalSteps;
    private bool NotCountsAsProgress;

    public int TutorialCurrentStep { get; private set; }

    private Dictionary<int, MessageStep> TutorialHints = new Dictionary<int, MessageStep>();
    private Dictionary<int, MessageStep> TutorialNoCountingHints = new Dictionary<int, MessageStep>();

    // Start is called before the first frame update
    public void Init()
    {
        //Messages for tutorial Progress.
        TutorialHints.Add(0, new MessageStep( 0, "Link 5 Tutorial", "."));
        TutorialHints.Add(1, new MessageStep(1, "Description", "The Board is build of numbered Tiles starting from 1 to 5 and some yellow ones."));
        TutorialHints.Add(2, new MessageStep( 2, "First turn", "In the first turn you can place a chip in any empty Tile."));
        TutorialHints.Add(3, new MessageStep(3, "Second Turn", "In your next turn you must place a chip in a Tile with a higher or equal number than the previous one."));
        TutorialHints.Add(4, new MessageStep(4, "Wild Yellow Tiles", "Yellow Tiles are wild they allows you to place your next chip in any other Tile no matters the number it has."));
        TutorialHints.Add(5, new MessageStep(5, "Link 5", "You continuously change turns with your rival. To win you must place 5 chips in a row, in any direction, horizontally, vertically or diagonally. Good Luck!"));

        //Custom Messages for specific actions, not counting as tutorial progress. Assign a index greater than 100
        TutorialNoCountingHints.Add(101, new MessageStep( 101, "", "You can't place a Chip on a Tile with a value less than the previous selected one."));
        TutorialNoCountingHints.Add(102, new MessageStep(102, "Higher Value", "You may place a Chip on a Tile with a Higher value than the previous selected."));
        TutorialNoCountingHints.Add(103, new MessageStep( 103, "Equal Value", "You may place a Chip on a Tile with a Equal value than the previous selected."));
        TutorialNoCountingHints.Add(104, new MessageStep(104, "Yellow Tile", "Place a Chip on a Yellow Tile."));

        TutorialTotalSteps = TutorialHints.Count;
        TutorialCurrentStep = 0;
    }

    public void ShowPanel(bool show = true)
    {
        TutorialPanel.SetActive(show);
        BlockPanel.SetActive(show);
        LeanTween.scale(TutorialPanel, show ? Vector3.one : Vector3.zero, 0.6f).setEase(LeanTweenType.easeOutElastic);
        if (!NotCountsAsProgress && !show)
        {
            GameManager.Instance.WaitUserMove();
        }
    }

    public void ActionTaken()
    {
        Debug.LogWarning("Action Taken in tutorialManager");
    }

    public void NextStep()
    {
        if(!NotCountsAsProgress)
        {
            TutorialCurrentStep++;
        }
        if (TutorialCurrentStep > TutorialTotalSteps)
        {
            TutorialTotalSteps = 0;
        }
        Debug.LogWarning("CurrentStep: " + TutorialCurrentStep);
    }

    public void PreviousStep()
    {
        TutorialCurrentStep--;
        if (TutorialCurrentStep < 0)
        {
            TutorialTotalSteps = 0;
        }
    }

    public MessageStep ShowMessage(int i)
    {
        MessageStep StepM = new MessageStep(-1, "Error", "This Should not happen again"); ;
        if (i > 100)
        {
            NotCountsAsProgress = true;
            TutorialNoCountingHints.TryGetValue(i, out StepM);
        }
        else 
        { 
            NotCountsAsProgress = false;
            TutorialHints.TryGetValue(i, out StepM);
        };
        return StepM;
    }

    public MessageStep GetMessage(int MessageIndex)
    {
        //If MessageIndex is Higher than 0, it means show a Custom message. else continuos with tutorial
        return ShowMessage(MessageIndex > 0 ? MessageIndex : TutorialCurrentStep);
    }

    public int CurrentStep()
    {
        return TutorialCurrentStep;
    }

    public class MessageStep
    {
        public int Index;

        public string Title;

        public string Content;
        public MessageStep(int Index, string title, string content)
        {
            this.Index = Index;
            Title = title;
            Content = content;
        }
    } 

    public interface ITutorialManagerCallbacks
    {

        void Init();

        int CurrentStep();
        void ActionTaken();
        MessageStep ShowMessage(int index);

        MessageStep GetMessage(int index);

        void NextStep();

        void PreviousStep();

    }
}

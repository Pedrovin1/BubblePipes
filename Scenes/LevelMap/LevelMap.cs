using Godot;
using System;

public partial class LevelMap : Node2D
{
    private struct Buttons
    {
        public Buttons(Node _Up, Node _Down){Up = (Button)_Up; Down = (Button)_Down; }
        public Button Up;
        public Button Down;
    }

    const string LevelFilesPath = Tabuleiro.LevelExportPath;
    int levelsAmount;
    int[] showedLevelsRange = new int[2];


    AnimationPlayer animationNode;
    Node rootLevelBoxes;
    Buttons buttons;

    public override void _Ready()
    {
        if(!Godot.DirAccess.DirExistsAbsolute(LevelMap.LevelFilesPath)) { return; };
        this.levelsAmount = Godot.DirAccess.GetFilesAt(LevelMap.LevelFilesPath).Length;

        PlayerData.self.Connect(PlayerData.SignalName.PlayerDataChanged, new Callable(this, MethodName.onPlayerDataChanged));

        this.showedLevelsRange[0] = 1;
        this.showedLevelsRange[1] = 5;

        this.animationNode = (AnimationPlayer)FindChild("AnimationPlayer");
        this.animationNode.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.onAnimationFinished));

        this.buttons = new(FindChild("UpButton"), FindChild("DownButton"));
        this.buttons.Up.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onUpButtonPressed));
        this.buttons.Down.Connect(Button.SignalName.Pressed, new Callable(this, MethodName.onDownButtonPressed));

        this.rootLevelBoxes = this.FindChild("LevelBoxes");
        this.updateLevelBoxes();
    }

    private void onUpButtonPressed()
    {
        if(this.showedLevelsRange[1] >= this.levelsAmount){ return; }

        this.stopLevelBoxesAnimation();

        this.animationNode.Play("MoverAvancarMapaNiveis");
        this.showedLevelsRange[0] += 5;
        this.showedLevelsRange[1] += 5;
    }
    private void onDownButtonPressed()
    {
        if(this.showedLevelsRange[0] <= 1){ return; }

        this.showedLevelsRange[0] -= 5;
        this.showedLevelsRange[1] -= 5;
        this.updateLevelBoxes();

        this.stopLevelBoxesAnimation();
        
        this.animationNode.PlayBackwards("MoverAvancarMapaNiveis");
    }

    private void onPlayerDataChanged()
    {
        this.updateLevelBoxes();
    }

    private void updateLevelBoxes()
    {

        for( int i = 0; i < this.rootLevelBoxes.GetChildCount(); i++)
        {
            LevelBox lbox = this.rootLevelBoxes.GetChild<LevelBox>(i);
            int levelNumber = this.showedLevelsRange[0] + i;
            lbox.SetLevelNumber(levelNumber, PlayerData.self.lastUnlockedLevel >= levelNumber);
        }
    }

    private void stopLevelBoxesAnimation()
    {
        for( int i = 0; i < this.rootLevelBoxes.GetChildCount(); i++)
        {
            LevelBox lbox = this.rootLevelBoxes.GetChild<LevelBox>(i);
            lbox.PauseAnimation();
        }
    }

    private void onAnimationFinished(string animationName)
    {
        if(animationName == "RESET"){ return; }

        this.updateLevelBoxes();

        this.animationNode.Play("RESET");
    }

}

using Godot;
using System.Collections.Generic;


public partial class Tabuleiro : GridContainer
{
    [Export]
    private bool exportLevelMode = false;
    [Export]
    private int currentLevel = 1;

    private List<int> LiquidSourceIndexes = new();
    private List<int> LiquidObjectiveIndexes = new();

    private bool levelCompleted = false;

    private bool c_connect = true;
    private Callable c_onObjectiveSlotStateChanged;
    private Callable c_onChildInteraction;
    private Callable c_onLevelSelected;

    int updateBoardStateAmount = 0;
    public static double animationWaitTime {get; set;} = 0d;
    public static bool processingBoardState {get; private set;} = false;


    public override void _Ready()
    {
        foreach(var tween in this.GetTree().GetProcessedTweens())
        {
            tween.Stop();
            tween.Kill();
        }

        this.levelCompleted = false;

        if(this.c_connect)
        {
            this.c_connect = false;
            this.c_onObjectiveSlotStateChanged = new Callable(this, MethodName.onObjectiveSlotStateChanged);
            this.c_onChildInteraction = new Callable(this, MethodName.onChildInteraction);
            this.c_onLevelSelected = new Callable(this, MethodName.onLevelSelected);

            GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.LevelSelected, this.c_onLevelSelected);
            GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.ConfigurationsMenuToggled, new Callable(this, Tabuleiro.MethodName.onConfigurationsButtonToggled));
        }
        
        this.LoadLevel(this.currentLevel);
        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.LevelLoaded);

        //DEBUG-CREATOR-MODE:------
        if(this.exportLevelMode)
        {
            this.ExportLevel();
            return;
        }
        //-------------------------

 
        this.LiquidSourceIndexes = new();
        this.LiquidObjectiveIndexes = new();
        
        foreach(Node node in this.GetChildren())
        {
            switch(node)
            {
                case BaseSource: LiquidSourceIndexes.Add(node.GetIndex()); break;
                case LiquidObjective slotObjective: 
                    this.LiquidObjectiveIndexes.Add(node.GetIndex());
                    
                    if(!slotObjective.IsConnected(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this.c_onObjectiveSlotStateChanged))
                    {
                        slotObjective.Connect(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this.c_onObjectiveSlotStateChanged);
                    }

                     foreach(int lockedIndex in slotObjective.bubbleLockedTilesIndexes)
                    {
                        this.GetChild<ISlotInteractable>(lockedIndex).LockRotation();
                    }

                    slotObjective.PlayBubbleSpreadingAnimation(this.GlobalPosition);
                    break;
            }

            var button = (Button)node;
            if(!button.IsConnected(Button.SignalName.Pressed, this.c_onChildInteraction))
            {
                button.Connect(Button.SignalName.Pressed, this.c_onChildInteraction);
            }
        }

        //this.UpdateBoardState();
    }

    public void onLevelSelected(int level)
    {
        foreach(Node node in this.GetChildren()){ node.GetChild<AnimationPlayer>(0).Play("RESET"); }

        if(level == 27) //placeholder for the version 1.0
        { 
            this.Hide();
            ((Sprite2D)this.GetOwner<Node>().FindChild("LevelCompleteSprite")).Frame = 1;
            this.GetOwner<Node>().GetChild<AnimationPlayer>(0).Play("LevelComplete");
            return;
        }

        this.currentLevel = level;
        ((Node2D)this.GetOwner<Node>().FindChild("LevelCompleteSprite")).Hide();
        this.GetOwner<Node>().GetChild<AnimationPlayer>(0).Play("levelTransition"); //it calls _Ready during animation!
        //this._Ready();
    }

    public async void onChildInteraction()
    {
        if(this.levelCompleted || this.updateBoardStateAmount > 0){ return; }

        Tabuleiro.processingBoardState = true;

        this.updateBoardStateAmount = 0;
        this.updateBoardStateAmount++; 

        while(this.updateBoardStateAmount > 0) //since chain reactions can happen.
        {
            Tabuleiro.animationWaitTime = 0;

            this.UpdateBoardState();
            this.updateBoardStateAmount--;

            if(ConfigsMenu.chainAnimations && this.updateBoardStateAmount > 0)
            {
                await ToSignal(GetTree().CreateTimer(Tabuleiro.animationWaitTime), Timer.SignalName.Timeout);
            }
        }

        Tabuleiro.processingBoardState = false;

        foreach(int index in this.LiquidObjectiveIndexes)
        {
            if(this.GetChild<LiquidObjective>(index).correctlyFilled == false)
            {
                return;
            }
        }

        GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.LevelCompleted, this.currentLevel);
        foreach(Node node in this.GetChildren()){ node.GetChild<AnimationPlayer>(0).Play("LevelCompletedAnimation"); }
            
        if(this.levelCompleted == false){ this.GetOwner<Node>().GetChild<AnimationPlayer>(0).Play("LevelComplete"); }
        this.levelCompleted = true;
    }

    private void onObjectiveSlotStateChanged(LiquidObjective objectiveSlot, bool correctlyFilled)
    {
        this.updateBoardStateAmount++;

        if(correctlyFilled)
        { 
            foreach(int index in objectiveSlot.bubbleLockedTilesIndexes)
            {
                this.GetChild<ISlotInteractable>(index).UnlockRotation();
            }
            
            objectiveSlot.PlayBubbleReleasingAnimation();
        }
        else
        {          
            foreach(int index in objectiveSlot.bubbleLockedTilesIndexes)
            {
                this.GetChild<ISlotInteractable>(index).LockRotation();
            }      

            objectiveSlot.PlayBubbleSpreadingAnimation(this.GlobalPosition);
        }
    }

    private void UpdateBoardState()
    {
        Dictionary<ISlotInteractable, List<Directions>> visitados = new();

        foreach(int sourceIndex in this.LiquidSourceIndexes)
        {
            var source = this.GetChild<BaseSource>(sourceIndex);
            foreach((Directions outletPos, SlotOutlet slotOulet) in source.outletStates)
            {
                if(slotOulet.Opened == true && 
                   isMoveInsideBounds(source.GetIndex(), outletPos, out ISlotInteractable neighborNode) &&
                   neighborNode.IsOpened(GameUtils.OppositeSide(outletPos)))
                { 
                    Stack<(ISlotInteractable, Directions, LiquidType)> proxVisita = new();
                    proxVisita.Push((neighborNode, GameUtils.OppositeSide(outletPos), source.GetLiquid(outletPos)));

                    FillPipes(visitados, proxVisita); 
                }
            }
        }

        foreach(ISlotInteractable node in this.GetChildren())
        {
            if(node is LiquidObjective objective)
            { 
                if(objective.bubbleLocked)
                {
                    node.ResetOutletLiquids(LiquidType.Vazio); 
                    continue; 
                }
            }

            if(!visitados.TryGetValue(node, out List<Directions> visitedOutlets)) 
            {
                node.ResetOutletLiquids(LiquidType.Vazio);
            }
            else
            {
                

                for(int i = 0; i < 4; i++)
                {
                    Directions outletPos = (Directions)i;
                    if(!visitedOutlets.Contains(outletPos))
                    {
                        node.SetLiquid(outletPos, LiquidType.Vazio);
                    }
                }
            }

            if(node.IsPlayingAnimation()){ continue; }
            node.UpdateDrawingState();
        }
    }

    private void FillPipes(Dictionary<ISlotInteractable, List<Directions>> visitados, Stack<(ISlotInteractable, Directions, LiquidType)> proxVisita)
    {
        while(proxVisita.Count > 0)
        {
            (ISlotInteractable currentNode, Directions outletPos, LiquidType currentLiquid) = proxVisita.Pop();

            if(!visitados.TryGetValue(currentNode, out _))
            {
                visitados.Add(currentNode, new List<Directions>());
            }

            if(visitados[currentNode].Contains(outletPos) || 
                !currentNode.IsOpened(outletPos) ||
                //(currentNode.GetLiquid(outletPos) != LiquidType.Vazio && currentNode.GetLiquid(outletPos) != currentLiquid) || //might break the game
                currentLiquid == LiquidType.Vazio)
            { 
                continue; 
            }

            currentNode.SetLiquid(outletPos, currentLiquid);
            visitados[currentNode].Add(outletPos);

            foreach(Directions connections in currentNode.GetConnections(outletPos))
            {
                if(!currentNode.IsOpened(connections)){ continue; }

                visitados[currentNode].Add(connections);
                if(isMoveInsideBounds(((Node)currentNode).GetIndex(), connections, out ISlotInteractable neighborNode))
                {
                    proxVisita.Push((neighborNode, GameUtils.OppositeSide(connections), currentNode.GetLiquid(connections)));
                }
            }

            
        }
    }

    public void onConfigurationsButtonToggled(bool toggledOn)
    {
        this.Visible = !toggledOn;
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

public partial class Tabuleiro : GridContainer
{
    [Export]
    private int currentLevel = 1;

    private List<int> LiquidSourceIndexes = new();
    private int objectiveSlotsAmount = 0;
    private int objectiveSlotsCorrectlyFilled = 0;

    private bool c_connect = true;
    private Callable c_onObjectiveSlotStateChanged;
    private Callable c_onChildInteraction;
    private Callable c_onLevelSelected;

    public override void _Ready()
    {
        if(this.c_connect)
        {
            this.c_connect = false;
            this.c_onObjectiveSlotStateChanged = new Callable(this, MethodName.onObjectiveSlotStateChanged);
            this.c_onChildInteraction = new Callable(this, MethodName.onChildInteraction);
            this.c_onLevelSelected = new Callable(this, MethodName.onLevelSelected);

            GetNode<SignalBus>(SignalBus.SignalBusPath).Connect(SignalBus.SignalName.LevelSelected, this.c_onLevelSelected);
        }
        
        this.LoadLevel(this.currentLevel);
        this.objectiveSlotsAmount = 0;
        this.objectiveSlotsCorrectlyFilled = 0;
        this.LiquidSourceIndexes = new();
        
        foreach(Node node in this.GetChildren())
        {
            switch(node)
            {
                case BaseSource: LiquidSourceIndexes.Add(node.GetIndex()); break;
                case LiquidObjective slotObjective: 
                    this.objectiveSlotsAmount++;
                    if(!slotObjective.IsConnected(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this.c_onObjectiveSlotStateChanged))
                    {
                        slotObjective.Connect(LiquidObjective.SignalName.ObjectiveSlotStateChanged, this.c_onObjectiveSlotStateChanged);
                    }

                    slotObjective.CallDeferred(LiquidObjective.MethodName.PlayBubbleSpreadingAnimation, this.GlobalPosition);
        
                    foreach(int lockedIndex in slotObjective.bubbleLockedTilesIndexes)
                    {
                        this.GetChild<ISlotInteractable>(lockedIndex).LockRotation();
                    }
                    
                    break;
            }

            var button = (Button)node;
            if(!button.IsConnected(Button.SignalName.Pressed, this.c_onChildInteraction))
            {
                button.Connect(Button.SignalName.Pressed, this.c_onChildInteraction);
            }
        }

        this.UpdateBoardState();
    }

    public void onLevelSelected(int level)
    {
        this.currentLevel = level;
        this.GetOwner<Node>().GetChild<AnimationPlayer>(0).Play("levelTransition"); //it calls _Reaady during animation!
        //this._Ready();
    }

    public void onChildInteraction()
    {
        this.UpdateBoardState();
        this.UpdateBoardState();

        if(this.objectiveSlotsCorrectlyFilled >= this.objectiveSlotsAmount)
        {
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.LevelCompleted, this.currentLevel);
        }
    }

    private void onObjectiveSlotStateChanged(LiquidObjective objectiveSlot, bool correctlyFilled)
    {
        if(correctlyFilled)
        { 
            this.objectiveSlotsCorrectlyFilled++;
            objectiveSlot.PlayBubbleReleasingAnimation();
            foreach(int index in objectiveSlot.bubbleLockedTilesIndexes)
            {
                this.GetChild<ISlotInteractable>(index).UnlockRotation();
            }
        }
        else
        {                
            this.objectiveSlotsCorrectlyFilled--;
            objectiveSlot.PlayBubbleSpreadingAnimation(this.GlobalPosition);
            foreach(int index in objectiveSlot.bubbleLockedTilesIndexes)
            {
                this.GetChild<ISlotInteractable>(index).LockRotation();
            }
        }

        if(this.objectiveSlotsCorrectlyFilled >= this.objectiveSlotsAmount)
        {
            GetNode<SignalBus>(SignalBus.SignalBusPath).EmitSignal(SignalBus.SignalName.LevelCompleted, this.currentLevel);
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
            node.UpdateDrawingState();}
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
                currentLiquid == LiquidType.Vazio)
            { 
                continue; 
            }

            currentNode.SetLiquid(outletPos, currentLiquid);

            foreach(Directions connections in currentNode.GetConnections(outletPos))
            {
                if(!currentNode.IsOpened(connections)){ continue; }

                visitados[currentNode].Add(connections);
                if(isMoveInsideBounds(((Node)currentNode).GetIndex(), connections, out ISlotInteractable neighborNode))
                {
                    proxVisita.Push((neighborNode, GameUtils.OppositeSide(connections), currentNode.GetLiquid(connections)));
                }
            }

            visitados[currentNode].Add(outletPos);
        }
    }
}

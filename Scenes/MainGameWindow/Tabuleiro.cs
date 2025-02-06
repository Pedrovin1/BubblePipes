using Godot;
using System;
using System.Collections.Generic;

public partial class Tabuleiro : GridContainer
{
    [Signal]
    public delegate void LevelCompletedEventHandler();

    [Export]
    private int currentLevel = 1;

    private List<int> LiquidSourceIndexes = new();
    private int objectiveSlotsAmount = 0;
    private int objectiveSlotsCorrectlyFilled = 0;

    public override void _Ready()
    {
        //this.LoadLevel(this.currentLevel);

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
                    slotObjective.ObjectiveSlotStateChanged += this.onObjectiveSlotStateChanged;
                    break;
            }

            ((Button)node).Pressed += this.onChildInteraction;
        }

        this.UpdateBoardState();
    }

    public void onChildInteraction()
    {
        this.UpdateBoardState();

        if(this.objectiveSlotsCorrectlyFilled >= this.objectiveSlotsAmount)
        {
            this.EmitSignal(Tabuleiro.SignalName.LevelCompleted);
        }

        //TEMP TEMP TEMP
        this.ExportLevel();
    }

    private void onObjectiveSlotStateChanged(LiquidObjective _, bool correctlyFilled)
    {
        if(correctlyFilled){ this.objectiveSlotsCorrectlyFilled++; }
        else{                this.objectiveSlotsCorrectlyFilled--; }
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

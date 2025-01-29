using Godot;
using System;
using System.Collections.Generic;

public partial class Tabuleiro : GridContainer
{

    //[Export] 
    //Resource LevelMap

    [Signal]
    public delegate void LevelCompletedEventHandler();

    private List<int> LiquidSourceIndexes = new();
    private int objectiveSlotsAmount = 0;
    private int objectiveSlotsCorrectlyFilled = 0;

    public override void _Ready()
    {
        //CarregarLevel a partir da Resource

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
            foreach((Directions outletPos, bool opened) in source.outletOpeningStates)
            {
                if(opened == true && 
                   isMoveInsideBounds(source.GetIndex(), outletPos, out ISlotInteractable neighborNode) &&
                   neighborNode.IsOpened(GameUtils.OppositeSide(outletPos)))
                { 
                    Stack<(ISlotInteractable, Directions, LiquidType)> proxVisita = new();
                    proxVisita.Push((neighborNode, GameUtils.OppositeSide(outletPos), source.outletLiquids[outletPos]));

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
                !currentNode.IsOpened(outletPos))    
            { 
                continue; 
            }

            currentNode.SetLiquid(outletPos, currentLiquid);

            foreach(Directions connections in currentNode.GetConnections(outletPos))
            {
                visitados[currentNode].Add(connections);
                //currentNode.SetLiquid(connections, liquid);
                if(isMoveInsideBounds(((Node)currentNode).GetIndex(), connections, out ISlotInteractable neighborNode))
                {
                    proxVisita.Push((neighborNode, GameUtils.OppositeSide(connections), currentNode.GetLiquid(connections)));
                }
            }

            visitados[currentNode].Add(outletPos);
        }
    }

    private bool isMoveInsideBounds(int currentIndex, Directions movement, out ISlotInteractable neighborNode)
    {
        int movementIndexOffset = 0;
        neighborNode = null;
        bool result = false;

        switch(movement)
        {
            case Directions.Cima: movementIndexOffset = -this.Columns;
                result = currentIndex + movementIndexOffset >= 0;
                break;
            
            case Directions.Direita: movementIndexOffset = 1;
                result = (currentIndex + movementIndexOffset) % this.Columns != 0 &&
                        currentIndex + movementIndexOffset < this.GetChildCount(); //unecessary but safer
                break;
            
            case Directions.Baixo: movementIndexOffset = this.Columns;
                result = currentIndex + movementIndexOffset < this.GetChildCount(); 
                break;
        
            case Directions.Esquerda: movementIndexOffset = -1;
                result = currentIndex % this.Columns != 0;
                break;

        }

        if(result)
        { 
            neighborNode = this.GetChild<ISlotInteractable>(currentIndex + movementIndexOffset); 
        }
        return result;
    }
}

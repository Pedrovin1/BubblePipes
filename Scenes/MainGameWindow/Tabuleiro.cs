using Godot;
using System;
using System.Collections.Generic;

public partial class Tabuleiro : GridContainer
{

    //[Export] Resource LevelMap
    private int[] LiquidSourceIndexes = null;

    public override void _Ready()
    {
        //CarregarLevel a partir da Resource

        List<int> indexes = new();
        foreach(Node node in this.GetChildren())
        {
            if(node is BaseSource)
            {
                indexes.Add(node.GetIndex());
            }

            ((Button)node).Pressed += this.onChildInteracted;
        }
        LiquidSourceIndexes = indexes.ToArray();

        this.UpdateBoardState();
    }

    public void onChildInteracted()
    {
        this.UpdateBoardState();
    }

    private void UpdateBoardState()
    {
        Dictionary<Node, bool> visitados = new();

        foreach(int sourceIndex in this.LiquidSourceIndexes)
        {
            var source = this.GetChild<BaseSource>(sourceIndex);
            foreach((Directions outletPos, bool opened) in source.outletOpeningStates)
            {
                if(opened == true && 
                   isMoveInsideBounds(source.GetIndex(), outletPos, out Node neighborNode) &&
                   neighborNode is BasePipe pipe && //condição temporária até prox refactor
                   pipe.outletStates[OppositeSide(outletPos)].Opened)
                { 
                    Stack<(Node, Directions)> proxVisita = new();
                    proxVisita.Push((neighborNode, OppositeSide(outletPos)));

                    FillPipes(source.outletLiquids[outletPos], visitados, proxVisita); 
                }
            }
        }

        foreach(Node node in this.GetChildren())
        {
            if(node is not BasePipe pipe){ continue; } //casting pra pipe temporario

            if(!visitados.TryGetValue(node, out _)) 
            {
                pipe.ResetOutletLiquids(LiquidType.Vazio);
            }

            pipe.UpdateDrawingState();
        }
    }

    private void FillPipes(LiquidType liquid, Dictionary<Node, bool> visitados, Stack<(Node, Directions)> proxVisita)
    {
        while(proxVisita.Count > 0)
        {
            (Node currentNode, Directions outletPos) = proxVisita.Pop();

            if(visitados.TryGetValue(currentNode, out _) || 
                currentNode is not BasePipe pipe         || //condição temporária até refactor
                !pipe.outletStates[outletPos].Opened     ||
                pipe.outletStates[outletPos].CurrentLiquid != LiquidType.Vazio && pipe.outletStates[outletPos].CurrentLiquid != liquid)
            { 
                continue; 
            }

            pipe.outletStates[outletPos].CurrentLiquid = liquid;

            foreach(Directions connections in pipe.outletStates[outletPos].Connections)
            {
                pipe.outletStates[connections].CurrentLiquid = liquid;
                if(isMoveInsideBounds(pipe.GetIndex(), connections, out Node neighborNode))
                {
                    proxVisita.Push((neighborNode, OppositeSide(connections)));
                }
            }

            visitados.Add(currentNode, true);
        }
    }

    private bool isMoveInsideBounds(int currentIndex, Directions movement, out Node neighborNode)
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
            neighborNode = this.GetChild(currentIndex + movementIndexOffset); 
        }
        return result;
    }

    public Directions OppositeSide(Directions direction)
    {
        return (Directions) ( ((int)direction + 2) % 4 ); 
    }
}

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
        }
        LiquidSourceIndexes = indexes.ToArray();
    }

    private void UpdateBoardState()
    {
        Dictionary<Node, bool> visitados = new();

        foreach(int sourceIndex in this.LiquidSourceIndexes)
        {
            var source = this.GetChild<BaseSource>(sourceIndex);
            foreach((Directions outletPos, bool opened) in source.outletOpeningStates)
            {
                if(opened == true){ FillPipes(source, outletPos, visitados); }
            }
        }
    }

    private void FillPipes(BaseSource source, Directions sourceStartOutlet, Dictionary<Node, bool> visitados)
    {
        Queue<(Node, Directions)> proxVisita = new();
        if(!isInsideBounds(source.GetIndex(), sourceStartOutlet)){ return; }
    }

    private bool isInsideBounds(int currentIndex, Directions movement)
    {
        int movementIndexOffset = 0;
        switch(movement)
        {
            case Directions.Cima: movementIndexOffset = -this.Columns;
                return currentIndex + movementIndexOffset >= 0;
            
            case Directions.Direita: movementIndexOffset = 1; 
                return (currentIndex + movementIndexOffset) % this.Columns != 0 &&
                        currentIndex + movementIndexOffset < this.GetChildCount(); //unecessary but safer
            
            case Directions.Baixo: movementIndexOffset = this.Columns;
                return currentIndex + movementIndexOffset < this.GetChildCount(); 
        
            case Directions.Esquerda: 
                return currentIndex % this.Columns != 0;
        }

        return false;
    }
}

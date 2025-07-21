using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


class Block {
 
    public List<Block> Next {get; set;} = new List<Block>();
    public int Index {get; set;}
    
    public Block(int index){
      this.Index = index;
    }
    
    public void SetNext(Block next){
        if (!this.Next.Any(e=>e.Index==next.Index)){
            this.Next.Add(next);
        }
    }
    
}

class Blocks : List<Block> {
    
    public Block GetBlock(int i){
        if (this.Any(e=>e.Index==i)){
         return this.FirstOrDefault(e=>e.Index==i);
        } else {
            Block block = new Block(i);
            this.Add(block);
            return block;
        }
    }
    
}
    
class Solution
{
    static int MaxPath(List<Block> blocks){
        List<int> paths= new List<int>();
        ExploreRecursive(blocks,new List<Block>(), paths);
        return paths.Max();
    }
    
    static void ExploreRecursive(List<Block> blocks, List<Block> chain=null, List<int> paths=null){
        foreach(var block in blocks){
            if (block.Next.Any()){
                if (chain==null)  { 
                    List<Block> newchain = new List<Block>();
                    newchain.Add(block);
                    ExploreRecursive(block.Next,newchain,paths); 
                }
                else 
                { 
                    List<Block> clonedchain = new List<Block>();
                    foreach(var chainblock in chain){
                        clonedchain.Add(chainblock);
                    }
                    clonedchain.Add(block);
                    ExploreRecursive(block.Next,clonedchain,paths);
                }
            } else {
                if (chain !=null){
                    var indexes = chain.Select(e=>e.Index).ToList();
                    if (paths==null) paths = new List<int>();
                    paths.Add(indexes.Count()+1);
                }
            }
        }
    }

    static void Main(string[] args)
    {
        Blocks blocks = new Blocks();
        int n = int.Parse(Console.ReadLine()); // the number of relationships of influence
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            Block block1 = blocks.GetBlock(int.Parse(inputs[0]));
            Block block2 = blocks.GetBlock(int.Parse(inputs[1]));
            block1.SetNext(block2);
        }
        Console.WriteLine(MaxPath(blocks));
    }
}
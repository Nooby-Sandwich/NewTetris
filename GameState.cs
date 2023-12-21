using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NewTetris
{
    public class GameState
    {
        private Block currentBlock;

        public Block CurrentBlock
        {
            get => currentBlock;
            private set
            {
                currentBlock = value;
                currentBlock.Reset();

                for(int i = 0; i < 2; i++)
                {
                    CurrentBlock.Move(1, 0);

                    if (!BlockFits())
                    {
                        currentBlock.Move(-1, 0);
                    }

                }
            }
        }

        public GameGrid gameGrid {  get; }
        public BlockQueue BlockQueue { get; }
        public bool GameOver { get; private set; }
        public GameGrid GameGrid{ get; }
        public int Score { get; private set; }
        public Block? HeldBlock { get; set; }
        public Boolean CanHold { get; private set; }

        public GameState()
        {
            GameGrid = new GameGrid(22, 10);
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate() ?? throw new InvalidOperationException("BlockQueue returned a null block");
            CanHold = true;
        }

        //for holding the block
        public void HoldBlock()
        {
            if (!CanHold)
            {
                return;
            }
            if(HeldBlock == null)
            {
                HeldBlock = CurrentBlock;
                CurrentBlock = BlockQueue.GetAndUpdate();

            }
            else
            {
                //Just Swap them 
                Block tmp = currentBlock;
                CurrentBlock = HeldBlock;
                HeldBlock = tmp;
            }

            CanHold = false;
        }

        
        //Whether the block is in the valid position or not
        private bool BlockFits()
        {
            foreach (Position p in CurrentBlock.TilePosition())
            {
                if (!GameGrid.IsEmpty(p.Row, p.Column))
                {
                    return false;
                }
            }

            return true;
        }

        //These Clockwise, counter-clockwise rotations and moving blocks only if block moves to a valid position
         public void RotateClockWise()
         {
            CurrentBlock.RotateClockwise();

            if (!BlockFits())
            {
                CurrentBlock.RotateCounterClockwise();
            }

         }

         public void RotateCounterClockWise()
         {
             CurrentBlock.RotateCounterClockwise();

             if (!BlockFits())
             {
                 CurrentBlock.RotateClockwise();
             }

         }

         public void MoveBlockLeft()
         {
            CurrentBlock.Move(0, -1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
            }

         }

         public void MoveBlockRight()
         {
             CurrentBlock.Move(0, 1);

             if (!BlockFits())
             {
                 CurrentBlock.Move(0, -1);
             }

         }

         
        //If game overs...
        private bool IsGameOver()
        {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
        }


        private void PlaceBlock()
        {
            // Move the current block to the grid
            foreach (Position p in CurrentBlock.TilePosition())
            {
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            int rowsCleared = GameGrid.ClearFullRow();

            // Award points based on the number of cleared rows
            switch (rowsCleared)
            {
                case 1:
                    Score += 40; // Single
                    break;
                case 2:
                    Score += 100; // Double
                    break;
                case 3:
                    Score += 300; // Triple
                    break;
                case 4:
                    Score += 1200; // Tetris
                    break;
            }

            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;
            }
        }




        //Move down method

        public void MoveBlockDown()
        {
            CurrentBlock.Move(1, 0);

            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
            }
        }
        private int TileDropDistance(Position p)
        {
            int drop = 0;

            while (GameGrid.IsEmpty(p.Row + drop + 1, p.Column))
            {
                drop++;
            }

            return drop;
        }

        public int BlockDropDistance()
        {
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilePosition())
            {
                drop = System.Math.Min(drop, TileDropDistance(p));
            }

            return drop;
        }

        public void DropBlock()
        {
            CurrentBlock.Move(BlockDropDistance(), 0);
            PlaceBlock();
        }

    }
}

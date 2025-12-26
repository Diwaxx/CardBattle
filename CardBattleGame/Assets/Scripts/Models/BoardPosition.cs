using System;
using UnityEngine;

namespace CardGame.Models
{
    [Serializable]
    public struct BoardPosition
    {
        public int row;     // 0-1 ( 2 ряда )
        public int column;  // 0-2 ( 3 колонки )
        public bool isPlayerSide;

        public BoardPosition(int row, int column, bool isPlayerSide)
        {
            this.row = row;
            this.column = column;
            this.isPlayerSide = isPlayerSide;
        }

        public bool IsValid()
        {
            return row >= 0 && row <= 1 && column >= 0 && column <= 2;
        }

        public override string ToString()
        {
            return $"Side: {(isPlayerSide ? "Player" : "Enemy")}, Row: {row}, Column: {column}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BoardPosition))
                return false;

            BoardPosition other = (BoardPosition)obj;
            return row == other.row && column == other.column && isPlayerSide == other.isPlayerSide;
        }

        public override int GetHashCode()
        {
            return (row, column, isPlayerSide).GetHashCode();
        }

        public static bool operator ==(BoardPosition a, BoardPosition b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BoardPosition a, BoardPosition b)
        {
            return !a.Equals(b);
        }
    }
}
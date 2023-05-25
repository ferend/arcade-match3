using _Project.Scripts.Match3.Actor;
using UnityEngine;

namespace _Project.Scripts.Match3.Gameplay.Powerups
{
    //TODO : Later improvement
    public class Highlight : MonoBehaviour
    {
        [SerializeField] private Board gameBoard;
        
        // void HighlightMatches()
        // {
        //     for (int i = 0; i < _width; i++)
        //     {
        //         for (int j = 0; j < _height; j++)
        //         {
        //             HighlightMatchesAt(i, j);
        //         }
        //     }
        // }
        //
        // private void HighlightMatchesAt(int x, int y)
        // {
        //     HighlightOff(x, y);
        //
        //     var combMatches = CombineMatches(x, y);
        //     if (combMatches.Count > 0)
        //     {
        //         foreach (GamePiece piece in combMatches)
        //         {
        //             HighlightOn(piece);
        //         }
        //     }
        // }
        //
        // private void HighlightOn(GamePiece piece)
        // {
        //     SpriteRenderer sr;
        //     sr = _tileArray[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
        //     sr.color = piece.GetComponent<SpriteRenderer>().color;
        // }
        //
        // private void HighlightOff(int x, int y)
        // {
        //     SpriteRenderer sr = _tileArray[x, y].GetComponent<SpriteRenderer>();
        //
        //     sr.color = new Color(sr.color.r, sr.color.g, sr.color.g, 0);
        // }


    }
}
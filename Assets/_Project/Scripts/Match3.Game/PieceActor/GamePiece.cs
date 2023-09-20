using System;
using System.Collections;
using _Project.Scripts.Match3.Game.BoardActor;
using _Project.Scripts.Match3.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Match3.Game.PieceActor
{
    
    public class GamePiece : MonoBehaviour
    {
        internal int xIndex;
        internal int yIndex;
        
        private SpriteRenderer _spriteRenderer;
        private Color[] _colors = Constants.TILE_COLORS;

        private bool isMoving = false;

        private Board _gameBoard;

        public Color gamePieceColor;

        private void Awake()
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            SetSpriteColor();
        }

        public void SetBoard(Board board)
        {
            _gameBoard = board;
        }
        
        private void SetSpriteColor()
        {
            
            int randomIndex = Random.Range(0, _colors.Length);
            Color randomColor = _colors[randomIndex];
            _spriteRenderer.color = randomColor;
            gamePieceColor = randomColor;
        }

        public void SetCoord(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        public void MoveGamePiece(int destX, int destY, float timeToMove)
        {
            if(isMoving) return;
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }

        private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
        {
            Vector3 startPos = transform.position;
            float elapsedTime = 0f;
            isMoving = true;

            while (true)
            {
                if (Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    if (_gameBoard != null)
                    {
                        _gameBoard.PlaceGamePiece(this, (int) destination.x, (int) destination.y);
                    }
                    break;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / timeToMove, 0f,1f);
                t = ExtensionMethods.SmoothStep(t);
                
                transform.position = Vector3.Lerp(startPos, destination, t);

                yield return null;
                
            }

            isMoving = false;
        }
        
    }
}
﻿using System.Collections;
using _Project.Scripts.Game.Board;
using _Project.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Game.Gamepiece
{
    
    public class BaseGamePiece : MonoBehaviour
    {
        internal int xIndex;
        internal int yIndex;
        
        internal SpriteRenderer spriteRenderer;
        private Color[] _colors = Constants.TILE_COLORS;

        private bool _isMoving = false;

        internal BoardComponent gameBoardComponent;

        public Color gamePieceColor;
        internal int scoreValue;

        private void Awake()
        {
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            SetSpriteColor();
            scoreValue = 1;
        }

        public void SetBoard(BoardComponent boardComponent)
        {
            gameBoardComponent = boardComponent;
        }

        protected void SetDefaultSpriteColor()
        {
            this.spriteRenderer.color = Color.white;
        }
        
        private void SetSpriteColor()
        {
            Bomb bomb = this.GetComponent<Bomb>();
            int randomIndex = Random.Range(1, _colors.Length);
            Color randomColor = _colors[randomIndex];
            spriteRenderer.color = randomColor;
            gamePieceColor = randomColor;
        }

        public void SetCoord(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        public void MoveGamePiece(int destX, int destY, float timeToMove)
        {
            if(_isMoving) return;
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }

        private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
        {
            Vector3 startPos = transform.position;
            float elapsedTime = 0f;
            _isMoving = true;

            while (true)
            {
                if (Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    if (gameBoardComponent != null)
                    {
                        gameBoardComponent.PlaceGamePiece(this, (int) destination.x, (int) destination.y);
                    }
                    break;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / timeToMove, 0f,1f);
                t = ExtensionMethods.SmoothStep(t);
                
                transform.position = Vector3.Lerp(startPos, destination, t);

                yield return null;
                
            }

            _isMoving = false;
        }
        
    }
}
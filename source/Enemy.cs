using AIGame.source;
using AIGame.source.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.source
{
    public class Enemy : Entity
    {
        private Animation enemyAnim;
        private Animation enemyAlertAnim;
        private Rectangle pathway;
        private float speed = 2;
        private bool isFacingRight = true;
        private StateMachine stateMachine;
        private Player player;
        State patrollingState;
        State attackingState;

        public Enemy(Texture2D spriteSheet, Texture2D alertSpriteSheet, Rectangle pathway, Player player, float speed = 1)
        {
            enemyAnim = new Animation(spriteSheet, millisecondsPerFrame: 100);
            enemyAlertAnim = new Animation(alertSpriteSheet, millisecondsPerFrame: 100);
            this.pathway = pathway;

            position = new Vector2(pathway.X, pathway.Y);
            hitbox = new Rectangle(pathway.X, pathway.Y, 16, 16);
            this.speed = speed;

            this.player = player;

            patrollingState = new StateEnemyPatrol(this);
            attackingState = new StateEnemyAttack(this);
            var transitionPlayerDistance = new TransitionPlayerDistance(this, player, 100, true);
            var transitionPlayerFarAway = new TransitionPlayerDistance(this, player, 200, false);
            var transitions = new Dictionary<State, List<(Transition, State)>>();
            transitions[patrollingState] = new List<(Transition, State)>() { { (transitionPlayerDistance, attackingState) } };
            transitions[attackingState] = new List<(Transition, State)>() { { (transitionPlayerFarAway, patrollingState) } };
            stateMachine = new StateMachine(patrollingState, transitions);
        }


        public override void Update(GameTime gameTime)
        {
            stateMachine.Update();

        }

        public void UpdatePatrolling()
        {
            if (!pathway.Contains(hitbox))
            {
                speed = -speed;
                isFacingRight = !isFacingRight;
            }
            position.X += speed;

            hitbox.X = (int)position.X;
            hitbox.Y = (int)position.Y;
        }

        public void UpdateAttacking()
        {
            if (pathway.Contains(hitbox))
            {
                if (speed > 0 && player.position.X < position.X || speed < 0 && player.position.X > position.X)
                {
                    speed = -speed;
                    isFacingRight = !isFacingRight;
                }

                // check if new position is outside pathway
                Vector2 newPos = position;
                newPos.X += speed;
                
                Rectangle newHitbox = hitbox;
                newHitbox.X = (int)newPos.X;

                if (pathway.Contains(newHitbox))
                {
                    position = newPos;
                    hitbox = newHitbox;
                }
            }
        }

        public bool hasHit(Rectangle playerRect)
        {
            return hitbox.Intersects(playerRect);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            SpriteEffects effect = isFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Animation animation = stateMachine.GetState == patrollingState ? enemyAnim : enemyAlertAnim;

            animation.Draw(spriteBatch, position, gameTime, effect);
        }
    }
}
using AIGame.source.StateMachineNS;
using AIGame.source.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.source.Entities
{
    public class Enemy : Entity
    {
        private Animation enemyAnim;
        private Animation enemyAlertAnim;
        private Animation deadAnim;
        private Animation jumpAnim;

        public bool Dead { get; set; } = false;
        public bool IsFacingRight { get; private set; } = true;

        private Rectangle pathway;
        private float speed = 2;
        private bool reset = false;
        private StateMachine stateMachine;
        private Player player;
        private State patrollingState;
        private State attackingState;
        private State deadState;
        private State jumpState;

        public Enemy(Texture2D spriteSheet, Texture2D alertSpriteSheet, Texture2D deadSpriteSheet, Texture2D jumpSpriteSheet, Rectangle pathway, Player player, float speed = 1, bool reset = false)
        {
            enemyAnim = new Animation(spriteSheet, millisecondsPerFrame: 100);
            enemyAlertAnim = new Animation(alertSpriteSheet, millisecondsPerFrame: 100);
            deadAnim = new Animation(deadSpriteSheet, millisecondsPerFrame: 100);
            deadAnim.Loop = false;
            jumpAnim = new Animation(jumpSpriteSheet, millisecondsPerFrame: 100);
            jumpAnim.Loop = false;

            this.pathway = pathway;

            position = new Vector2(pathway.X, pathway.Y);
            hitbox = new Rectangle(pathway.X, pathway.Y, 16, 16);
            this.speed = speed;

            this.player = player;

            patrollingState = new StateEnemyPatrol(this);
            attackingState = new StateEnemyAttack(this);
            deadState = new StateEnemyDead(this);
            jumpState = new StateEnemyJump(this);

            var transitionPlayerDistance = new TransitionPlayerDistance(this, player, 100, true, true);
            var transitionPlayerFarAway = new TransitionPlayerDistance(this, player, 150, false, false);
            var transitionDead = new TransitionDead(this);
            var transitionJumpComplete = new TransitionAnimationComplete(this, jumpAnim);

            var transitions = new Dictionary<State, List<(Transition, State)>>();
            transitions[patrollingState] = new List<(Transition, State)>() { { (transitionPlayerDistance, jumpState) }, { (transitionDead, deadState) } };
            transitions[jumpState] = new List<(Transition, State)>() { { (transitionJumpComplete, attackingState) }, { (transitionDead, deadState) } };
            transitions[attackingState] = new List<(Transition, State)>() { { (transitionPlayerFarAway, patrollingState) }, { (transitionDead, deadState) } };
            stateMachine = new StateMachine(patrollingState, transitions);
            this.reset = reset;
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
                IsFacingRight = !IsFacingRight;
            }
            position.X += speed;

            hitbox.X = (int)position.X;
            hitbox.Y = (int)position.Y;
        }

        public void UpdateAttacking()
        {
            jumpAnim.Reset();

            if (pathway.Contains(hitbox))
            {
                if (speed > 0 && player.position.X < position.X || speed < 0 && player.position.X > position.X)
                {
                    speed = -speed;
                    IsFacingRight = !IsFacingRight;
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

        public void UpdateDead()
        {

        }

        public bool hasHit(Rectangle playerRect)
        {
            return hitbox.Intersects(playerRect);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            SpriteEffects effect = IsFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Animation animation;
            if (stateMachine.GetState == patrollingState)
            {
                animation = enemyAnim;
            }
            else if (stateMachine.GetState == attackingState)
            {
                animation = enemyAlertAnim;
            }
            else if (stateMachine.GetState == deadState)
            {
                animation = deadAnim;
            }
            else if (stateMachine.GetState == jumpState)
            {
                animation = jumpAnim;
            }
            else
            {
                throw new Exception("invalid state");
            }

            animation.Draw(spriteBatch, position, gameTime, effect);
        }
    }
}
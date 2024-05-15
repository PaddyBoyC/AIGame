using AIGame.source.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.Flocking
{
    internal class SnowBat : Entity
    {
        public Vector2 velocity;
        private Animation anim;
        private Animation sleepAnim;
        private Animation awakeAnim;
        private List<SnowBat> snowbats;
        public bool Awake { get; private set; }
        private Player player;
        private Vector2? biasDirection;

        public SnowBat(Vector2 position, (Texture2D, Texture2D) spriteSheets, List<SnowBat> snowBats, Player player, Vector2? biasDirection = null)
        {
            this.position = position;
            this.biasDirection = biasDirection;
            this.player = player;
            snowbats = snowBats;
            sleepAnim = new Animation(spriteSheets.Item1, millisecondsPerFrame: 500);
            awakeAnim = new Animation(spriteSheets.Item2, millisecondsPerFrame: 100);
            anim = sleepAnim;

            velocity = new Vector2(0, -1);
            Awake = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Awake)
            {
                if ((player.position - position).Length() < 80)
                {
                    Awake = true;
                    anim = awakeAnim;
                    Random rnd = new Random();
                    velocity.X = rnd.NextSingle() * 2 - 1;
                }
                return;
            }
            const float avoidFactor = 0.0008f;
            const float matchingFactor = 0.05f;
            const float centeringFactor = 0.0005f;
            const float turnFactor = 0.2f;
            const float biasFactor = 0.1f;
            const float tooCloseRange = 80;
            const float visionRange = 200;
            const float maxSpeed = 2;
            const float leftMargin = 0;
            const float topMargin = 990;
            const float rightMargin = 900;
            const float bottomMargin = 1376;

            Vector2 close = Vector2.Zero;
            Vector2 velAverage = Vector2.Zero;
            Vector2 posAverage = Vector2.Zero;
            int numNeighbours = 0;

            foreach (SnowBat other in snowbats)
            {
                if (other != this && other.Awake)
                {
                    float distance = (other.position - position).Length();
                    if (distance < tooCloseRange)
                    {
                        close += position - other.position;
                    }
                    if (distance < visionRange)
                    {
                        velAverage += other.velocity;
                        posAverage += other.position;
                        numNeighbours++;
                    }
                }
            }

            //check for screen edges

            if (position.X < leftMargin)
            {
                velocity.X += turnFactor;
            }
            if (position.Y < topMargin)
            {
                velocity.Y += turnFactor;
            }
            if (position.X > rightMargin)
            {
                velocity.X -= turnFactor;
            }
            if (position.Y > bottomMargin)
            {
                velocity.Y -= turnFactor;
            }

            velocity += close * avoidFactor;
            if (numNeighbours > 0)
            {
                velAverage /= numNeighbours;
                posAverage /= numNeighbours;
                velocity += velAverage * matchingFactor;
                velocity += (posAverage - position) * centeringFactor;
            }

            if (biasDirection.HasValue)
            {
                velocity += biasDirection.Value * biasFactor;
            }

            if (velocity.Length() > maxSpeed)
            {
                velocity.Normalize();
                velocity *= maxSpeed;
            }
            position += velocity;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            anim.Draw(spriteBatch, position, gameTime, velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}

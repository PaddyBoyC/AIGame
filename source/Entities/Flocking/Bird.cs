using AIGame.source.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    internal class Bird : Entity
    {  
        public Vector2 velocity;
        private Animation anim;
        private Animation sleepAnim;
        private Animation awakeAnim;
        private List<Bird> birds;
        public bool Awake { get; private set; }
        private Player player;
        private Vector2? biasDirection;
        private int awakeDistance;
        float leftMargin; //prevents the birds from forever flying off the screen.
        float topMargin; //prevents the birds from forever flying off the screen.
        float rightMargin; //prevents the birds from forever flying off the screen.
        float bottomMargin; //keeps the respective flying animal (parrot in this case) from leaving their respective biome.

        public Bird(Vector2 position, (Texture2D, Texture2D) spriteSheets,  List<Bird> birds, Player player, Vector2? biasDirection = null, int awakeDistance = 50,
                    float leftMargin = 0, float topMargin = 0, float rightMargin = 900, float bottomMargin = 369)
        {
            this.position = position;
            this.biasDirection = biasDirection;
            this.player = player;
            this.birds = birds;
            this.awakeDistance = awakeDistance;
            this.leftMargin = leftMargin;
            this.topMargin = topMargin;
            this.rightMargin = rightMargin;
            this.bottomMargin = bottomMargin;

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
                if ((player.position - position).Length() < awakeDistance) //if the player comes into a radius of 50 pixels the birds will transition from being asleep to awake.
                {
                    Awake = true;
                    anim = awakeAnim;
                    Random rnd = new Random();
                    velocity.X = rnd.NextSingle() * 2 - 1; //randomises the bird sprite everytime the game is launced.
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

            Vector2 close = Vector2.Zero;
            Vector2 velAverage = Vector2.Zero;
            Vector2 posAverage = Vector2.Zero;
            int numNeighbours = 0;

            foreach (Bird other in birds)
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

            anim.Draw(spriteBatch, position, gameTime, velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None); //essentially checks if the sprite is moving negative or positive on the x axis and flips the sprite.
        }
    }
}

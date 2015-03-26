using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlingGame
{
    public class BowlingGame
    {
        private readonly List<int> rolls = new List<int>();
        private int currentFrame = 1;
        private int currentRollOfFrame = 1;

        static public string ExceptionMoreThan10PinsSingleRollMessage = "Cannot hit more than 10 pins in a single roll.";
        static public string ExceptionMoreThan10PinsTwoRollsMessage = "Pins hit in current frame cannot exceed 10.";
        static public string ExceptionCannotCalculateIncompleteScore = "You haven't rolled twice in your current frame, or you rolled a strike or spare without followup roll(s).";

        public BowlingGame()
        {
                
        }

        public void Roll(int pinsHit)
        {
            rolls.Add(pinsHit);

            // validate "rules"
            if (pinsHit > 10)
            {
                throw new ArgumentException(ExceptionMoreThan10PinsSingleRollMessage);
            }
            else if (currentRollOfFrame == 2 && (pinsHit + rolls[rolls.Count - 2]) > 10)
            {
                throw new ArgumentException(ExceptionMoreThan10PinsTwoRollsMessage);
            }

            if (pinsHit == 10)
            {
                ++currentFrame;
                currentRollOfFrame = 1;
            }
            else if (currentRollOfFrame == 2)
            {
                ++currentFrame;
                currentRollOfFrame = 1;
            }
            else
            {
                ++currentRollOfFrame;
            }
        }

        public void Reset()
        {
            rolls.Clear();
        }

        public int Score
        {
            get
            {
                int score = 0;
                int frame = 0;

                try
                {
                    for (int rollIndex = 0; rollIndex < rolls.Count && frame < 10; ++frame)
                    {
                        if (rolls[rollIndex] == 10) // strike
                        {
                            score += 10 + rolls[rollIndex + 1] + rolls[rollIndex + 2];
                            ++rollIndex;
                        }
                        else if (rolls[rollIndex] + rolls[rollIndex + 1] == 10) // spare
                        {
                            score += 10 + rolls[rollIndex + 2];
                            rollIndex += 2;
                        }
                        else
                        {
                            score += rolls[rollIndex] + rolls[rollIndex + 1];
                            rollIndex += 2;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new InvalidOperationException(ExceptionCannotCalculateIncompleteScore);
                }

                return score;
            } 
            
        }
    }
}

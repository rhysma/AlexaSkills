using System;
using System.Collections.Generic;
using System.Text;

namespace alexawitskill
{
    class FactData
    {
        public string GetFactMessage { get; set; }
        public List<string> Facts { get; set; }

        public string SkillName { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
        public string LaunchMessage { get; set; }
        public string LaunchMessageReprompt { get; set; }
        public string AskMessage { get; set; }

        public FactData()
        {
            this.Facts = new List<string>();
        }

        /// <summary>
        /// Load the fact data into the list for processing/use
        /// </summary>
        /// <returns></returns>
        public static List<FactData> LoadFacts()
        {
            List<FactData> factList = new List<FactData>();
            FactData facts = new FactData();

            //prompt messages necessary for voice interaction
            facts.SkillName = "Women in tech";
            facts.GetFactMessage = "Here's an interesting one: ";
            facts.HelpMessage = "You can say tell me a women in tech fact and I will give you a random one...What can I help you with?";
            facts.HelpReprompt = "What can I help you with?";
            facts.StopMessage = "Goodbye!";
            facts.LaunchMessage = "Welcome to Women in tech facts. I know facts about famous women in technology and computing. What would you like to know?";
            facts.LaunchMessageReprompt = "Try asking me to give you a women in tech fact";
            facts.AskMessage = " What else would you like to know?";

            //populate the list with the facts used 
            facts.Facts.Add("Sister Mary Kenneth Keller was the first women in the United States to recieve a Ph.D in computer science in 1965...");
            facts.Facts.Add("The first computer programmer was Ada Lovelace...");
            facts.Facts.Add("Edith Clarke was the first female electrical engineer and was the first female professor of electrical engineering at the University of Texas at Austin...");
            facts.Facts.Add("The first compiler was invited by Grace Hopper in 1951...");
            facts.Facts.Add("The spanning-tree protocol that makes the modern Internet possible was invented by Radia Perlmann...");
            facts.Facts.Add("Jean Jennings Bartik was one of the six first ENIAC operators and did calculations on rocket and cannon trajectories in 1945...");
            facts.Facts.Add("The Apple icon was developed by Susan Kare...");
            facts.Facts.Add("Carol Shaw was a pioneer game developer for Atari in 1978 and was noted as one of the best programmers for the 6 5 0 2 microprocessor...");
            facts.Facts.Add("In 1942 Hedy Lamarr invented the frequency-hopping technology that allows for the modern day use of wi-fi and bluetooth...");
            facts.Facts.Add("Google hired Marissa Mayer in 1999 as their first female engineer and she is now vice president of location and local services...");
            factList.Add(facts);
            return factList;
        }
    }
}

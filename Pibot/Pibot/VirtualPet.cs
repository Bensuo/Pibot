using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pibot
{
    class VirtualPet
    {
        private string name;
        private int hunger;
        private int happiness;
        private long chatID;

        public VirtualPet()
        {
            this.name = "";
            this.chatID = 0;
            this.hunger = 0;
            this.happiness = 5;
        }
        public VirtualPet(string name, long chatID)
        {
            this.name = name;
            this.chatID = chatID;
            this.hunger = 0;
            this.happiness = 5;
        }

        public int Hunger
        {
            get { return hunger; }
        }

        public int Happiness
        {
            get { return happiness; }
        }

        public long ChatId
        {
            get { return chatID; }
        }

        public string Name
        {
            get { return name; }
        }

        public void Update()
        {
            if (Hunger < 10 )
            {
                hunger = Hunger + 1;
            }
            if (Happiness > 0)
            {
                happiness = Happiness - 1;
            }
        }

        public void Feed(int amount)
        {
            hunger = Hunger - amount;
            if (Hunger < 0) hunger = 0;
        }

        public void PlayWith()
        {
            happiness = Happiness + 5;
        }
    }
}

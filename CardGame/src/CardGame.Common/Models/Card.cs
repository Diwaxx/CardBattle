using System;
using System.Collections.Generic;
using System.Text;

namespace CardGame.Common.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public long Damage { get; set; } 
        public long Health { get; set; }

        public string? Description { get; set; }
        public int Level { get; set; }
        public long Exp {  get; set; }
        public string? Type { get; set; }
     
    }
}

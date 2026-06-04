using System.Collections.Generic;
namespace ElementGame.Managers
{
    /* інкапсуляція: рецепти приховані всередині класу */
    public class ElementRecipeManager
    {
        /* словник рецептів, ключ — рядок виду "A+B" */
        private static readonly Dictionary<string, string> Recipes = new()
        {
            { "Earth+Fire",   "Ash"       },
            { "Fire+Earth",   "Ash"       },
            { "Fire+Water",   "Steam"     },
            { "Water+Fire",   "Steam"     },
            { "Earth+Water",  "Mud"       },
            { "Water+Earth",  "Mud"       },
            { "Earth+Wind",   "Sand"      },
            { "Wind+Earth",   "Sand"      },
            { "Steam+Wind",   "Cloud"     },
            { "Wind+Steam",   "Cloud"     },
            { "Sand+Fire",    "Ash"       },
            { "Fire+Sand",    "Ash"       },
            { "Mud+Fire",     "Brick"     },
            { "Fire+Mud",     "Brick"     },
            { "Fire+Cloud",   "Thunder"   },
            { "Cloud+Fire",   "Thunder"   },
            { "Brick+Fire",   "Stone"     },
            { "Fire+Brick",   "Stone"     },
            { "Stone+Fire",   "Metal"     },
            { "Fire+Stone",   "Metal"     },
            { "Thunder+Metal","Magnet"    },
            { "Metal+Thunder","Magnet"    },
            { "Magnet+Cloud", "Artefact"  },
            { "Cloud+Magnet", "Artefact"  },
            { "Artefact+Cloud","LifeStone"},
            { "Cloud+Artefact","LifeStone"},
        };

        /* шукаємо результат у словнику, null якщо рецепта немає */
        public virtual string Combine(string a, string b)
        {
            if (a == null || b == null) return null;
            string key = a + "+" + b;
            return Recipes.TryGetValue(key, out var result) ? result : null;
        }

        /* зручна перевірка без отримання результату */
        public bool CanCombine(string a, string b) => Combine(a, b) != null;
    }
}
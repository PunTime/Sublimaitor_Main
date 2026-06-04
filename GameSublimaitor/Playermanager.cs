using System;
using System.Collections.Generic;
using System.IO;
namespace ElementGame.Managers
{
    /* дані одного гравця: ім'я і список відкритих елементів */
    public class PlayerData
    {
        public string Name { get; set; } = "Player";
        /* базові 4 елементи доступні з початку */
        public List<string> UnlockedElements { get; set; } = new List<string> { "Fire", "Water", "Wind", "Earth" };
    }

    public class PlayerManager
    {
        private const int MaxPlayers = 4;
        private PlayerData[] _players = new PlayerData[MaxPlayers];
        private bool[] _winners = new bool[MaxPlayers]; /* true якщо гравець створив LifeStone */

        public bool IsWinner(int index) => _winners[index];

        public void SetWinner(int index)
        {
            if (index >= 0 && index < MaxPlayers)
                _winners[index] = true;
        }

        /* ініціалізуємо всіх гравців з іменами за замовчуванням */
        public PlayerManager()
        {
            for (int i = 0; i < MaxPlayers; i++)
                _players[i] = new PlayerData { Name = $"Player {i + 1}" };
        }

        public PlayerData GetPlayer(int index)
        {
            if (index < 0 || index >= MaxPlayers) throw new IndexOutOfRangeException();
            return _players[index];
        }

        /* ім'я обрізаємо до 15 символів і одразу зберігаємо */
        public void SetPlayerName(int index, string name)
        {
            if (index < 0 || index >= MaxPlayers) return;
            _players[index].Name = name.Length > 15 ? name.Substring(0, 15) : name;
            Save(index);
        }

        /* додаємо елемент якщо його ще немає і зберігаємо */
        public void UnlockElement(int playerIndex, string element)
        {
            var p = _players[playerIndex];
            if (!p.UnlockedElements.Contains(element))
                p.UnlockedElements.Add(element);
            Save(playerIndex);
        }

        /* зберігаємо гравця у текстовий файл: рядок імені + рядок елементів */
        public void Save(int index)
        {
            try
            {
                string path = $"player{index + 1}.txt";
                using var sw = new StreamWriter(path);
                sw.WriteLine(_players[index].Name);
                sw.WriteLine(string.Join(",", _players[index].UnlockedElements));
            }
            catch { } /* мовчки ігноруємо помилки запису */
        }

        /* завантажуємо всіх гравців, пропускаємо відсутні файли */
        public void Load()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                try
                {
                    string path = $"player{i + 1}.txt";
                    if (!File.Exists(path)) continue;
                    var lines = File.ReadAllLines(path);
                    if (lines.Length >= 1) _players[i].Name = lines[0];
                    if (lines.Length >= 2)
                    {
                        /* відновлюємо список елементів з рядка через кому */
                        _players[i].UnlockedElements = new List<string>(lines[1].Split(','));
                    }
                }
                catch { } /* мовчки ігноруємо помилки читання */
            }
        }
    }
}
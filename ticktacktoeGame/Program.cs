using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ticktacktoeGame
{
    public enum Direction
    {
        Left,
        Rigth,
        Up,
        Down,
    }

    public class BoardState
    {

        public int Id { get; set; }
        public int Y { get; set; }
        public int X { get; set; }
        public Player Player { get; set; }
        public string CurrentText
        {
            get
            {
                if (Player != null)
                    return Player.figure;
                return string.Empty;
            }
        }
    }
    public class WinData
    {
        private List<Tuple<int, Player>> data = new List<Tuple<int, Player>>();

        public WinData(params int[] tableids)
        {

            foreach (var id in tableids)
            {
                data.Add(new Tuple<int, Player>(id, null));
            }
        }
        public bool doesHaveid(int id)
        {
            var count = data.Where(x => x.Item1.Equals(id)).Count();
            if (count > 0)
                return true;
            return false;
        }
        public void AddPositionToPlayer(int boardid, Player player)
        {
            data.Where(x => x.Item1.Equals(boardid)).ToList().ForEach((x) =>
            {
                data.Remove(x);
                data.Add(new Tuple<int, Player>(x.Item1, player));
            });
        }
        public bool DoesHaveThisCombination(Player player)
        {
            var count = data.Where(X => player.Id.Equals(X.Item2?.Id));
            if (count.Count() == data.Count())
                return true;
            return false;

        }

    }
    public class Player
    {
        public string Name { get; set; }
        public string figure { get; set; }
        public Guid Id { get; set; }
        public int Score { get; set; }
        public Player()
        {
            Score = 0;
            Id = Guid.NewGuid();
        }
    }
    public class Board
    {
        public readonly List<BoardState> TableValues;
        public List<WinData> winDatas = new List<WinData>();
        public string ErrorMessages { get; set; }
        public void DrawControlsHelp()
        {
            Console.SetCursorPosition(30, 2);
            Console.Write("Press Enter To set your spot and use The Arrows to move");
        }
        public void ResetBoard()
        {
            if (winDatas.Count > 0)
                winDatas.Clear();

            winDatas.Add(new WinData(1, 2, 3));
            winDatas.Add(new WinData(4, 5, 6));
            winDatas.Add(new WinData(7, 8, 9));
            winDatas.Add(new WinData(1, 4, 7));
            winDatas.Add(new WinData(2, 5, 8));
            winDatas.Add(new WinData(3, 6, 9));
            winDatas.Add(new WinData(1, 5, 9));
            winDatas.Add(new WinData(3, 5, 7));

            if (TableValues.Count > 0)
                TableValues.Clear();

            var y = 1;
            var id = 0;
            for (int i = 0; i < 3; i++)
            {
                TableValues.Add(new BoardState { X = 3, Y = y, Id = ++id });
                TableValues.Add(new BoardState { X = 9, Y = y, Id = ++id });
                TableValues.Add(new BoardState { X = 16, Y = y, Id = ++id });
                y += 2;

            }

        }
        public Board()
        {
            TableValues = new List<BoardState>();
            ResetBoard();
        }
        public bool CheckForWinners(Player player)
        {
            if (winDatas.Where(x => x.DoesHaveThisCombination(player)).Count() > 0)
                return true;

            return false;

        }
        public void DrawErrorMessages()
        {
            if (ErrorMessages != string.Empty)
            {
                Console.SetCursorPosition(30, 1);
                Console.Write(ErrorMessages);
                ErrorMessages = string.Empty;
            }
        }
        public void DrawSpots()
        {
            var value = TableValues.Where(k => k.CurrentText != string.Empty).ToList();
            foreach (var item in value)
            {
                Console.SetCursorPosition(item.X, item.Y - 1);
                Console.Write(item.CurrentText);
            }

        }
        public bool AddSpot(int x, int y, Player player)
        {
            var value = TableValues.FirstOrDefault(k => (k.Y == y && k.X == x) && (k.Player == null));
            if (value != null)
            {
                TableValues.Remove(value);
                value.Player = player;
                TableValues.Add(value);
                var items = winDatas.Where(f => f.doesHaveid(value.Id));
                foreach (var item in items)
                {
                    item.AddPositionToPlayer(value.Id, player);
                }
                return true;
            }
            return false;
        }
        public void DrawScore(Player player, Player player2)
        {
            Console.SetCursorPosition(30, 0);
            Console.Write($"{player.Name}:{player.Score} | {player2.Name}:{player2.Score}");

        }
        public void DrawBoard()
        {
            Console.Clear();
            var left = 0;
            var top = 0;
            for (int i = 1; i <= 80; i++)
            {

                left = (left < 20) ? left : 0;
                Console.SetCursorPosition(left, top);
                Console.Write("¨¨");
                left++;
                top = (left < 20) ? top : top + 2;

            }
            for (int i = 0; i < 6; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("|");
                Console.SetCursorPosition(6, i);
                Console.Write("|");
                Console.SetCursorPosition(13, i);
                Console.Write("|");
                Console.SetCursorPosition(20, i);
                Console.Write("|");
            }

        }

    }
    public class GameEngine
    {
        private readonly Board CurrentBoard;
        private Player CurrentPlayer { get; set; }
        private Tuple<Player, Player> Players { get; set; }
        private int YPosition { get; set; }
        private int XPosition { get; set; }
        public bool CheckIfIsFull()
        {
            return CurrentBoard.TableValues.TrueForAll(x => x.Player != null);
        }
        private void CheckMovement()
        {
            var Isvalid = CurrentBoard.AddSpot(XPosition, YPosition, CurrentPlayer);

            if (!Isvalid)
            {
                CurrentBoard.ErrorMessages = "Invalid Case";
                return;
            }
            if (CurrentBoard.CheckForWinners(CurrentPlayer))
            {
                CurrentBoard.ErrorMessages = $"{CurrentPlayer.Name} Wins!";
                CurrentBoard.ResetBoard();
                CurrentPlayer.Score += 1;
                SetPotiionToTheMidle();

            }
            if (CheckIfIsFull())
            {
                CurrentBoard.ErrorMessages = "All squares are full, starting a new game..";
                CurrentBoard.ResetBoard();
            }
            if (CurrentPlayer.Equals(Players.Item1))
            {
                CurrentPlayer = Players.Item2;
            }
            else
            {
                CurrentPlayer = Players.Item1;
            }
        }
        public bool CheckXDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    {

                        if (XPosition == 9)
                        {
                            XPosition -= 6;
                            return true;
                        }
                        if (XPosition == 16)
                        {
                            XPosition -= 7;
                            return true;
                        }
                    }
                    break;
                case Direction.Rigth:
                    {
                        if (XPosition == 3)
                        {
                            XPosition += 6;
                            return true;
                        }
                        if (XPosition == 9)
                        {
                            XPosition += 7;
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        public bool CheckYDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    {

                        if (YPosition == 1)
                        {
                            YPosition += 2;
                            return true;
                        }
                        if (YPosition == 3)
                        {
                            YPosition += 2;
                            return true;
                        }
                    }
                    break;
                case Direction.Up:
                    {
                        if (YPosition == 3)
                        {
                            YPosition -= 2;
                            return true;
                        }
                        if (YPosition == 5)
                        {
                            YPosition -= 2;
                            return true;
                        }
                    }
                    break;
            }
            return false;

        }
        public void DrawScoreBoard()
        {
            CurrentBoard.DrawScore(Players.Item1, Players.Item2);
        }
        public void LitsenPlayerMoves()
        {

            new Thread(() =>
            {
                Console.SetCursorPosition(XPosition, YPosition);

                for (ConsoleKey? key = null; key != ConsoleKey.Escape; key = Console.ReadKey().Key)
                {

                    switch (key)
                    {
                        case ConsoleKey.LeftArrow:
                            {
                                CheckXDirection(Direction.Left);
                            }
                            break;
                        case ConsoleKey.RightArrow:
                            {
                                CheckXDirection(Direction.Rigth);
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            {
                                CheckYDirection(Direction.Up);
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            {
                                CheckYDirection(Direction.Down);
                            }
                            break;
                        case ConsoleKey.Enter:
                            {
                                CheckMovement();
                            }
                            break;

                    }
                    CurrentBoard.DrawBoard();
                    CurrentBoard.DrawSpots();
                    DrawScoreBoard();
                    CurrentBoard.DrawControlsHelp();
                    CurrentBoard.DrawErrorMessages();
                    Console.SetCursorPosition(XPosition, YPosition);
                }

            }).Start();

        }
        public void SetPotiionToTheMidle()
        {
            YPosition = 3;
            XPosition = 9;
        }
        public GameEngine(string playerName1, string playerName2)
        {
            Players = new Tuple<Player, Player>(
                new Player { Name = playerName1, figure = "X" },
                new Player { Name = playerName2, figure = "O" });
            SetPotiionToTheMidle();
            CurrentPlayer = Players.Item1;
            CurrentBoard = new Board();
            LitsenPlayerMoves();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the first Name of the Player");
            var player1 = Console.ReadLine();
            Console.WriteLine("Enter the Second Name of the Player");
            var player2 = Console.ReadLine();
            Console.Clear();
            new GameEngine(player1, player2);

        }
    }
}


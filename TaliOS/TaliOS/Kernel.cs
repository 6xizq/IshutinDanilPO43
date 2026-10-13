using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using Sys = Cosmos.System;

namespace TaliOS
{
    public class Kernel : Sys.Kernel
    {
        private CommandHandler commandHandler;
        private List<string> history = new List<string>();
        private int historyIndex = -1;
        protected override void BeforeRun()
        {
            var fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs);

            Console.Clear();
            Console.WriteLine("Micro Core v0.1");
            Console.WriteLine("Type 'help' for available commands.");
            commandHandler = new CommandHandler();
        }
        protected override void Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("root@core> ");
            Console.ResetColor();
            string input = ReadLineWithHistory();
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (history.Count == 0 || history[history.Count - 1] != input)
                {
                    history.Add(input);
                }
                historyIndex = history.Count;
                commandHandler.Execute(input);
            }
        }
        private string ReadLineWithHistory()
        {
            string currentLine = "";
            int cursorPos = 0;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return currentLine;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursorPos > 0)
                    {
                        currentLine = currentLine.Remove(cursorPos - 1, 1);
                        cursorPos--;
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (history.Count > 0)
                    {
                        while (cursorPos > 0)
                        {
                            Console.Write("\b \b");
                            cursorPos--;
                        }
                        historyIndex = Math.Max(0, historyIndex - 1);
                        currentLine = history[historyIndex];
                        Console.Write(currentLine);
                        cursorPos = currentLine.Length;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (history.Count > 0 && historyIndex < history.Count - 1)
                    {
                        while (cursorPos > 0)
                        {
                            Console.Write("\b \b");
                            cursorPos--;
                        }
                        historyIndex++;
                        currentLine = history[historyIndex];
                        Console.Write(currentLine);
                        cursorPos = currentLine.Length;
                    }
                    else
                    {
                        while (cursorPos > 0)
                        {
                            Console.Write("\b \b");
                            cursorPos--;
                        }
                        historyIndex = history.Count;
                        currentLine = "";
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    // Запланировано движение курсора (пока не реализовано)
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    // Запланировано движение курсора (пока не реализовано)
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    currentLine = currentLine.Insert(cursorPos, key.KeyChar.ToString());
                    cursorPos++;
                    Console.Write(key.KeyChar);
                }
            }
        }
        public class CommandHandler
        {
            public void Execute(string input)
            {
                string[] parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string cmd = parts[0].ToLower();
                string args = parts.Length > 1 ? parts[1] : null;
                switch (cmd)
                {
                    case "help":
                        Console.WriteLine("Available commands:");
                        Console.WriteLine("  help  - Show this help message");
                        Console.WriteLine("  clear - Clear the screen");
                        Console.WriteLine("  echo  - Print a message (usage: echo <message>)");
                        Console.WriteLine("  time  - Display current system time");
                        Console.WriteLine("  date  - Display current system date");
                        Console.WriteLine("  ls    - List files and directories");
                        Console.WriteLine("  run   - Execute a script (usage: run <path>)");
                        Console.WriteLine("  reboot - Reboot the system");
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "time":
                        Console.WriteLine($"Current time: {DateTime.Now.ToLongTimeString()}");
                        break;
                    case "date":
                        Console.WriteLine(DateTime.Now.ToLongDateString());
                        break;
                    case "reboot":
                        Console.WriteLine("Rebooting...");
                        Sys.Power.Reboot();
                        break;
                    case "echo":
                        if (!string.IsNullOrEmpty(args))
                        {
                            Console.WriteLine(args);
                        }
                        else
                        {
                            Console.WriteLine("Usage: echo <message>");
                        }
                        break;
                    case "ls":
                        try
                        {
                            Console.WriteLine("Files:");
                            string[] files = Directory.GetFiles(@"0:\");
                            foreach (string file in files)
                            {
                                Console.WriteLine("  [FILE] " + Path.GetFileName(file));
                            }

                            Console.WriteLine("\nDirectories:");
                            string[] dirs = Directory.GetDirectories(@"0:\");
                            foreach (string dir in dirs)
                            {
                                Console.WriteLine("  [DIR]  " + new DirectoryInfo(dir).Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error reading directory: " + ex.Message);
                        }
                        break;
                    case "run":
                        if (string.IsNullOrEmpty(args))
                        {
                            Console.WriteLine("Usage: run <path>");
                            break;
                        }
                        try
                        {
                            string content = File.ReadAllText(args);
                            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string line in lines)
                            {
                                if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                                {
                                    Console.WriteLine("> " + line);
                                    Execute(line);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Script error: " + ex.Message);
                        }
                        break;

                    default:
                        Console.WriteLine($"Unknown command: '{cmd}'");
                        break;
                }
            }
        }
    }
}
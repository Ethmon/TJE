using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Terminal.Gui;
using static Terminal.Gui.View;
using System.Globalization;

namespace Text_editor_E
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Dictionary<string, ConsoleColor> mode = new Dictionary<string, ConsoleColor>();
                while (true)
                {
                    string text = Console.ReadLine();
                    string fileName = @"" + text;
                    try
                    {
                        if (text == "new")
                        {
                            string name = Console.ReadLine();
                            if (name == "/n/")
                            {
                                break;
                            }
                            text_editor text_editor = new text_editor("", name + ".txt", true);
                            text_editor.Run(mode);
                        }
                        else if (text == "inside")
                        {
                            string name = Console.ReadLine();
                            if (name == "/n/")
                            {
                                break;
                            }
                            text = File.ReadAllText(name);
                            text_editor text_editor = new text_editor(text, name, true);
                            text_editor.Run(mode);
                        }
                        else if (text == "path")
                        {
                            using (StreamReader streamReader = File.OpenText(fileName))
                            {
                                string name = Console.ReadLine();
                                if (name == "/n/")
                                {
                                    break;
                                }
                                text = streamReader.ReadToEnd();
                                text_editor text_editor = new text_editor(text, "", false);
                                text_editor.Run(mode);

                            }
                        }
                        else if (text == "help")
                        {
                            Console.Clear();
                            Console.WriteLine("________________________________________________________________________________________");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("  Welcome to the E Text Editor\n  this is allso the creator made IDE for the JumpE Language ");
                            Console.ResetColor();
                            Console.WriteLine("________________________________________________________________________________________");
                        }
                        else if (text == "JumpE")
                        {
                            Console.WriteLine("https://github.com/Ethmon/jumpE");
                        }
                        else if (text == "Run JE")
                        {

                        }
                        else if (text == "JE")
                        {
                            Console.WriteLine("Loading...");
                            mode = LoadDictionary("JE_Syntax.json");
                            Console.WriteLine("Done");
                        }
                        else if (text == "N1")
                        {
                            mode = new Dictionary<string, ConsoleColor>();
                        }
                        else if (text == "end")
                        {
                            break;
                        }
                    }
                    catch { Console.WriteLine("ERROR"); }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static Dictionary<string, ConsoleColor> LoadDictionary(string filePath)
        {
            try
            {
                string json = File.ReadAllText(text_editor.getPath()+filePath);
                return JsonSerializer.Deserialize<Dictionary<string, ConsoleColor>>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

    }


    
    public class text_editor
    {
        public static string getPath()
        {
            string aa = System.IO.Directory.GetCurrentDirectory();
            aa = aa.Replace("TJE\\bin\\Debug\\net8.0", "TJE\\");
            return aa;
        }

        string full_text;
        List<char> charList;
        List<char> charListVis;
        int pos = 0;
        int SartSellection = -1;
        bool runner = true;
        string file_name;
        bool new_file;
        
        string file_type = "txt";
        Modes modes = new Modes();
        public text_editor(string text, string file_name, bool new_file)
        {
            this.full_text = text;
            this.file_name = file_name;
            this.new_file = new_file;
            charList = new List<char>(text.ToCharArray());
            charListVis = new List<char>(text.ToCharArray());

        }

        public static TextView CreateNonEditableTextField(string text, int x, int y, int width, int height)
        {
            return new TextView
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = text,
                ReadOnly = true, // Makes it non-editable
                CanFocus = true, // Prevents cursor focus
            };
        }


        public Object[] EditorWindow()
        {

            Application.Init();
            var top = Application.Top;

            var win = new Window("Non-Editable Text Field Example")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var text = CreateNonEditableTextField("This is a non-editable text field.", 0, 0, 40, 1);
            win.Add(text);

            top.Add(win);

            Application.Run();
            return new Object[] { top, win, text };
        }

        public void Run(Dictionary<string, ConsoleColor> mode)
        {
            Application.Init();
            var top = Application.Top;

            // Create the main window
            var win = new Window("Terminal GUI Editor")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            
            // Display area for text
            var textView = new TextView()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = false,
                CanFocus = false,

            };
            textView.DesiredCursorVisibility = CursorVisibility.Invisible;
            textView.CursorPosition = new Point(0, 0);
            
            win.Add(textView);

            // Handle key presses
            top.KeyDown += (args) =>
            {
                HandleKeyPress(args, textView);
            };

            

            top.Add(win);
            Application.Run();
        }
        public int FindPrevReturns() // finds the amount of previous returns in the text
        {
            int i = int.Clamp( pos,0,charList.Count()-1);
            int count = 0;
            while (i > 0)
            {
                if (charList[i] == '\n')
                {
                    count++;
                }
                i--;
            }
            return count;


        }
        public int LastReturnAway(int o)
        {
            int i = int.Clamp(o, 0, charList.Count() - 1);
            int count = 0;
            while (i > 0)
            {
                if (charList[i] != '\n')
                {
                    count++;
                }
                else break;
                i--;
            }
            return count;
        }
        public int NextReturnAway(int o)
        {
            int i = int.Clamp(o, 0, charList.Count() - 1);
            int count = 0;
            while (i < charList.Count())
            {
                if (charList[i] != '\n')
                {
                    count++;
                }
                else break;
                i++;
            }
            return count;
        }
        private void HandleKeyPress(KeyEventEventArgs args, TextView textView)
        {
            switch (args.KeyEvent.Key)
            {
                case Key.Backspace:
                    if (pos > 0)
                    {
                        charList.RemoveAt(pos - 1);
                        pos--;
                    }
                    break;
                
                case Key.Delete:
                    if (pos < charList.Count - 1)
                    {
                        charList.RemoveAt(pos);
                    }
                    break;
                case Key.CursorRight or (Key.ShiftMask | Key.CursorRight):
                    if (pos < charList.Count)
                        pos++;
                    if (args.KeyEvent.IsShift)
                    {
                        if (SartSellection == -1)
                        {
                            SartSellection = pos - 1;
                        }
                    }
                    else
                    SartSellection = -1;
                    break;
                case Key.CursorLeft or (Key.ShiftMask | Key.CursorLeft):
                    if (pos > 0)
                        pos--;
                    if (args.KeyEvent.IsShift)
                    {
                        if (SartSellection == -1)
                        {
                            SartSellection = pos + 1;
                        }
                    }
                    else
                        SartSellection = -1;
                    break;
                case Key.CtrlMask | Key.C:
                    if (pos > SartSellection)
                    {
                        string selected = string.Join("", charList.GetRange(SartSellection, pos - SartSellection));
                        Clipboard.Contents = selected;
                    }
                    else
                    {
                        string selected = string.Join("", charList.GetRange(pos, SartSellection - pos));
                        Clipboard.Contents = selected;
                    }
                    break;
                case Key.CursorUp:
                    pos -= (LastReturnAway(pos)>(LastReturnAway(pos - LastReturnAway(pos)-1)) ?(pos-LastReturnAway(pos)>0) ? LastReturnAway(pos) + 1 : pos : (pos-LastReturnAway(pos - LastReturnAway(pos)-1)>0) ? LastReturnAway(pos - LastReturnAway(pos) -1)  + 1: 0 ) ;
                    SartSellection = -1;
                    break;
                case Key.CursorDown:
                    int len = LastReturnAway(pos);
                    pos += NextReturnAway(pos)+1;
                    len = int.Clamp(len, 0, NextReturnAway(pos));
                    pos += len - 1;
                    SartSellection = -1;
                    break;
                case Key.Esc:
                    HandleEscapeCommand(textView);
                    break;
                case Key.Enter:
                    charList.Insert(pos, '\n');
                    pos++;
                    break;
                case Key.PageDown:
                    pos = charList.Count;
                    break;
                case Key.PageUp:
                    pos = 0;
                    break;
                default:
                    bool pri = true;
                    if (args.KeyEvent.IsCtrl || args.KeyEvent.IsAlt)
                        break;

                    char pressedChar = (char)args.KeyEvent.KeyValue;

                    if (args.KeyEvent.IsShift)
                    {
                        if (char.IsLetter(pressedChar))
                        {
                            // Convert lowercase letters to uppercase
                            pressedChar = char.ToUpper(pressedChar);
                        }
                        else
                        {
                            if (args.KeyEvent.IsShift && args.KeyEvent.Key == Key.ShiftMask)
                                pri = false;
                        }
                    }
                    if (pri) { 
                    charList.Insert(pos, pressedChar);
                    pos++;
                        }
                    break;

            }
            textView.CursorPosition = new Point(0, FindPrevReturns());
            
            UpdateTextView(textView);
            textView.ScrollTo(FindPrevReturns()-4);
            Application.Refresh();
            textView.ScrollTo(FindPrevReturns() - 4);
        }

        private void HandleEscapeCommand(TextView textView)
        {
            int input = MessageBox.Query("Command", "Enter command:", "quit", "save", "type", "S&Q", "help");

            if (input == 1)
            {
                string charString = string.Join("", charList);
                string filePath = new_file ? file_name : (MessageBox.Query("File Name", "Enter file name:", "default") + "." + file_type);
                File.WriteAllText(filePath, charString);
            }
            else if (input == 2)
            {
                file_type = "." + MessageBox.Query("File Type", "Enter file type:", "txt");
            }
            else if (input == 0)
            {
                
                Application.RequestStop();
                Application.Shutdown();
                Console.Clear();
            }
            else if (input == 3)
            {
                string charString = string.Join("", charList);
                string filePath = new_file ? file_name : (MessageBox.Query("File Name", "Enter file name:", "default") + "." + file_type);
                File.WriteAllText(filePath, charString);
                Application.RequestStop();
                Application.Shutdown();
                Console.Clear();
            }
            else if (input == 4)
            {
                MessageBox.Query("Help", "quit\nsave\nS&Q", "OK");
            }
        }

        private void UpdateTextView(TextView textView)
        {
            try
            {
                int i, k = 0;
                charListVis = new List<char>();
                for (i = 0; i <= charList.Count(); i++)
                {
                    if (i < this.pos)
                    {
                        charListVis.Insert(i, charList[k]);
                    }
                    else if (i == this.pos)
                    {
                        charListVis.Insert(i, '|');
                        k--;
                    }
                    else if (i > this.pos)
                    {
                        charListVis.Insert(i, charList[k]);
                    }
                    k++;
                }
            }
            catch { }
            textView.Text = string.Join("", charListVis);
            
        }

        public class Modes
        {
            public void Write(string text, Dictionary<string, ConsoleColor> modual)
            {
                int line_num = 1;
                Console.Clear();
                Thread.Sleep(10);
                List<string> colorizer_text = Tokenizer(text);
                Console.Write(line_num + ".\t");
                for (int i = 0; i < colorizer_text.Count; i++)
                {
                    Console.ResetColor();
                    if (colorizer_text[i] == "\n")
                    {
                        line_num++;
                        Console.Write("\n" + line_num + ".\t");
                    }
                    if (modual.ContainsKey(colorizer_text[i]))
                    {
                        Console.ForegroundColor = (modual[colorizer_text[i]]);
                    }
                    if (!(colorizer_text[i] == "\n"))
                    {
                        Console.Write(colorizer_text[i] + " ");
                    }


                }
                
                Console.ResetColor();
                Console.Out.Flush();

            }
            public List<string> Tokenizer(string input)
            {
                List<string> words = new List<string>();

                string[] lines = input.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Count(); i++)
                {
                    string[] tokens = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in tokens)
                    {
                        words.Add(line);
                    }
                    words.Add("\n");
                }
                return words;
            }
        }

    }
}

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
using System.IO.Pipes;

namespace Text_editor_E
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Dictionary<string, Terminal.Gui.Color> mode = new Dictionary<string, Terminal.Gui.Color>();
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
                            text_editor text_editor = new text_editor(" ", (Directory.GetCurrentDirectory()).Replace("bin\\Release\\net8.0", "Local_files\\") + name, true,mode);
                            text_editor.Run(mode);
                        }
                        else if (text == "local")
                        {
                            string allNames = string.Join("\n",Directory.EnumerateFiles((Directory.GetCurrentDirectory().Replace("bin\\Release\\net8.0", "Local_files\\"))));
                            Console.WriteLine(allNames);
                        }
                        else if (text == "inside")
                        {
                            string name = Console.ReadLine();
                            if (name == "local")
                            {
                                name = (Directory.GetCurrentDirectory()).Replace("bin\\Release\\net8.0", "Local_files\\") + Console.ReadLine();
                            }
                            if (name == "/n/")
                            {
                                break;
                            }
                            text = File.ReadAllText(name);
                            text_editor text_editor = new text_editor(" "+text, name, true,mode);
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
                                text_editor text_editor = new text_editor(text, "", false, mode);
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
                            mode = new Dictionary<string, Terminal.Gui.Color>();
                        }
                        else if (text == "end")
                        {
                            break;
                        }
                    }
                    catch(Exception e) { Console.WriteLine(e); }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static Dictionary<string, Terminal.Gui.Color> LoadDictionary(string filePath)
        {
            try
            {
                string json = File.ReadAllText(text_editor.getPath()+filePath);
                return ParseColorDictionary(JsonSerializer.Deserialize<Dictionary<string, int>>(json));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        static Dictionary<string, Terminal.Gui.Color> ParseColorDictionary(Dictionary<string, int> input)
        {
            Dictionary<string, Terminal.Gui.Color> output = new Dictionary<string, Terminal.Gui.Color>();
            foreach (KeyValuePair<string, int> entry in input)
            {
                output.Add(entry.Key, GetColorFromInt(entry.Value));
            }
            return output;
        }
        static Terminal.Gui.Color GetColorFromInt(int colorCode)
        {
            return colorCode switch
            {
                2 => Terminal.Gui.Color.Blue,
                4 => Terminal.Gui.Color.Green,
                6 => Terminal.Gui.Color.Magenta,
                9 => Terminal.Gui.Color.Cyan,
                10 => Terminal.Gui.Color.BrightRed,
                11 => Terminal.Gui.Color.Brown,
                12 => Terminal.Gui.Color.Red,
                13 => Terminal.Gui.Color.BrightYellow,
                
                _ => Terminal.Gui.Color.Black
            };
        }

    }





    public class colordString
    {
        string text;
        Terminal.Gui.Color color;
        public colordString(string text, Terminal.Gui.Color color)
        {
            this.text = text;
            this.color = color;
        }
        public string getText()
        {
            return text;
        }
        public Terminal.Gui.Color getColor()
        {
            return color;
        }
    }

    public class ColordTextView : TextView
    {
        int topy = -1;
        public int scroll = 0;
        public List<List<colordString>> sections = new List<List<colordString>>();
        public ColordTextView(colordString text,int x, int y, int width, int height, bool readOnly, bool canFoucus)
        {
            sections.Add(new List<colordString> { text });
            X = x;
            Y = y;
            if(width == -1 || height == -1)
            {
                Width = Dim.Fill();
                Height = Dim.Fill();
            }
            else
            {
                Width = width;
                Height = height;
            }
            ReadOnly = readOnly;
            CanFocus = canFoucus;
            
            
        }
        public void ClearText()
        {
            sections = new List<List<colordString>>() { new List<colordString>() { new colordString(" ", Terminal.Gui.Color.Black) } };
        }
        public void AddText(colordString text)
        {
            sections[sections.Count()-1].Add(text);
        }
        public void returnLine()
        {
            sections.Add(new List<colordString>());
        }
        public void AddText(string text)
        {
            sections[sections.Count() - 1].Add(new colordString(text, Terminal.Gui.Color.Black));
        }
        public override void Redraw(Rect rect)
        {
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Black, Color.Gray)); // Set default background
            base.Redraw(rect);
            Clear(rect);

            int posx = Bounds.X;
            int posy = Bounds.Y;
            int count = 0; ;
            // Iterate over the lines stored in sections
            foreach (List<colordString> line in sections)
            {
                int lineStartX = posx; // Store starting position to fill extra spaces later
                if (count < scroll)
                {
                    count++;
                    continue;
                }
                foreach (colordString section in line)
                {
                    
                    Terminal.Gui.Color color = section.getColor();
                    string text = section.getText();

                    Driver.SetAttribute(new Terminal.Gui.Attribute(color, Color.Gray));

                    foreach (char c in text)
                    {
                        if (posx >= Bounds.Width + Bounds.X)
                        {
                            // Move to the next line when reaching the width
                            posx = Bounds.X + 4;
                            posy++;
                        }
                        if (posy >= Bounds.Height + Bounds.Y)
                        {
                            return; // Stop drawing if out of bounds
                        }
                        if(c == '\n')
                        {
                            continue;
                        }
                        if(c == '\r')
                        {
                            continue;
                        }
                        if(c == '\t')
                        {
                            posx += 4;
                            continue;
                        }

                        Move(posx, posy);
                        Driver.AddRune(c);
                        posx++;
                    }
                }

                // Fill remaining space on the line with spaces to overwrite old content
                while (posx < Bounds.Width + Bounds.X)
                {
                    Move(posx, posy);
                    Driver.AddRune(' '); // Overwrite with spaces
                    posx++;
                }

                posy++; // Move to next line
                posx = Bounds.X; // Reset X position
            }

            // Fill remaining lines with blank spaces to ensure full overwrite
            while (posy < Bounds.Height + Bounds.Y)
            {
                Move(Bounds.X, posy);
                for (int i = 0; i < Bounds.Width; i++)
                {
                    Driver.AddRune(' ');
                }
                posy++;
            }
        }

    }












    public class text_editor
    {
        public static string getPath()
        {
            string aa = System.IO.Directory.GetCurrentDirectory();
            aa = aa.Replace("TJE\\bin\\Release\\net8.0", "TJE\\");
            return aa;
        }

        string full_text;
        List<char> charList;
        List<char> charListVis;

        int pos = 1;
        int SartSellection = -1;
        bool runner = true;
        string file_name;
        bool new_file;
        int scroll = 0;
        int default_scroll = 4;
        string file_type = "txt";
        Dictionary<string,Terminal.Gui.Color> Syntax = new Dictionary<string, Terminal.Gui.Color>();
        Modes modes = new Modes();
        public text_editor(string text, string file_name, bool new_file,Dictionary<string,Terminal.Gui.Color> syn)
        {
            this.full_text = text;
            this.file_name = file_name;
            this.new_file = new_file;
            charList = new List<char>(text.ToCharArray());
            charListVis = new List<char>(text.ToCharArray());
            Syntax = syn;

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

        public void Run(Dictionary<string, Terminal.Gui.Color> mode)
        {
            Application.Init();
            var top = Application.Top;

            // Create the main window
            var win = new Window("Terminal GUI Editor")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            
            };
            win.Border.Background = Color.White;
            win.Border.BorderBrush = Color.White;
            
            
            // Display area for text



            ColordTextView textView = new ColordTextView(new colordString(" ", Terminal.Gui.Color.Black), 1, 1, -1, -1, false, false);
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
        private void HandleKeyPress(KeyEventEventArgs args, ColordTextView textView)
        {
            bool camMove = false;
            switch (args.KeyEvent.Key)
            {
                case Key.Backspace:
                    if (pos > 1)
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
                case Key.AltMask | Key.CursorUp:
                    scroll++;
                    camMove = true;
                    break;
                case Key.CursorDown:
                    int len = LastReturnAway(pos);
                    pos += NextReturnAway(pos)+1;
                    len = int.Clamp(len, 0, NextReturnAway(pos));
                    pos += len - 1;
                    SartSellection = -1;
                    break;
                case Key.AltMask | Key.CursorDown:
                    scroll--;
                    camMove = true;
                    break;
                case Key.Esc:
                    HandleEscapeCommand(textView);
                    camMove = true;
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
            pos = int.Clamp(pos, 1, charList.Count);
            textView.CursorPosition = new Point(0, FindPrevReturns());
            if(!camMove) scroll = 0;
            textView.ClearText();
            UpdateTextView(textView);
            textView.scroll = (FindPrevReturns()-(default_scroll+scroll));
            Application.Refresh();
            textView.scroll = (FindPrevReturns() - (default_scroll+scroll));
            
        }

        private void HandleEscapeCommand(TextView textView)
        {
            int input = MessageBox.Query("Command", "Enter command:", "quit", "save", "type", "S&Q", "help","Defaul Scroll");

            if (input == 1)
            {
                string charString = string.Join("", charList);
                string filePath = new_file ? file_name : (MessageBox.Query("File Name", "Enter file name:", "default") + "." + file_type);
                File.WriteAllText(filePath, charString.Substring(1));
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
                File.WriteAllText(filePath, charString.Substring(1));
                Application.RequestStop();
                Application.Shutdown();
                Console.Clear();
            }
            else if (input == 4)
            {
                MessageBox.Query("Help", "quit\nsave\nS&Q", "OK");
            }
            else if (input == 5)
            {
                var dialog = new Dialog("Scroll Amount", 60, 10);

                var label = new Label("Enter scroll amount:")
                {
                    X = 1,
                    Y = 1,
                };
                dialog.Add(label);

                var textField = new TextField("")
                {
                    X = Pos.Right(label) + 1,
                    Y = Pos.Top(label),
                    Width = 40,
                };
                dialog.Add(textField);

                var okButton = new Button("Ok")
                {
                    X = 20,
                    Y = 4,
                    IsDefault = true,
                };
                dialog.AddButton(okButton);

                var cancelButton = new Button("Cancel")
                {
                    X = Pos.Right(okButton) + 2,
                    Y = 4,
                };
                dialog.AddButton(cancelButton);

                okButton.Clicked += () =>
                {
                    if (int.TryParse(textField.Text.ToString(), out int scrollAmount))
                    {
                        default_scroll = scrollAmount;
                        Application.RequestStop();
                    }
                    else
                    {
                        MessageBox.ErrorQuery(50, 7, "Error", "Invalid number entered", "Ok");
                    }
                };

                cancelButton.Clicked += () =>
                {
                    Application.RequestStop();
                };

                Application.Run(dialog);
            }
        }

        private void UpdateTextView(ColordTextView textView)
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
            string b = string.Join("", charListVis);
            List<string> before = Modes.Tokenizer(b);
            for(int i = 0; i < before.Count; i++)
            {
                if (before[i] == "\n")
                {
                    textView.returnLine();
                }
                else
                {
                    if (Syntax.TryGetValue(RemoveWhiteSpace(before[i]), out Terminal.Gui.Color nnn))
                    {
                        textView.AddText(new colordString(before[i], nnn));
                    }
                    else if (Double.TryParse(before[i],out double nnnn))
                    {
                        textView.AddText(new colordString(before[i], (Syntax.TryGetValue(RemoveWhiteSpace("#NUM#"), out Terminal.Gui.Color nnnnn)) ? nnnnn : Terminal.Gui.Color.Black));
                    }
                    else
                        textView.AddText(before[i]);
                }
                if (i != before.Count - 1)
                    textView.AddText(" ");
            }
            
        }
        string RemoveWhiteSpace(string text)
        {
            string newText = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != ' ' && text[i] != '\n' && text[i] != '\r' || text[i] != '\t')
                {
                    newText += text[i];
                }
            }
            return newText;
        }

        public class Modes
        {
            //public void Write(string text, Dictionary<string, ConsoleColor> modual)
            //{
            //    int line_num = 1;
            //    Console.Clear();
            //    Thread.Sleep(10);
            //    List<string> colorizer_text = Tokenizer(text);
            //    Console.Write(line_num + ".\t");
            //    for (int i = 0; i < colorizer_text.Count; i++)
            //    {
            //        Console.ResetColor();
            //        if (colorizer_text[i] == "\n")
            //        {
            //            line_num++;
            //            Console.Write("\n" + line_num + ".\t");
            //        }
            //        if (modual.ContainsKey(colorizer_text[i]))
            //        {
            //            Console.ForegroundColor = (modual[colorizer_text[i]]);
            //        }
            //        if (!(colorizer_text[i] == "\n"))
            //        {
            //            Console.Write(colorizer_text[i] + " ");
            //        }


            //    }
                
            //    Console.ResetColor();
            //    Console.Out.Flush();

            //}
            public static List<string> Tokenizer(string input)
            {
                List<string> words = new List<string>();

                List<string> lines = new List<string>() { "" };

                for(int i = 0;i<input.Length;i++)
                {
                    if (input[i] == '\n')
                    {
                        lines.Add("");
                    }
                    else
                        if (input[i] == '\r')
                            { continue; }
                        else
                            lines[lines.Count() - 1] += input[i];
                }



                for (int i = 0; i < lines.Count(); i++)
                {
                    if (lines[i] == "")
                    {
                        words.Add("\n");
                        continue;
                    }
                    List<string> tokens = new List<string>() { "" };

                    for (int k = 0; k < lines[i].Count(); k++)
                    {
                        if (lines[i][k] == ' ')
                        {
                            tokens.Add("");
                        }
                        else
                            tokens[tokens.Count() - 1] += lines[i][k];
                    }


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

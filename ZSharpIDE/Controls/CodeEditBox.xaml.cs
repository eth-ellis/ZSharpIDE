using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using ZSharpIDE.Services;

namespace ZSharpIDE.Controls
{
    public sealed partial class CodeEditBox : UserControl
    {
        private readonly SettingsService settingsService = (Application.Current as App).Container.GetService<SettingsService>();
        private readonly StateService stateService = (Application.Current as App).Container.GetService<StateService>();
        private readonly AppService appService = (Application.Current as App).Container.GetService<AppService>();

        public static readonly DependencyProperty PlainTextProperty = DependencyProperty.Register(nameof(PlainText), typeof(string), typeof(CodeEditBox), null);

        public string PlainText
        {
            get { return (string)GetValue(PlainTextProperty); }
            set { SetValue(PlainTextProperty, value); }
        }

        public static readonly DependencyProperty CodeFontSizeProperty = DependencyProperty.Register(nameof(CodeFontSize), typeof(int), typeof(CodeEditBox), null);

        public int CodeFontSize
        {
            get { return (int)GetValue(CodeFontSizeProperty); }
            set { SetValue(CodeFontSizeProperty, value); }
        }

        public CodeEditBox()
        {
            this.InitializeComponent();

            this.CodeFontSize = this.stateService.CurrentCodeFontSize;
        }

        private void RichEditBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            this.RichEditBox.Document.GetText(TextGetOptions.None, out var text);

            var caretPosition = this.RichEditBox.Document.Selection.StartPosition;

            var spacesPerTab = this.settingsService.SpacesPerTab;

            if (e.Key == VirtualKey.Tab)
            {
                var tab = new string(' ', spacesPerTab);

                var parsed = text.Insert(caretPosition, tab);

                this.RichEditBox.Document.SetText(TextSetOptions.None, parsed);

                var newCaretPosition = caretPosition + spacesPerTab;

                this.RichEditBox.Document.Selection.SetRange(newCaretPosition, newCaretPosition);

                e.Handled = true;
            }

            if (e.Key == VirtualKey.Back)
            {
                if (this.RichEditBox.Document.Selection.Length > 0)
                {
                    return;
                }

                if (caretPosition >= spacesPerTab && text.Substring(caretPosition - spacesPerTab, spacesPerTab).All(c => c == ' '))
                {
                    var parsed = text.Remove(caretPosition - spacesPerTab, spacesPerTab);

                    this.RichEditBox.Document.SetText(TextSetOptions.None, parsed);

                    var newCaretPosition = caretPosition - spacesPerTab;

                    this.RichEditBox.Document.Selection.SetRange(newCaretPosition, newCaretPosition);

                    e.Handled = true;
                }
            }

            // Left Square/Curly Bracket
            if (e.Key == (VirtualKey)219)
            {

            }

            // Right Square/Curly Bracket
            if (e.Key == (VirtualKey)221)
            {

            }
        }

        private async void RichEditBox_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control))
            {
                var delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;

                if (delta > 0)
                {
                    if (this.CodeFontSize < 40 )
                    {
                        this.CodeFontSize++;
                    }
                }
                else if (delta < 0)
                {
                    if (this.CodeFontSize > 5)
                    {
                        this.CodeFontSize--;
                    }
                }

                this.stateService.CurrentCodeFontSize = this.CodeFontSize;

                await this.UpdateLineNumbers(true);

                e.Handled = true;
            }
        }

        private async void RichEditBox_SelectionChanging(RichEditBox sender, RichEditBoxSelectionChangingEventArgs args)
        {
            await this.UpdateLineNumbers();
        }

        private async Task UpdateLineNumbers(bool invalidate = false)
        {
            await Task.Delay(10);

            if (invalidate)
            {
                this.LineNumbersBox.Children.Clear();
            }

            this.RichEditBox.Document.GetText(TextGetOptions.None, out var text);

            var cursorPosition = this.RichEditBox.Document.Selection.EndPosition;

            var newlineMatches = Regex.Matches(text, @"\r").ToList();

            var difference = newlineMatches.Count - this.LineNumbersBox.Children.Count;

            while (difference != 0)
            {
                if (difference > 0)
                {
                    var previousLineNumber = this.GetPreviousLineNumber();

                    var range = this.RichEditBox.Document.GetRange(newlineMatches[previousLineNumber].Index, newlineMatches[previousLineNumber].Length);

                    range.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Top, PointOptions.ClientCoordinates, out var point);

                    var stackPanel = this.CreateLineNumberElement(previousLineNumber + 1, point.Y);

                    this.LineNumbersBox.Children.Add(stackPanel);
                }
                else
                {
                    this.LineNumbersBox.Children.RemoveAt(this.LineNumbersBox.Children.Count - 1);
                }

                difference = newlineMatches.Count - this.LineNumbersBox.Children.Count;
            }

            // TODO - can this be improved?
            foreach (var stackPanel in this.LineNumbersBox.Children.OfType<StackPanel>().ToList())
            {
                var textBlock = stackPanel.Children.FirstOrDefault() as TextBlock;

                textBlock.Foreground = new SolidColorBrush(Colors.Gray);
            }


            // TODO - This doesn't work properly
            var currentLineIndex = newlineMatches.FindIndex(match =>
            {
                return cursorPosition >= match.Index && cursorPosition < (match.Index + match.Length);
            });

            if (currentLineIndex < 0)
            {
                return;
            }

            var currentLineStackPanel = this.LineNumbersBox.Children[currentLineIndex] as StackPanel;
            var currentLineTextBlock = currentLineStackPanel.Children.FirstOrDefault() as TextBlock;

            currentLineTextBlock.Foreground = new SolidColorBrush(Colors.White);
        }

        private int GetPreviousLineNumber()
        {
            var lastStackPanel = this.LineNumbersBox.Children.LastOrDefault() as StackPanel;
            var lineNumberTextBlock = lastStackPanel?.Children.FirstOrDefault() as TextBlock;
            return int.Parse(lineNumberTextBlock?.Text ?? "0");
        }

        private StackPanel CreateLineNumberElement(int lineNumber, double yPos)
        {
            var stackPanel = new StackPanel()
            {
                Width = 60,
            };

            var textBlock = new TextBlock()
            {
                Text = $"{lineNumber}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Gray),
                FontSize = this.CodeFontSize
            };

            stackPanel.Children.Add(textBlock);

            Canvas.SetTop(stackPanel, yPos);
            Canvas.SetLeft(stackPanel, 0);

            return stackPanel;
        }

        private void RichEditBox_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            this.RichEditBox.Document.GetText(TextGetOptions.None, out var text);

            this.RichEditBox.Document.GetRange(0, text.Length).CharacterFormat.ForegroundColor = Colors.White;

            if (this.stateService.CurrentOpenFile.Extension == ".zs" ||
                this.stateService.CurrentOpenFile.Extension == ".cs")
            {
                this.UpdateCodeFileSyntaxHighlighting(text);
            }

            if (this.stateService.CurrentOpenFile.Extension == ".zsproj" ||
                this.stateService.CurrentOpenFile.Extension == ".csproj")
            {
                this.UpdateProjectFileSyntaxHighlighting(text);
            }

            this.UpdateLayout();
        }

        private void UpdateCodeFileSyntaxHighlighting(string text)
        {
            var numberRegex = @"[-+]?\d*\.?\d+";

            var reservedKeywords = Constants.ReservedKeywords
                .Select(keyword => keyword.ToString())
                .ToArray();

            var contextualKeywords = Constants.ContextualKeywords
                .Select(keyword => keyword.ToString())
                .ToArray();

            var specialKeywords = Constants.SpecialKeywords
                .Select(keyword => keyword.ToString())
                .ToArray();

            var keywords = reservedKeywords
                .Concat(contextualKeywords)
                .Concat(specialKeywords)
                .ToArray();

            var keywordsRegex = $"\\b({string.Join("|", keywords)})\\b";

            var stringRegex = @"""([^""\\]*(\\.[^""\\]*)*)""";
            var charRegex = @"'([^'\\]*(\\.[^'\\]*)*)'";
            var directiveRegex = @"#[^\n\r]+?(?:\*\)|[\n\r])";
            var commentRegex = @"\/\/[^\n\r]+?(?:\*\)|[\n\r])";

            this.SetMatchedBraceColours(text);

            var numberMatches = Regex.Matches(text, numberRegex).ToList();
            foreach (var match in numberMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.PaleGreen;
            }

            var keywordsMatches = Regex.Matches(text, keywordsRegex).ToList();
            foreach (var match in keywordsMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.DeepSkyBlue;
            }

            var stringMatches = Regex.Matches(text, stringRegex).ToList();
            foreach (var match in stringMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.Orange;
            }

            var charMatches = Regex.Matches(text, charRegex).ToList();
            foreach (var match in charMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.Gold;
            }

            var directiveMatches = Regex.Matches(text, directiveRegex).ToList();
            foreach (var match in directiveMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.Gray;
            }

            var commentMatches = Regex.Matches(text, commentRegex).ToList();
            foreach (var match in commentMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.LimeGreen;
            }
        }

        private void UpdateProjectFileSyntaxHighlighting(string text)
        {
            var tagRegex = @"<(\/*?)+?.+?>"; // TODO - Improve

            var tagMatches = Regex.Matches(text, tagRegex).ToList();
            foreach (var match in tagMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.DeepSkyBlue;
            }
        }

        private void SetMatchedBraceColours(string text)
        {
            Stack<int> stack = new Stack<int>();
            Dictionary<int, int> bracePairs = new Dictionary<int, int>();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '{')
                {
                    stack.Push(i);
                }
                else if (c == '}')
                {
                    if (stack.Count > 0)
                    {
                        int startIndex = stack.Pop();
                        bracePairs[startIndex] = i;
                    }
                }
            }

            int x = 0;

            foreach (var pair in bracePairs)
            {
                var range1 = this.RichEditBox.Document.GetRange(pair.Key, pair.Key + 1);
                var range2 = this.RichEditBox.Document.GetRange(pair.Value, pair.Value + 1);

                if (x % 3 == 2)
                {
                    range1.CharacterFormat.ForegroundColor = Colors.MediumPurple;
                    range2.CharacterFormat.ForegroundColor = Colors.MediumPurple;

                }
                else if (x % 3 == 1)
                {
                    range1.CharacterFormat.ForegroundColor = Colors.LimeGreen;
                    range2.CharacterFormat.ForegroundColor = Colors.LimeGreen;
                }
                else
                {
                    range1.CharacterFormat.ForegroundColor = Colors.Yellow;
                    range2.CharacterFormat.ForegroundColor = Colors.Yellow;
                }

                x++;
            }
        }

        // TODO
        //private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        //{
        //    var currentOpenFile = this.OpenFiles.FirstOrDefault(file => file.FileInfo.FullName == this.stateService.CurrentOpenFile.FullName);

        //    await File.WriteAllTextAsync(currentOpenFile.FileInfo.FullName, currentOpenFile.EditContent);

        //    currentOpenFile.FileContent = currentOpenFile.EditContent;
        //}
    }
}
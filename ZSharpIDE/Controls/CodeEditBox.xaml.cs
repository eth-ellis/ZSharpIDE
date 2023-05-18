using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.System;
using ZSharpIDE.Extensions;
using ZSharpIDE.Services;

namespace ZSharpIDE.Controls
{
    public sealed partial class CodeEditBox : UserControl
    {
        private readonly StateService stateService = (Application.Current as App).Container.GetService<StateService>();

        public static readonly DependencyProperty PlainTextProperty = DependencyProperty.Register(nameof(PlainText), typeof(string), typeof(CodeEditBox), null);

        public string PlainText
        {
            get { return (string)GetValue(PlainTextProperty); }
            set { SetValue(PlainTextProperty, value); }
        }

        public CodeEditBox()
        {
            this.InitializeComponent();
        }

        private void CodeEditor_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            this.RichEditBox.Document.GetText(TextGetOptions.None, out var text);

            int caretPosition = this.RichEditBox.Document.Selection.StartPosition;

            if (e.Key == VirtualKey.Tab)
            {
                var tab = new string(' ', Constants.SpacesPerTab);

                var parsed = text.Insert(caretPosition, tab);

                this.RichEditBox.Document.SetText(TextSetOptions.None, parsed);

                var newCaretPosition = caretPosition + Constants.SpacesPerTab;

                this.RichEditBox.Document.Selection.SetRange(newCaretPosition, newCaretPosition);

                e.Handled = true;
            }

            if (e.Key == VirtualKey.Back)
            {
                if (this.RichEditBox.Document.Selection.Length > 0)
                {
                    return;
                }

                if (caretPosition >= Constants.SpacesPerTab && text.Substring(caretPosition - Constants.SpacesPerTab, Constants.SpacesPerTab).All(c => c == ' '))
                {
                    var parsed = text.Remove(caretPosition - Constants.SpacesPerTab, Constants.SpacesPerTab);

                    this.RichEditBox.Document.SetText(TextSetOptions.None, parsed);

                    var newCaretPosition = caretPosition - Constants.SpacesPerTab;

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

        private void CodeEditor_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
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
            var reservedWordRegex = $"\\b({string.Join("|", Constants.ReservedWords)})\\b";
            var directiveRegex = @"#[^\n\r]+?(?:\*\)|[\n\r])";
            var commentRegex = @"\/\/[^\n\r]+?(?:\*\)|[\n\r])";
            var stringRegex = @"""([^""\\]*(\\.[^""\\]*)*)""";
            var charRegex = @"'([^'\\]*(\\.[^'\\]*)*)'";

            this.SetMatchedBraceColours(text);

            var numberMatches = Regex.Matches(text, numberRegex).ToList();
            foreach (var match in numberMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.PaleGreen;
            }

            var reservedWordsMatches = Regex.Matches(text, reservedWordRegex).ToList();
            foreach (var match in reservedWordsMatches)
            {
                var range = this.RichEditBox.Document.GetRange(match.Index, match.Index + match.Length);

                range.CharacterFormat.ForegroundColor = Colors.DeepSkyBlue;
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
